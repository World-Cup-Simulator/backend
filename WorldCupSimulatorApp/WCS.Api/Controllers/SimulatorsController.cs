using Microsoft.AspNetCore.Mvc;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.RequestDTO;
using WCS.Application.DTO.ResponseDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Application.Mappers;
using WCS.Application.Services.Brackets;
using WCS.Application.Services.Simulators;
using WCS.Domain.Enums;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class SimulatorsController : ControllerBase
    {
        private readonly IWorldCupTeamRepository _teamRepository;
        private readonly IWorldCupMatchRepository _matchRepository;
        private readonly IGroupStageService _groupStageService;
        private readonly IKnockoutsService _knockoutsService;
        private readonly ISimulationService _simulationService;

        public SimulatorsController(IWorldCupTeamRepository teamRepository, IWorldCupMatchRepository matchRepository,
            IGroupStageService groupStageService, IKnockoutsService knockoutsService, ISimulationService simulationService)
        {
            _teamRepository = teamRepository;
            _matchRepository = matchRepository;
            _groupStageService = groupStageService;
            _knockoutsService = knockoutsService;
            _simulationService = simulationService;
        }

        [HttpGet("groups")]
        [ProducesResponseType(typeof(GroupStageSimulationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SimulateGroups([FromQuery] SimulationType type)
        {
            if (!Enum.IsDefined(typeof(SimulationType), type))
                return BadRequest(new { message = $"Invalid simulation type: {type}. Valid values: {string.Join(", ", Enum.GetNames<SimulationType>())}" });

            var teams = await _teamRepository.GetAllForGroupStageAsync();
            if (teams == null || teams.Count == 0)
                return StatusCode(500, "Cannot obtain teams data");

            var groups = _groupStageService.BuildGroups(teams);
            if (groups == null || groups.Count == 0)
                return StatusCode(500, "Failed to build groups");

            var matches = await _matchRepository.GetAllForSimulationAsync();
            if (matches == null || matches.Count == 0)
                return StatusCode(500, "Cannot obtain matches data");

            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulator = type switch
            {
                SimulationType.OutcomeBased => _simulationService.SimpleSimulateGroupsStage,
                SimulationType.ScoreBased => _simulationService.SimpleSimulateGroupsStageWithScores,
                _ => throw new InvalidOperationException("Unhandled simulation type")
            };

            var results = _groupStageService.UpdateGroups(matches, groups, simulator);
            if (results == null || results.Count == 0)
                return StatusCode(500, "Failed to simulate groups");

            var knockoutBracket = _groupStageService.BuildRoundOf32(groups);
            if (knockoutBracket == null || knockoutBracket.Count == 0)
                return StatusCode(500, "Failed to create brackets");

            var ratingData = _knockoutsService.ConvertGroupResultsToRatingData(results);
            if (ratingData == null || ratingData.Count == 0)
                return StatusCode(500, "Internal Server Error");

            var response = new GroupStageSimulationResponse
            {
                Results = results.Select(DisplayMappers.MapGroupResult).ToList(),
                FinalStandings = groups.Select(DisplayMappers.MapGroupTable).ToList(),
                KnockoutBracket = knockoutBracket,
                RatingData = ratingData
            };

            return Ok(response);
        }


        [HttpGet("knockouts/simple")]
        [ProducesResponseType(typeof(KnockoutSimulationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SimulateSimpleKnockouts(
            [FromQuery] SimulationType type,
            [FromBody] List<KnockoutMatchDTO> request)
        {
            if (!Enum.IsDefined(typeof(SimulationType), type))
                return BadRequest(new { message = $"Invalid simulation type: {type}. Valid values: {string.Join(", ", Enum.GetNames<SimulationType>())}" });

            if (request is null || request.Count == 0)
                return BadRequest(new { message = "Matches list cannot be empty." });

            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulator = type switch
            {
                SimulationType.OutcomeBased => _simulationService.SimpleSimulateKnockouts,
                SimulationType.ScoreBased => _simulationService.SimpleSimulateKnockoutsWithScores,
                _ => throw new InvalidOperationException("Unhandled simulation type")
            };

            var outcome = _knockoutsService.PerformSimpleKnockouts(request, simulator);
            if (outcome == null || outcome.Results.Count == 0)
                return StatusCode(500, "Failed to simulate knockouts");

            var response = new KnockoutSimulationResponse
            {
                Results = outcome.Results.Select(DisplayMappers.MapKnockoutResult).ToList(),
                NextMatches = outcome.NextMatches,
                PreviousResults = [],
                IsFinal = outcome.NextMatches.Count == 0
            };

            return Ok(response);
        }

        [HttpGet("knockouts/adaptive")]
        [ProducesResponseType(typeof(KnockoutSimulationResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult SimulateAdaptiveKnockouts(
            [FromBody] AdaptiveKnockoutRequest request)
        {
            if (request?.Matches is null || request.Matches.Count == 0)
                return BadRequest(new { message = "Matches list cannot be empty." });

            if (request.PreviousResults is null)
                return BadRequest(new { message = "Previous results cannot be null." });

            var outcome = _knockoutsService.PerformAdaptativeKnockouts(request.Matches, request.PreviousResults);
            if (outcome == null || outcome.Results.Count == 0)
                return StatusCode(500, "Failed to simulate knockouts");

            var response = new KnockoutSimulationResponse
            {
                Results = outcome.Results.Select(DisplayMappers.MapKnockoutResult).ToList(),
                NextMatches = outcome.NextMatches,
                PreviousResults = outcome.PreviousResults,
                IsFinal = outcome.NextMatches.Count == 0
            };

            return Ok(response);
        }        
    }
}
