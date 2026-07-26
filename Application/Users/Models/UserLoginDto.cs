namespace Application.Users.Models;

public sealed record UserLoginDto(int Id, string Login, string PasswordHash);
