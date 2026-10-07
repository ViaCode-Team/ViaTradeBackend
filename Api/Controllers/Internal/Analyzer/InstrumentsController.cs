using System.ComponentModel.DataAnnotations;
using Mediator;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Routing;
using ViaTrade.Api.Security.Authorization;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Application.Instruments.GetFile;
using ViaTrade.Application.Instruments.ListFiles;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Api.Controllers.Internal.Analyzer;

[Route($"{ApiRoutes.V1.Analyzer}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.Analyzer)]
[ApiController]
public class InstrumentsController(ISender sender) : ControllerBase
{
	[ServicePassword]
	[HttpGet]
	public async Task<Ok<List<InstrumentFileResult>>> GetFiles(CancellationToken ct)
	{
		var instruments = await sender.Send(new ListInstrumentFilesQuery(TradeDataType.Stocks), ct);

		return TypedResults.Ok(instruments.ToList());
	}

	[ServicePassword]
	[HttpGet("{instrumentId:int}")]
	public async Task<Ok<InstrumentFileResult>> GetFileById(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		CancellationToken ct
	)
	{
		var instrument = await sender.Send(
			new GetInstrumentFileQuery(TradeDataType.Stocks, instrumentId.ToString()),
			ct
		);

		return TypedResults.Ok(instrument);
	}
}
