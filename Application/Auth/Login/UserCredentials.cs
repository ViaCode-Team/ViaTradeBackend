namespace ViaTrade.Application.Auth.Login;

public sealed record UserCredentials(int Id, string Login, string PasswordHash);
