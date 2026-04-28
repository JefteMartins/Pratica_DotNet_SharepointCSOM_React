namespace HotelAPI.Models;

public record PagedResult(List<object> Items, string? NextPosition, long ElapsedMs);
public record StreamResult(List<object> Items, long ElapsedMs);
public record WritingResult(int Count, long ElapsedMs, string Mode);
public record ResilienceResult(bool Success, int Retries, long ElapsedMs, string Message);
public record SearchFilters(string? Title, string? Status, DateTime? MinDate, DateTime? MaxDate);
public record DeletionResult(int Count, long ElapsedMs, string Mode);
public record TaskUpdateModel(int Id, string Title, string? Status, string? Description, DateTime? DueDate);
