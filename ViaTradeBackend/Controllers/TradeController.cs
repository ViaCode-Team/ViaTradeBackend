using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Domain.Entities.CSV;
using Domain.Models.TradeLogic;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TradeController(IFileReader tradefileReader, ITradeDataBuilder tradeDataBuilder) : ControllerBase
    {
        private readonly IFileReader _tradefileReader = tradefileReader;
        private readonly ITradeDataBuilder _tradeDataBuilder = tradeDataBuilder;

        /// <summary>
        /// Reads strategy results for a specific trade code with date filtering.
        /// </summary>
        [HttpGet("result/strategy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<StrategyResult>> GetResult(
            [FromQuery, Required] string tradeCode,
            [FromQuery] DateTime? startDate,
            [FromQuery] DateTime? endTime)
        {
            // Use new method: pass single code as collection
            var result = _tradefileReader.ReadDataByCodes<StrategyResult>(
                TradeDataType.Strategy,
                new[] { tradeCode },
                startDate,
                endTime
            );

            return Ok(result);
        }

        /// <summary>
        /// Returns available trade codes for the specified data type.
        /// Supports optional filtering by code list.
        /// </summary>
        [HttpGet("code/{dataTypeId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TradeCodeResonse>> GetCodes(
            [Required] int dataTypeId,
            [FromQuery] IEnumerable<string>? listCodes)
        {
            var type = (TradeDataType)dataTypeId;

            // Service handles file scanning and code extraction internally
            var tradeCodes = _tradefileReader.GetTradeCodes(type, listCodes);

            return Ok(tradeCodes);
        }
    }
}
