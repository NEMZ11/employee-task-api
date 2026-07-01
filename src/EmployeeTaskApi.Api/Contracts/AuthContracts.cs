using EmployeeTaskApi.Api.Domain;

namespace EmployeeTaskApi.Api.Contracts;

public sealed record RegisterRequest(string FullName, string Email, string Password, AppRole Role = AppRole.Employee);
public sealed record LoginRequest(string Email, string Password);
public sealed record AuthResponse(string Token, UserResponse User);
public sealed record UserResponse(int Id, string FullName, string Email, AppRole Role);
