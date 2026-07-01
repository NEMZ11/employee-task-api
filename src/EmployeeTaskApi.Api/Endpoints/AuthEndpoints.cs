using EmployeeTaskApi.Api.Contracts;
using EmployeeTaskApi.Api.Data;
using EmployeeTaskApi.Api.Domain;
using EmployeeTaskApi.Api.Services;
using Microsoft.EntityFrameworkCore;

namespace EmployeeTaskApi.Api.Endpoints;

public static class AuthEndpoints
{
    public static RouteGroupBuilder MapAuthEndpoints(this IEndpointRouteBuilder routes)
    {
        var group = routes.MapGroup("/api/auth").WithTags("Auth");

        group.MapPost("/register", async (
            RegisterRequest request,
            AppDbContext db,
            IPasswordService passwords,
            JwtTokenService tokens,
            CancellationToken ct) =>
        {
            if (string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email) ||
                string.IsNullOrWhiteSpace(request.Password))
            {
                return Results.BadRequest(new { message = "Full name, email, and password are required." });
            }

            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            if (await db.Users.AnyAsync(user => user.Email == normalizedEmail, ct))
            {
                return Results.Conflict(new { message = "A user with this email already exists." });
            }

            var user = new AppUser
            {
                FullName = request.FullName.Trim(),
                Email = normalizedEmail,
                PasswordHash = passwords.Hash(request.Password),
                Role = request.Role
            };
            db.Users.Add(user);
            await db.SaveChangesAsync(ct);
            return Results.Created($"/api/users/{user.Id}", ToAuthResponse(user, tokens));
        });

        group.MapPost("/login", async (
            LoginRequest request,
            AppDbContext db,
            IPasswordService passwords,
            JwtTokenService tokens,
            CancellationToken ct) =>
        {
            var normalizedEmail = request.Email.Trim().ToLowerInvariant();
            var user = await db.Users.FirstOrDefaultAsync(item => item.Email == normalizedEmail, ct);
            if (user is null || !passwords.Verify(request.Password, user.PasswordHash))
            {
                return Results.Unauthorized();
            }

            return Results.Ok(ToAuthResponse(user, tokens));
        });

        return group;
    }

    private static AuthResponse ToAuthResponse(AppUser user, JwtTokenService tokens)
    {
        return new AuthResponse(
            tokens.CreateToken(user),
            new UserResponse(user.Id, user.FullName, user.Email, user.Role));
    }
}
