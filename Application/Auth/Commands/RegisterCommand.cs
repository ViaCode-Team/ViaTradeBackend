using Application.Models;
using MediatR;

namespace Application.Auth.Commands;

public record RegisterCommand(string Login, string Password) : IRequest<AuthInternalResult>;
