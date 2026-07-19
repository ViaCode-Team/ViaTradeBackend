using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using Domain.Reminds.Entities;
using FluentValidation;
using MediatR;

namespace Application.Reminds.Commands;

public record CreateTradeRemindCommand(int UserId, int TradeCodeId, string TextRemind, DateTime DateTime) : ITransactionalCommand;

public class CreateTradeRemindValidator : AbstractValidator<CreateTradeRemindCommand>
{
	public CreateTradeRemindValidator()
	{
		RuleFor(x => x.TextRemind).NotEmpty().MaximumLength(1024);
		RuleFor(x => x.UserId).GreaterThan(0);
		RuleFor(x => x.TradeCodeId).GreaterThan(0);
		RuleFor(x => x.DateTime).GreaterThan(DateTime.MinValue);
	}
}

public class CreateTradeRemindCommandHandler(ITradeRemindRepository repository) : IRequestHandler<CreateTradeRemindCommand>
{
	public async Task Handle(CreateTradeRemindCommand request, CancellationToken cancellationToken)
	{
		var remind = new TradeRemind
		{
			TextRemind = request.TextRemind,
			DateTime = request.DateTime,
			TradeCodeId = request.TradeCodeId,
			UserId = request.UserId
		};

		await repository.AddAsync(remind, cancellationToken);
	}
}
