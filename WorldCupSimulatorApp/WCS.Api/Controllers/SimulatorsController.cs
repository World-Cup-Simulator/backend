using Microsoft.AspNetCore.Mvc;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.RequestDTO;
using WCS.Application.DTO.ResponseDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Application.Services.Brackets;
using WCS.Application.Services.Simulators;
using WCS.Domain.Entities;
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
            var groups = _groupStageService.BuildGroups(teams);
            var matches = await _matchRepository.GetAllForSimulationAsync();

            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulator = type switch
            {
                SimulationType.OutcomeBased => _simulationService.SimpleSimulateGroupsStage,
                SimulationType.ScoreBased => _simulationService.SimpleSimulateGroupsStageWithScores,
                _ => throw new InvalidOperationException("Unhandled simulation type")
            };

            var results = _groupStageService.UpdateGroups(matches, groups, simulator);
            var knockoutBracket = _groupStageService.BuildRoundOf32(groups);
            var ratingData = _knockoutsService.ConvertGroupResultsToRatingData(results);

            var response = new GroupStageSimulationResponse
            {
                Results = results.Select(MapGroupResult).ToList(),
                FinalStandings = groups.Select(MapGroupTable).ToList(),
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

            var response = new KnockoutSimulationResponse
            {
                Results = outcome.Results.Select(MapKnockoutResult).ToList(),
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

            var response = new KnockoutSimulationResponse
            {
                Results = outcome.Results.Select(MapKnockoutResult).ToList(),
                NextMatches = outcome.NextMatches,
                PreviousResults = outcome.PreviousResults,
                IsFinal = outcome.NextMatches.Count == 0
            };

            return Ok(response);
        }

        private static GroupResultDisplayDTO MapGroupResult(GroupResultDTO result)
        {
            return new GroupResultDisplayDTO
            {
                GroupCode = result.GroupCode,
                TeamA = result.TeamA,
                TeamB = result.TeamB,
                GoalsA = result.GoalsA,
                GoalsB = result.GoalsB,
                Winner = result.Winner,
                Date = result.Date,
                OutcomeProbability = result.OutcomeProbability,
                ScoreProbability = result.ScoreProbability,
                DecidedByPenalties = result.DecidedByPenalties
            };
        }

        private static GroupTableDisplayDTO MapGroupTable(GroupTable group)
        {
            return new GroupTableDisplayDTO
            {
                GroupCode = group.GroupCode,
                Teams = group.Teams.Select(t => new GroupTableTeamDisplayDTO
                {
                    Name = t.Name,
                    Points = t.Points,
                    GoalsScored = t.GoalsScored,
                    GoalsConceded = t.GoalsConceded
                }).ToList()
            };
        }

        private static KnockoutResultDisplayDTO MapKnockoutResult(IMatchResult result)
        {
            var display = new KnockoutResultDisplayDTO
            {
                TeamA = result.TeamA,
                TeamB = result.TeamB,
                Winner = result.Winner,
                OutcomeProbability = result.OutcomeProbability
            };

            if (result is IScoreResult scoreResult)
            {
                display.GoalsA = scoreResult.GoalsA;
                display.GoalsB = scoreResult.GoalsB;
                display.ScoreProbability = scoreResult.ScoreProbability;
                display.DecidedByPenalties = scoreResult.DecidedByPenalties;
            }

            return display;
        }
    }
}
