using Application.Reminds.Interfaces;
using FluentValidation;
using MediatR;

namespace Application.Reminds.Commands;

public record UpdateTradeRemindCommand(int RemindId, int UserId, string TextRemind, DateTime DateTime) : IRequest;

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
		var reminds = await repository.FindAsync(x => x.Id == request.RemindId && x.UserId == request.UserId, cancellationToken);
		var remind = reminds.FirstOrDefault();

		if (remind == null)
		{
			throw new Exception("Remind not found.");
		}

		remind.Update(request.TextRemind, request.DateTime);
		repository.Update(remind);
		await repository.SaveChangesAsync(cancellationToken);
	}
}
