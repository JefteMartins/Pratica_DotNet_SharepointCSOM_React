namespace SharePointCsomApi.Services;

public record PagedResult(List<object> Items, string? NextPosition, long ElapsedMs);
public record StreamResult(List<object> Items, long ElapsedMs);
public record WritingResult(int Count, long ElapsedMs, string Mode);
public record ResilienceResult(bool Success, int Retries, long ElapsedMs, string Message);
public record SearchFilters(string? Title, string? Status, DateTime? MinDate, DateTime? MaxDate);
public record DeletionResult(int Count, long ElapsedMs, string Mode);
public record TaskUpdateModel(int Id, string Title, string? Status, string? Description, DateTime? DueDate);

public interface ISharePointService
{
    Task<List<object>> GetTasksAsync();
    Task SeedDataAsync(int count);
    Task<PagedResult> GetTasksPagedAsync(int pageSize, string? position);
    Task<StreamResult> GetTasksStreamAsync(int pageSize);
    Task<WritingResult> CreateItemsSequentialAsync(int count);
    Task<WritingResult> CreateItemsBatchedAsync(int count, int batchSize = 50);
    void SetStressMode(bool enabled);
    Task<ResilienceResult> CreateItemWithResilienceAsync(string title);
    Task<List<object>> SearchTasksAsync(SearchFilters filters);
    Task<DeletionResult> DeleteItemsSequentialAsync(int count);
    Task<DeletionResult> DeleteItemsBatchedAsync(int count, int batchSize = 50);
    Task<DeletionResult> DeleteTasksByFilterAsync(SearchFilters filters);
    
    /// <summary>
    /// Atualiza as informações de uma tarefa existente.
    /// </summary>
    Task<bool> UpdateTaskAsync(TaskUpdateModel task);
}
