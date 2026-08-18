using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTrade.Api.Attribute;
using ViaTrade.Api.Contracts.Instruments;
using ViaTrade.Api.Mappings;
using ViaTrade.Api.Routing;
using ViaTrade.Application.Instruments.Interfaces;
using ViaTrade.Domain.Enums;

namespace ViaTrade.Api.Controllers.Internal.Analyzer;

[Route($"{ApiRoutes.V1.Analyzer}/[controller]")]
[ApiExplorerSettings(GroupName = InternalServices.Analyzer)]
[ApiController]
public class InstrumentsController(IInstrumentQueryService instrumentQueryService) : ControllerBase
{
	[ServicePassword]
	[HttpGet]
	public async Task<Ok<List<InstrumentFileResponse>>> GetFiles(CancellationToken ct)
	{
		var instruments = await instrumentQueryService.ListFileMetadataAsync(TradeDataType.Stocks, ct);

		return TypedResults.Ok(instruments.Select(ApiMapper.ToResponse).ToList());
	}

	[ServicePassword]
	[HttpGet("{instrumentId:int}")]
	public async Task<Ok<InstrumentFileResponse>> GetFileById(
		[FromRoute, Range(1, int.MaxValue)] int instrumentId,
		CancellationToken ct
	)
	{
		var instrument = await instrumentQueryService.GetFileMetadataAsync(
			TradeDataType.Stocks,
			instrumentId.ToString(),
			ct
		);

		return TypedResults.Ok(ApiMapper.ToResponse(instrument));
	}
}
