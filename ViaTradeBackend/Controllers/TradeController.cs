using System.ComponentModel.DataAnnotations;
using Application.Interfaces;
using Application.Interfaces.Auth;
using Domain.Entities.CSV;
using Domain.Models.TradeLogic;
using Infrastructure.Repositories.DataBase;
using Infrastructure.Repositoryes.DataBase;
using Infrastructure.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ViaTradeBackend.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class TradeController(IFileReader tradefileReader, ITradeResultsService tradeResultsService, UserTradeStrategyRepository userTradeStrategyRepository, TradeStrategyRepository tradeStrategyRepository,
        IJwtHelper jwtHelper, UserService userService) : ControllerBase
    {
        private readonly IFileReader _tradefileReader = tradefileReader;
        private readonly ITradeResultsService _tradeResultsService = tradeResultsService;
        private readonly TradeStrategyRepository _tradeStrategyRepository = tradeStrategyRepository;
        private readonly UserTradeStrategyRepository _userTradeStrategyRepository = userTradeStrategyRepository;
        private readonly UserService _userService = userService;
        private readonly IJwtHelper _jwtHelper = jwtHelper;

        /// <summary>
        /// Reads strategy results for a specific trade code with date filtering.
        /// </summary>
        [HttpGet("result/strategy")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<ActionResult<StrategyResultResponse>> GetResult(
                [FromQuery] DateTime? startDate,
                [FromQuery] DateTime? endTime,
                CancellationToken cancellationToken)
        {
            var userId = _jwtHelper.GetUserIdFromClaims(User);

            // 1. Get user preferences: Strategy -> [Codes]
            var preferences = await _userTradeStrategyRepository
                .GetUserPreferencesAsync(userId, cancellationToken);

            // 2. Fetch results
            var response = await _tradeResultsService.GetStrategyResultAsync(
                userId,
                startDate,
                endTime,
                cancellationToken
            );

            return Ok(response);
        }

        /// <summary>
        /// Returns available trade codes for the specified data type.
        /// Supports optional filtering by code list.
        /// </summary>
        [HttpGet("code")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public ActionResult<IEnumerable<TradeCodeResonse>> GetCodes(
            [FromQuery, Required] TradeDataType dataType,
            [FromQuery] IEnumerable<string>? listCodes)
        {
            var tradeCodes = _tradefileReader.GetTradeCodes(dataType, listCodes);

            return Ok(tradeCodes);
        }
    }
}
