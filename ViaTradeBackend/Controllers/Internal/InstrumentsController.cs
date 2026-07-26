using System.ComponentModel.DataAnnotations;
using Application.Instruments.Interfaces;
using Domain.Enums;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using ViaTradeBackend.Attribute;
using ViaTradeBackend.Contracts.Instruments;
using ViaTradeBackend.Mappings;

namespace ViaTradeBackend.Controllers.Internal;

[Route("api/v1/internal/[controller]")]
[ApiExplorerSettings(GroupName = "internal")]
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
