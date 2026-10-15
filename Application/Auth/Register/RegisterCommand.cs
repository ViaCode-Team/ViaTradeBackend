using Mediator;
using ViaTrade.Application.Email;

namespace ViaTrade.Application.Auth.Register;

public sealed record RegisterCommand(string Login, string Email, string Password) : ICommand<EmailAcceptedResult>;
