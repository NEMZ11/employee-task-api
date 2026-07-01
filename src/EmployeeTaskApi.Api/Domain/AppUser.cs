namespace EmployeeTaskApi.Api.Domain;

public sealed class AppUser
{
    public int Id { get; set; }
    public required string FullName { get; set; }
    public required string Email { get; set; }
    public required string PasswordHash { get; set; }
    public AppRole Role { get; set; } = AppRole.Employee;
    public DateTime CreatedAtUtc { get; set; } = DateTime.UtcNow;

    public ICollection<EmployeeTaskItem> AssignedTasks { get; set; } = [];
    public ICollection<EmployeeTaskItem> CreatedTasks { get; set; } = [];
}
