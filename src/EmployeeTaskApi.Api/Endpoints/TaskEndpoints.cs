using System.Security.Claims;
using EmployeeTaskApi.Api.Contracts;
using EmployeeTaskApi.Api.Domain;
using EmployeeTaskApi.Api.Services;

namespace EmployeeTaskApi.Api.Endpoints;

public static class TaskEndpoints
{
    public static RouteGroupBuilder MapTaskEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/tasks")
            .RequireAuthorization()
            .WithTags("Tasks");

        group.MapGet("/", async (ClaimsPrincipal user, TaskService tasks, CancellationToken ct) =>
        {
            var current = CurrentUser.FromClaims(user);
            return Results.Ok(await tasks.ListAsync(current.UserId, current.Role, ct));
        });

        group.MapGet("/{id:int}", async (int id, ClaimsPrincipal user, TaskService tasks, CancellationToken ct) =>
        {
            var current = CurrentUser.FromClaims(user);
            var task = await tasks.GetAsync(id, current.UserId, current.Role, ct);
            return task is null ? Results.NotFound() : Results.Ok(task);
        });

        group.MapPost("/", async (CreateTaskRequest request, ClaimsPrincipal user, TaskService tasks, CancellationToken ct) =>
        {
            var current = CurrentUser.FromClaims(user);
            try
            {
                var task = await tasks.CreateAsync(request, current.UserId, ct);
                return Results.Created($"/api/tasks/{task.Id}", task);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        group.MapPut("/{id:int}", async (int id, UpdateTaskRequest request, ClaimsPrincipal user, TaskService tasks, CancellationToken ct) =>
        {
            var current = CurrentUser.FromClaims(user);
            try
            {
                var task = await tasks.UpdateAsync(id, request, current.UserId, current.Role, ct);
                return task is null ? Results.NotFound() : Results.Ok(task);
            }
            catch (Exception ex) when (ex is ArgumentException or InvalidOperationException)
            {
                return Results.BadRequest(new { message = ex.Message });
            }
        });

        group.MapDelete("/{id:int}", async (int id, ClaimsPrincipal user, TaskService tasks, CancellationToken ct) =>
        {
            var current = CurrentUser.FromClaims(user);
            return await tasks.DeleteAsync(id, current.UserId, current.Role, ct)
                ? Results.NoContent()
                : Results.NotFound();
        });

        return group;
    }

    private sealed record CurrentUser(int UserId, AppRole Role)
    {
        public static CurrentUser FromClaims(ClaimsPrincipal principal)
        {
            var userId = int.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");
            var role = Enum.Parse<AppRole>(principal.FindFirstValue(ClaimTypes.Role) ?? AppRole.Employee.ToString());
            return new CurrentUser(userId, role);
        }
    }
}
