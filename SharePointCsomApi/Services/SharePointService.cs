using Microsoft.SharePoint.Client;
using PnP.Framework;
using Polly;
using Polly.Retry;

namespace SharePointCsomApi.Services;

public class SharePointService : ISharePointService
{
    private readonly ISharePointContextFactory _contextFactory;
    private readonly IConfiguration _config;
    private static bool _stressModeEnabled = false;

    public SharePointService(ISharePointContextFactory contextFactory, IConfiguration config)
    {
        _contextFactory = contextFactory;
        _config = config;
    }

    public void SetStressMode(bool enabled) => _stressModeEnabled = enabled;

    public async Task<ResilienceResult> CreateItemWithResilienceAsync(string title)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        int retryCount = 0;

        // Configuração da Política Polly: Wait and Retry com Exponential Backoff
        // Em um cenário real de SharePoint, você esperaria segundos. Aqui vamos usar ms para o Lab ser dinâmico.
        var retryPolicy = new ResiliencePipelineBuilder()
            .AddRetry(new RetryStrategyOptions
            {
                ShouldHandle = new PredicateBuilder().Handle<Exception>(ex => 
                    ex.Message.Contains("Too Many Requests") || ex.Message.Contains("429")),
                MaxRetryAttempts = 3,
                BackoffType = DelayBackoffType.Exponential,
                UseJitter = true,
                Delay = TimeSpan.FromMilliseconds(500),
                OnRetry = args =>
                {
                    retryCount++;
                    return default;
                }
            })
            .Build();

