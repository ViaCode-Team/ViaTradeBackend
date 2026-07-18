using Application.Models;
using MediatR;

namespace Application.Auth.Commands;

public record LoginCommand(string Login, string Password, string UserAgent) : IRequest<AuthInternalResult>;
