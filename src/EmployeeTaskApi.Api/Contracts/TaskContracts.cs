using ApiTaskStatus = EmployeeTaskApi.Api.Domain.TaskStatus;

namespace EmployeeTaskApi.Api.Contracts;

public sealed record CreateTaskRequest(string Title, string? Description, DateOnly? DueDate, int? AssignedToId);
public sealed record UpdateTaskRequest(string Title, string? Description, ApiTaskStatus Status, DateOnly? DueDate, int? AssignedToId);
public sealed record TaskResponse(
    int Id,
    string Title,
    string? Description,
    ApiTaskStatus Status,
    DateOnly? DueDate,
    int CreatedById,
    int? AssignedToId,
    DateTime CreatedAtUtc,
    DateTime UpdatedAtUtc);
