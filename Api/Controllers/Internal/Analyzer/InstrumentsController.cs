using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attribute;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Common.Abstractions;
using ViaTrade.Application.Instruments.Common;
using ViaTrade.Application.Instruments.GetFile;
using ViaTrade.Application.Instruments.ListFiles;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Api.Controllers.Internal.Analyzer;

[Route($"{ApiRoutes.V1.Analyzer}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.Analyzer)]
[ApiController]
public class InstrumentsController : ControllerBase
{
	[ServicePassword]
	[HttpGet]
	public async Task<Ok<List<InstrumentFileResult>>> GetFiles(
		[FromServices] IQueryHandler<ListInstrumentFilesQuery, IReadOnlyList<InstrumentFileResult>> handler,
		CancellationToken ct
	)
	{
		var instruments = await handler.HandleAsync(new ListInstrumentFilesQuery(TradeDataType.Stocks), ct);

		return TypedResults.Ok(instruments.ToList());
	}

	[ServicePassword]
	[HttpGet("{instrumentId:int}")]
	public async Task<Ok<InstrumentFileResult>> GetFileById(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		[FromServices] IQueryHandler<GetInstrumentFileQuery, InstrumentFileResult> handler,
		CancellationToken ct
	)
	{
		var instrument = await handler.HandleAsync(
			new GetInstrumentFileQuery(TradeDataType.Stocks, instrumentId.ToString()),
			ct
		);

		return TypedResults.Ok(instrument);
	}
}