        try
        {
            await retryPolicy.ExecuteAsync(async token =>
            {
                using var context = await _contextFactory.CreateContextAsync();
                var listName = _config["SharePoint:ListName"] ?? "Tasks";
                var list = context.Web.Lists.GetByTitle(listName);

                var itemCreateInfo = new ListItemCreationInformation();
                var newItem = list.AddItem(itemCreateInfo);
                newItem["Title"] = $"{title} (Retry: {retryCount})";
                newItem.Update();

                // SIMULAÇÃO DE STRESS
                // Se o modo stress estiver ativo e for a primeira ou segunda tentativa, forçamos o erro 429
                if (_stressModeEnabled && retryCount < 2)
                {
                    throw new Exception("Artificial Throttling: HTTP 429 Too Many Requests");
                }

                await context.ExecuteQueryRetryAsync();
            });

            watch.Stop();
            return new ResilienceResult(true, retryCount, watch.ElapsedMilliseconds, "Item criado com sucesso!");
        }
        catch (Exception ex)
        {
            watch.Stop();
            return new ResilienceResult(false, retryCount, watch.ElapsedMilliseconds, $"Falha após retentativas: {ex.Message}");
        }
    }

    public async Task<List<object>> GetTasksAsync()
    {
        using var context = await _contextFactory.CreateContextAsync();

        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        var items = list.GetItems(CamlQuery.CreateAllItemsQuery());

        context.Load(items, collection => collection.Include(
            item => item.Id,
            item => item["Title"],
            item => item["Description"],
            item => item["Status"],
            item => item["DueDate"]
        ));
        
        await context.ExecuteQueryRetryAsync();

        return items.AsEnumerable().Select(i => new
        {
            Id = i.Id,
            Title = i["Title"]?.ToString() ?? string.Empty,
            Description = i["Description"]?.ToString() ?? string.Empty,
            Status = i["Status"]?.ToString() ?? string.Empty,
            DueDate = i["DueDate"]?.ToString() ?? string.Empty
        }).Cast<object>().ToList();
    }

    public async Task SeedDataAsync(int count)
    {
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        var statuses = new[] { "Pending", "In Progress", "Done", "Cancelled" };
        var random = new Random();

        // setando batchSize de 100 para evitar long-running single ExecuteQuery e lidar com timeouts
        int batchSize = 100;
        for (int i = 0; i < count; i++)
        {
            var itemCreateInfo = new ListItemCreationInformation();
            var newItem = list.AddItem(itemCreateInfo);
            newItem["Title"] = $"Task {i + 1} - Generated at {DateTime.Now:dd/MM/yyyy HH:mm}";
            newItem["Description"] = $"This is a test task for the performance lab. Item index: {i}";
            newItem["Status"] = statuses[random.Next(statuses.Length)];
            newItem["DueDate"] = DateTime.Now.AddDays(random.Next(-10, 30));
            newItem.Update();

            if ((i + 1) % batchSize == 0 || (i + 1) == count)
            {
                await context.ExecuteQueryRetryAsync();
            }
        }
    }

    public async Task<PagedResult> GetTasksPagedAsync(int pageSize, string? position)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        var query = new CamlQuery
        {
            ViewXml = $"<View><RowLimit>{pageSize}</RowLimit></View>",
            ListItemCollectionPosition = !string.IsNullOrEmpty(position)
                ? new ListItemCollectionPosition { PagingInfo = position }
                : null
        };

        var items = list.GetItems(query);
        
        // CORREÇÃO: Precisamos carregar a propriedade da coleção explicitamente
        context.Load(items, i => i.ListItemCollectionPosition);
        
        context.Load(items, collection => collection.Include(
            item => item.Id,
            item => item["Title"],
            item => item["Status"],
            item => item["Description"],
            item => item["DueDate"]
        ));

        await context.ExecuteQueryRetryAsync();
        watch.Stop();

        var resultItems = items.AsEnumerable().Select(i => new
        {
            Id = i.Id,
            Title = i["Title"]?.ToString() ?? string.Empty,
            Status = i["Status"]?.ToString() ?? string.Empty,
            Description = i["Description"]?.ToString() ?? string.Empty,
            DueDate = i["DueDate"]?.ToString() ?? string.Empty
        }).Cast<object>().ToList();

        return new PagedResult(resultItems, items.ListItemCollectionPosition?.PagingInfo, watch.ElapsedMilliseconds);
    }

    public async Task<StreamResult> GetTasksStreamAsync(int pageSize)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        var parameters = new RenderListDataParameters
        {
            ViewXml = $"<View><RowLimit>{pageSize}</RowLimit></View>",
            RenderOptions = RenderListDataOptions.ListData
        };

        var result = list.RenderListDataAsStream(parameters, new RenderListDataOverrideParameters());
        await context.ExecuteQueryRetryAsync();
        watch.Stop();

        // Fazendo o Parse do JSON retornado pelo SharePoint
        var items = new List<object>();
        try 
        {
            using var doc = System.Text.Json.JsonDocument.Parse(result.Value);
            var rows = doc.RootElement.GetProperty("Row");

            foreach (var row in rows.EnumerateArray())
            {
                items.Add(new
                {
                    Id = row.GetProperty("ID").GetString(),
                    Title = row.GetProperty("Title").GetString(),
                    Status = row.TryGetProperty("Status", out var status) ? status.GetString() : "N/A"
                });
            }
        }
        catch (Exception)
        {
            // Em caso de erro no parse, retornamos a lista vazia mas mantemos o tempo
        }
        
        return new StreamResult(items, watch.ElapsedMilliseconds);
    }

    public async Task<WritingResult> CreateItemsSequentialAsync(int count)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);
        
        var statuses = new[] { "Pending", "In Progress", "Done" };
        var random = new Random();

        try 
        {
            for (int i = 0; i < count; i++)
            {
                var itemCreateInfo = new ListItemCreationInformation();
                var newItem = list.AddItem(itemCreateInfo);
                newItem["Title"] = $"Sequential Task {i + 1} ({Guid.NewGuid().ToString().Substring(0,4)})";
                newItem["Description"] = "Criado individualmente para teste de performance.";
                newItem["Status"] = statuses[random.Next(statuses.Length)];
                newItem["DueDate"] = DateTime.Now.AddDays(random.Next(1, 30));
                newItem.Update();

                await context.ExecuteQueryRetryAsync();
            }
        }
        catch (ServerException ex)
        {
            throw new Exception($"Erro do SharePoint: {ex.Message}.");
        }

        watch.Stop();
        return new WritingResult(count, watch.ElapsedMilliseconds, "Sequential");
    }

    public async Task<WritingResult> CreateItemsBatchedAsync(int count, int batchSize = 50)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        var statuses = new[] { "Pending", "In Progress", "Done" };
        var random = new Random();

        try 
        {
            for (int i = 0; i < count; i++)
            {
                var itemCreateInfo = new ListItemCreationInformation();
                var newItem = list.AddItem(itemCreateInfo);
                newItem["Title"] = $"Batched Task {i + 1} ({Guid.NewGuid().ToString().Substring(0,4)})";
                newItem["Description"] = "Criado em lote (batching) para otimização.";
                newItem["Status"] = statuses[random.Next(statuses.Length)];
                newItem["DueDate"] = DateTime.Now.AddDays(random.Next(1, 30));
                newItem.Update();

                if ((i + 1) % batchSize == 0 || (i + 1) == count)
                {
                    await context.ExecuteQueryRetryAsync();
                }
            }
        }
        catch (ServerException ex)
        {
            throw new Exception($"Erro do SharePoint (Batch): {ex.Message}");
        }

        watch.Stop();
        return new WritingResult(count, watch.ElapsedMilliseconds, "Batched");
    }

    public async Task<List<object>> SearchTasksAsync(SearchFilters filters)
    {
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        // Construindo as cláusulas WHERE dinamicamente
        var conditions = new List<string>();

        if (!string.IsNullOrEmpty(filters.Title))
            conditions.Add($"<Contains><FieldRef Name='Title'/><Value Type='Text'>{filters.Title}</Value></Contains>");

        if (!string.IsNullOrEmpty(filters.Status))
            conditions.Add($"<Eq><FieldRef Name='Status'/><Value Type='Choice'>{filters.Status}</Value></Eq>");

        if (filters.MinDate.HasValue)
            conditions.Add($"<Geq><FieldRef Name='DueDate'/><Value Type='DateTime' IncludeTimeValue='FALSE'>{filters.MinDate.Value:yyyy-MM-ddTHH:mm:ssZ}</Value></Geq>");

        if (filters.MaxDate.HasValue)
            conditions.Add($"<Leq><FieldRef Name='DueDate'/><Value Type='DateTime' IncludeTimeValue='FALSE'>{filters.MaxDate.Value:yyyy-MM-ddTHH:mm:ssZ}</Value></Leq>");

        string camlWhere = string.Empty;
        if (conditions.Count > 0)
        {
            camlWhere = conditions[0];
            for (int i = 1; i < conditions.Count; i++)
            {
                // CAML exige que AND/OR envolvam apenas duas condições por vez
                camlWhere = $"<And>{camlWhere}{conditions[i]}</And>";
            }
            camlWhere = $"<Where>{camlWhere}</Where>";
        }

        var query = new CamlQuery { ViewXml = $"<View><Query>{camlWhere}</Query><RowLimit>50</RowLimit></View>" };
        var items = list.GetItems(query);

        context.Load(items, collection => collection.Include(
            item => item.Id,
            item => item["Title"],
            item => item["Status"],
            item => item["DueDate"]
        ));

        await context.ExecuteQueryRetryAsync();

        return items.AsEnumerable().Select(i => new
        {
            Id = i.Id,
            Title = i["Title"]?.ToString() ?? string.Empty,
            Status = i["Status"]?.ToString() ?? string.Empty,
            DueDate = i["DueDate"]?.ToString() ?? string.Empty
        }).Cast<object>().ToList();
    }

    public async Task<DeletionResult> DeleteItemsSequentialAsync(int count)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        // Buscar os IDs dos últimos N itens
        var query = new CamlQuery { ViewXml = $"<View><Query><OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy></Query><RowLimit>{count}</RowLimit></View>" };
        var items = list.GetItems(query);
        context.Load(items, i => i.Include(item => item.Id));
        await context.ExecuteQueryRetryAsync();

        foreach (var item in items.ToList())
        {
            item.Recycle(); // Envia para lixeira
            await context.ExecuteQueryRetryAsync(); // Round-trip por item
        }

        watch.Stop();
        return new DeletionResult(items.Count, watch.ElapsedMilliseconds, "Sequential");
    }

    public async Task<DeletionResult> DeleteItemsBatchedAsync(int count, int batchSize = 50)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        var query = new CamlQuery { ViewXml = $"<View><Query><OrderBy><FieldRef Name='ID' Ascending='FALSE'/></OrderBy></Query><RowLimit>{count}</RowLimit></View>" };
        var items = list.GetItems(query);
        context.Load(items, i => i.Include(item => item.Id));
        await context.ExecuteQueryRetryAsync();

        var itemList = items.ToList();
        for (int i = 0; i < itemList.Count; i++)
        {
            itemList[i].Recycle();
            if ((i + 1) % batchSize == 0 || (i + 1) == itemList.Count)
            {
                await context.ExecuteQueryRetryAsync();
            }
        }

        watch.Stop();
        return new DeletionResult(itemList.Count, watch.ElapsedMilliseconds, "Batched");
    }

    public async Task<DeletionResult> DeleteTasksByFilterAsync(SearchFilters filters)
    {
        var watch = System.Diagnostics.Stopwatch.StartNew();
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        // Construindo a mesma lógica de CAML do Search
        var conditions = new List<string>();
        if (!string.IsNullOrEmpty(filters.Title))
            conditions.Add($"<Contains><FieldRef Name='Title'/><Value Type='Text'>{filters.Title}</Value></Contains>");
        if (!string.IsNullOrEmpty(filters.Status))
            conditions.Add($"<Eq><FieldRef Name='Status'/><Value Type='Text'>{filters.Status}</Value></Eq>");
        if (filters.MinDate.HasValue)
            conditions.Add($"<Geq><FieldRef Name='DueDate'/><Value Type='DateTime' IncludeTimeValue='FALSE'>{filters.MinDate.Value:yyyy-MM-ddTHH:mm:ssZ}</Value></Geq>");
        if (filters.MaxDate.HasValue)
            conditions.Add($"<Leq><FieldRef Name='DueDate'/><Value Type='DateTime' IncludeTimeValue='FALSE'>{filters.MaxDate.Value:yyyy-MM-ddTHH:mm:ssZ}</Value></Leq>");

        string camlWhere = string.Empty;
        if (conditions.Count > 0)
        {
            camlWhere = conditions[0];
            for (int i = 1; i < conditions.Count; i++)
                camlWhere = $"<And>{camlWhere}{conditions[i]}</And>";
            camlWhere = $"<Where>{camlWhere}</Where>";
        }

        int totalDeleted = 0;
        bool hasMore = true;

        // Loop de Exaustão: Continua buscando e deletando até limpar tudo
        while (hasMore)
        {
            var query = new CamlQuery { ViewXml = $"<View><Query>{camlWhere}</Query><RowLimit>200</RowLimit></View>" };
            var items = list.GetItems(query);
            context.Load(items, i => i.Include(item => item.Id));
            await context.ExecuteQueryRetryAsync();

            var itemList = items.ToList();
            if (itemList.Count == 0)
            {
                hasMore = false;
                break;
            }

            // Deleta o lote atual
            for (int i = 0; i < itemList.Count; i++)
            {
                itemList[i].Recycle();
                if ((i + 1) % 50 == 0 || (i + 1) == itemList.Count)
                {
                    await context.ExecuteQueryRetryAsync();
                }
            }

            totalDeleted += itemList.Count;

            // Se o lote veio incompleto, significa que acabaram os itens no servidor
            if (itemList.Count < 200)
            {
                hasMore = false;
            }
        }

        watch.Stop();
        return new DeletionResult(totalDeleted, watch.ElapsedMilliseconds, "FullExhaustionBatch");
    }

    public async Task<bool> UpdateTaskAsync(TaskUpdateModel task)
    {
        using var context = await _contextFactory.CreateContextAsync();
        var listName = _config["SharePoint:ListName"] ?? "Tasks";
        var list = context.Web.Lists.GetByTitle(listName);

        var item = list.GetItemById(task.Id);
        
        item["Title"] = task.Title;
        if (task.Status != null) item["Status"] = task.Status;
        if (task.Description != null) item["Description"] = task.Description;
        if (task.DueDate.HasValue) item["DueDate"] = task.DueDate.Value;

        item.Update();
        await context.ExecuteQueryRetryAsync();
        
        return true;
    }
}
