namespace SharePointCsomApi.Services;

public record PagedResult(List<object> Items, string? NextPosition, long ElapsedMs);
public record StreamResult(List<object> Items, long ElapsedMs);

public record WritingResult(int Count, long ElapsedMs, string Mode);

public interface ISharePointService
{
    /// <summary>
    /// Recupera tarefas da lista de forma básica (CSOM padrão).
    /// </summary>
    Task<List<object>> GetTasksAsync();

    /// <summary>
    /// Endpoint para popular a lista com dados de teste para o Lab.
    /// </summary>
    /// <param name="count">Quantidade de itens a gerar.</param>
    Task SeedDataAsync(int count);

    /// <summary>
    /// Recupera tarefas usando paginação clássica do CSOM (ListItemCollectionPosition).
    /// </summary>
    Task<PagedResult> GetTasksPagedAsync(int pageSize, string? position);

    /// <summary>
    /// Recupera tarefas usando a API moderna RenderListDataAsStream.
    /// </summary>
    Task<StreamResult> GetTasksStreamAsync(int pageSize);

    /// <summary>
    /// Cria itens um por um, executando uma query por item (Modo Ineficiente).
    /// </summary>
    Task<WritingResult> CreateItemsSequentialAsync(int count);

    /// <summary>
    /// Cria itens em blocos, reduzindo round-trips (Modo Eficiente).
    /// </summary>
    Task<WritingResult> CreateItemsBatchedAsync(int count, int batchSize = 50);
}
