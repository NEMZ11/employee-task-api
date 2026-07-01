namespace EmployeeTaskApi.Api.Domain;

public sealed class EmployeeTaskItem
{
    public int Id { get; set; }
    public required string Title { get; set; }
    public string? Description { get; set; }
    public TaskStatus Status { get; set; } = TaskStatus.Todo;
    public DateOnly? DueDate { get; set; }
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAtUtc { get; set; } = DateTime.UtcNow;

    public int CreatedById { get; set; }
    public AppUser? CreatedBy { get; set; }

    public int? AssignedToId { get; set; }
    public AppUser? AssignedTo { get; set; }
}
