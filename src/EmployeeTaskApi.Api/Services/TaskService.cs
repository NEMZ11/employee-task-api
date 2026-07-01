using EmployeeTaskApi.Api.Contracts;
using EmployeeTaskApi.Api.Data;
using EmployeeTaskApi.Api.Domain;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTaskApi.Api.Services;

public sealed class TaskService(AppDbContext db)
{
    public async Task<IReadOnlyList<TaskResponse>> ListAsync(int userId, AppRole role, CancellationToken ct)
    {
        var query = db.Tasks.AsNoTracking();
        if (role != AppRole.Admin)
        {
            query = query.Where(task => task.AssignedToId == userId || task.CreatedById == userId);
        }

        return await query.OrderBy(task => task.DueDate).ThenBy(task => task.Id)
            .Select(task => ToResponse(task))
            .ToListAsync(ct);
    }

    public async Task<TaskResponse?> GetAsync(int id, int userId, AppRole role, CancellationToken ct)
    {
        var task = await db.Tasks.AsNoTracking().FirstOrDefaultAsync(item => item.Id == id, ct);
        return task is null || !CanAccess(task, userId, role) ? null : ToResponse(task);
    }

    public async Task<TaskResponse> CreateAsync(CreateTaskRequest request, int createdById, CancellationToken ct)
    {
        ValidateTitle(request.Title);
        await EnsureUserExistsAsync(createdById, ct);
        if (request.AssignedToId is not null)
        {
            await EnsureUserExistsAsync(request.AssignedToId.Value, ct);
        }

        var task = new EmployeeTaskItem
        {
            Title = request.Title.Trim(),
            Description = request.Description?.Trim(),
            DueDate = request.DueDate,
            AssignedToId = request.AssignedToId,
            CreatedById = createdById
        };

        db.Tasks.Add(task);
        await db.SaveChangesAsync(ct);
        return ToResponse(task);
    }

    public async Task<TaskResponse?> UpdateAsync(int id, UpdateTaskRequest request, int userId, AppRole role, CancellationToken ct)
    {
        ValidateTitle(request.Title);
        var task = await db.Tasks.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (task is null || !CanAccess(task, userId, role))
        {
            return null;
        }

        if (request.AssignedToId is not null)
        {
            await EnsureUserExistsAsync(request.AssignedToId.Value, ct);
        }

        task.Title = request.Title.Trim();
        task.Description = request.Description?.Trim();
        task.Status = request.Status;
        task.DueDate = request.DueDate;
        task.AssignedToId = request.AssignedToId;
        task.UpdatedAtUtc = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
        return ToResponse(task);
    }

    public async Task<bool> DeleteAsync(int id, int userId, AppRole role, CancellationToken ct)
    {
        var task = await db.Tasks.FirstOrDefaultAsync(item => item.Id == id, ct);
        if (task is null || !CanAccess(task, userId, role))
        {
            return false;
        }

        db.Tasks.Remove(task);
        await db.SaveChangesAsync(ct);
        return true;
    }

    private static bool CanAccess(EmployeeTaskItem task, int userId, AppRole role)
    {
        return role == AppRole.Admin || task.CreatedById == userId || task.AssignedToId == userId;
    }

    private async Task EnsureUserExistsAsync(int userId, CancellationToken ct)
    {
        if (!await db.Users.AnyAsync(user => user.Id == userId, ct))
        {
            throw new InvalidOperationException($"User {userId} was not found.");
        }
    }

    private static void ValidateTitle(string title)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Task title is required.", nameof(title));
        }
    }

    private static TaskResponse ToResponse(EmployeeTaskItem task)
    {
        return new TaskResponse(
            task.Id,
            task.Title,
            task.Description,
            task.Status,
            task.DueDate,
            task.CreatedById,
            task.AssignedToId,
            task.CreatedAtUtc,
            task.UpdatedAtUtc);
    }
}
