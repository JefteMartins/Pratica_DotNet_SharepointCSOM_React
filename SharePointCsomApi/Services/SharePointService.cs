using Microsoft.SharePoint.Client;
using PnP.Framework;

namespace SharePointCsomApi.Services;

public class SharePointService : ISharePointService
{
    private readonly ISharePointContextFactory _contextFactory;
    private readonly IConfiguration _config;

    public SharePointService(ISharePointContextFactory contextFactory, IConfiguration config)
    {
        _contextFactory = contextFactory;
        _config = config;
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
            item => item["Status"]
        ));

        await context.ExecuteQueryRetryAsync();
        watch.Stop();

        var resultItems = items.AsEnumerable().Select(i => new
        {
            Id = i.Id,
            Title = i["Title"]?.ToString() ?? string.Empty,
            Status = i["Status"]?.ToString() ?? string.Empty
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
}
