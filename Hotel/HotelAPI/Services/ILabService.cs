using HotelAPI.Models;

namespace HotelAPI.Services;

public interface ILabService
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
    Task<List<object>> GetFieldMappingsAsync();
    Task<DeletionResult> DeleteItemsSequentialAsync(int count);
    Task<DeletionResult> DeleteItemsBatchedAsync(int count, int batchSize = 50);
    Task<DeletionResult> DeleteTasksByFilterAsync(SearchFilters filters);
    Task<bool> UpdateTaskAsync(TaskUpdateModel task);
}
