using Application.Common.Interfaces;
using Application.Reminds.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Reminds.Commands;

public record UpdateTradeRemindCommand(int RemindId, int UserId, string TextRemind, DateTime DateTime) : ICommand;

public class UpdateTradeRemindValidator : AbstractValidator<UpdateTradeRemindCommand>
{
	public UpdateTradeRemindValidator()
	{
		RuleFor(x => x.TextRemind).NotEmpty().MaximumLength(1024);
		RuleFor(x => x.UserId).GreaterThan(0);
		RuleFor(x => x.RemindId).GreaterThan(0);
		RuleFor(x => x.DateTime).GreaterThan(DateTime.MinValue);
	}
}

public class UpdateTradeRemindCommandHandler(ITradeRemindRepository repository) : IRequestHandler<UpdateTradeRemindCommand>
{
	public async Task Handle(UpdateTradeRemindCommand request, CancellationToken cancellationToken)
	{
		var rows = await repository.UpdateUserRemindAsync(
			request.RemindId,
			request.UserId,
			request.TextRemind,
			request.DateTime,
			cancellationToken);

		if (rows == 0)
			throw new Exception("Remind not found.");
	}
}
