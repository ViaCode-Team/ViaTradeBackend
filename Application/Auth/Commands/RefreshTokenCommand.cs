using Application.Models;
using MediatR;

namespace Application.Auth.Commands;

public record RefreshTokenCommand(string RefreshToken) : IRequest<AuthInternalResult>;
