using EmployeeTaskApi.Api.Contracts;
using EmployeeTaskApi.Api.Data;
using EmployeeTaskApi.Api.Domain;
using EmployeeTaskApi.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTaskApi.Tests;

public sealed class TaskServiceTests
{
    [Fact]
    public async Task Employee_OnlySeesCreatedOrAssignedTasks()
    {
        await using var db = CreateDbContext();
        await SeedUsersAsync(db);
        db.Tasks.AddRange(
            new EmployeeTaskItem { Title = "Own task", CreatedById = 2, AssignedToId = 2 },
            new EmployeeTaskItem { Title = "Other task", CreatedById = 1, AssignedToId = 3 });
        await db.SaveChangesAsync();
        var service = new TaskService(db);

        var tasks = await service.ListAsync(2, AppRole.Employee, CancellationToken.None);

        Assert.Single(tasks);
        Assert.Equal("Own task", tasks[0].Title);
    }

    [Fact]
    public async Task Admin_CanSeeAllTasks()
    {
        await using var db = CreateDbContext();
        await SeedUsersAsync(db);
        db.Tasks.AddRange(
            new EmployeeTaskItem { Title = "One", CreatedById = 1, AssignedToId = 2 },
            new EmployeeTaskItem { Title = "Two", CreatedById = 2, AssignedToId = 3 });
        await db.SaveChangesAsync();
        var service = new TaskService(db);

        var tasks = await service.ListAsync(1, AppRole.Admin, CancellationToken.None);

        Assert.Equal(2, tasks.Count);
    }

    [Fact]
    public async Task CreateAsync_RejectsBlankTitle()
    {
        await using var db = CreateDbContext();
        await SeedUsersAsync(db);
        var service = new TaskService(db);

        await Assert.ThrowsAsync<ArgumentException>(() =>
            service.CreateAsync(new CreateTaskRequest(" ", null, null, null), 1, CancellationToken.None));
    }

    private static AppDbContext CreateDbContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    private static async Task SeedUsersAsync(AppDbContext db)
    {
        db.Users.AddRange(
            new AppUser { Id = 1, FullName = "Admin User", Email = "admin@example.com", PasswordHash = "hash", Role = AppRole.Admin },
            new AppUser { Id = 2, FullName = "Employee One", Email = "one@example.com", PasswordHash = "hash", Role = AppRole.Employee },
            new AppUser { Id = 3, FullName = "Employee Two", Email = "two@example.com", PasswordHash = "hash", Role = AppRole.Employee });
        await db.SaveChangesAsync();
    }
}
