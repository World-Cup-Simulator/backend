using Microsoft.AspNetCore.Mvc;
using WCS.Application.DTO.InsertsDTO;
using WCS.Application.DTO.UpdatesDTO;
using WCS.Domain.Entities;
using WCS.Infrastructure.Repositories;
using WCS.Infrastructure.Repositories.Interfaces;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace WCS.Api.Controllers
{
    [ApiController]
    [Route("api/admin")]
    public class AdminController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IHistoricalMatchRepository _historicalMatchRepository;
        private readonly IWorldCupFinalsRepository _worldCupFinalsRepository;
        private readonly IWorldCupMatchRepository _worldCupMatchRepository;
        private readonly INationalTeamRepository _nationalTeamRepository;
        private readonly IWorldCupTeamRepository _worldCupTeamRepository;

        public AdminController(IConfiguration configuration, IHistoricalMatchRepository historicalMatchRepository,
            IWorldCupFinalsRepository worldCupFinalsRepository, IWorldCupMatchRepository worldCupMatchRepository,
            INationalTeamRepository nationalTeamRepository, IWorldCupTeamRepository worldCupTeamRepository)
        {
            _configuration = configuration;
            _historicalMatchRepository = historicalMatchRepository;
            _worldCupFinalsRepository = worldCupFinalsRepository;
            _worldCupMatchRepository = worldCupMatchRepository;
            _nationalTeamRepository = nationalTeamRepository;
            _worldCupTeamRepository = worldCupTeamRepository;
        }

        private bool IsAuthorized([FromHeader(Name = "API-Key")] string apiKey)
        {
            return apiKey == _configuration["AdminApiKey:ApiKey"];
        }

        [HttpPost("historical-matches")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostHistoricalMatch(
            [FromHeader(Name = "API-Key")] string apiKey,
            [FromBody] List<HistoricalMatchCreateDTO> matches)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized(new { message = "Invalid API key." });

            if (matches == null || matches.Count == 0)
                return BadRequest(new { message = "Matches list cannot be empty." });

            // Validate Team Ids
            var teamIds = matches
                .SelectMany(m => new[] { m.TeamAId, m.TeamBId })
                .Distinct()
                .ToList();

            var existingIds = await _worldCupTeamRepository.GetExistingIdsAsync(teamIds);
            var invalidIds = teamIds.Except(existingIds);

            if (invalidIds.Any())
            {
                return BadRequest(new
                {
                    message = $"Invalid team ids: {string.Join(", ", invalidIds)}"
                });
            }

            // Validate Goals
            var invalidGoals = matches.Where(m => m.GoalsA < 0 || m.GoalsB < 0).ToList();
            if (invalidGoals.Any())
                return BadRequest(new { message = "Invalid goal counts detected" });

            var entities = matches.Select(dto => new HistoricalMatch
            {
                Date = dto.Date,
                GoalsA = dto.GoalsA,
                GoalsB = dto.GoalsB,
                Competition = dto.Competition,
                Stage = dto.Stage,
                TeamAId = dto.TeamAId,
                TeamBId = dto.TeamBId
            }).ToList();

            await _historicalMatchRepository.InsertListAsync(entities);

            return Ok(new { message = $"{entities.Count} historical matches inserted successfully." });
        }

        [HttpPost("finals-matches")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> PostFinalsMatch(
            [FromHeader(Name = "API-Key")] string apiKey,
            [FromBody] List<WorldCupFinalsCreateDTO> matches)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized(new { message = "Invalid API key." });

            if (matches == null || matches.Count == 0)
                return BadRequest(new { message = "Matches list cannot be empty." });

            // Validate Team Ids
            var teamIds = matches
                .SelectMany(m => new[] { m.TeamAId, m.TeamBId })
                .Distinct()
                .ToList();

            var existingIds = await _worldCupTeamRepository.GetExistingIdsAsync(teamIds);
            var invalidIds = teamIds.Except(existingIds);

            if (invalidIds.Any())
            {
                return BadRequest(new
                {
                    message = $"Invalid team ids: {string.Join(", ", invalidIds)}"
                });
            }

            // Validate Keys
            var invalidKeys = matches.Where(m => m.Key < 0 || m.NextMatchKey < 0).ToList();
            if (invalidKeys.Any())
                return BadRequest(new { message = "Invalid keys detected" });

            var entities = matches.Select(dto => new WorldCupFinals
            {
                Key = dto.Key,
                Stage = dto.Stage,
                Date = dto.Date,
                NextMatchKey = dto.NextMatchKey,
                TeamAId = dto.TeamAId,
                TeamBId = dto.TeamBId
            }).ToList();

            await _worldCupFinalsRepository.InsertListAsync(entities);
            await _worldCupFinalsRepository.SaveAsync();

            return Ok(new { message = $"{entities.Count} finals matches inserted successfully." });
        }

        [HttpPut("finals-matches")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateFinalsMatch(
            [FromHeader(Name = "API-Key")] string apiKey,
            [FromBody] List<WorldCupMatchUpdateDTO> matches)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized(new { message = "Invalid API key." });

            if (matches == null || matches.Count == 0)
                return BadRequest(new { message = "Matches list cannot be empty." });

            // Validate Match Ids
            var matchIds = matches
                .SelectMany(m => new[] { m.WorldCupMatchId })
                .Distinct()
                .ToList();

            var existingIds = await _worldCupFinalsRepository.GetExistingIdsAsync(matchIds);
            var invalidIds = matchIds.Except(existingIds);

            if (invalidIds.Any())
            {
                return BadRequest(new
                {
                    message = $"Invalid match ids: {string.Join(", ", invalidIds)}"
                });
            }

            // Validate Stats
            var invalidStats = matches.Where(m => m.GoalsA < 0 || m.GoalsB < 0).ToList();
            if (invalidStats.Any())
                return BadRequest(new { message = "Invalid stats detected" });

            await _worldCupFinalsRepository.UpdateScoresBatchAsync(matches);
            await _worldCupFinalsRepository.SaveAsync();

            return Ok(new { message = $"{matches.Count} finals matches updated successfully." });
        }

        [HttpPut("group-matches")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateWorldCupMatch(
            [FromHeader(Name = "API-Key")] string apiKey,
            [FromBody] List<WorldCupMatchUpdateDTO> matches)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized(new { message = "Invalid API key." });

            if (matches == null || matches.Count == 0)
                return BadRequest(new { message = "Matches list cannot be empty." });

            // Validate Match Ids
            var matchIds = matches
                .SelectMany(m => new[] { m.WorldCupMatchId })
                .Distinct()
                .ToList();

            var existingIds = await _worldCupMatchRepository.GetExistingIdsAsync(matchIds);
            var invalidIds = matchIds.Except(existingIds);

            if (invalidIds.Any())
            {
                return BadRequest(new
                {
                    message = $"Invalid match ids: {string.Join(", ", invalidIds)}"
                });
            }

            // Validate Goals
            var invalidGoals = matches.Where(m => m.GoalsA < 0 || m.GoalsB < 0).ToList();
            if (invalidGoals.Any())
                return BadRequest(new { message = "Invalid goal counts detected" });

            await _worldCupMatchRepository.UpdateScoresBatchAsync(matches);
            await _worldCupMatchRepository.SaveAsync();

            return Ok(new { message = $"{matches.Count} group matches updated successfully." });
        }

        [HttpPut("ratings")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateRatings(
            [FromHeader(Name = "API-Key")] string apiKey,
            [FromBody] List<NationalTeamStatsUpdateDTO> updates)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized(new { message = "Invalid API key." });

            if (updates == null || updates.Count == 0)
                return BadRequest(new { message = "Updates list cannot be empty." });

            // Validate Team Ids
            var teamIds = updates
                .SelectMany(u => new[] { u.TeamId })
                .Distinct()
                .ToList();

            var existingIds = await _nationalTeamRepository.GetExistingIdsAsync(teamIds);
            var invalidIds = teamIds.Except(existingIds);

            if (invalidIds.Any())
            {
                return BadRequest(new
                {
                    message = $"Invalid team ids: {string.Join(", ", invalidIds)}"
                });
            }

            // Validate Stats
            var invalidStats = updates.Where(t => t.AttackRating < 0 || t.AccumulatedScores < 0 || t.AccumulatedWeights < 0
            || t.DefenseRating < 0 || t.AccumulatedPenalties < 0 || t.AccumulatedCount < 0).ToList();
            if (invalidStats.Any())
                return BadRequest(new { message = "Invalid stats detected" });

            await _nationalTeamRepository.UpdateRatingsStatsBatchAsync(updates);
            await _nationalTeamRepository.SaveAsync();

            return Ok(new { message = $"{updates.Count} team ratings updated successfully." });
        }

        [HttpPut("worldcup-teams")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateWorldCupTeams(
            [FromHeader(Name = "API-Key")] string apiKey,
            [FromBody] List<WorldCupTeamUpdateDTO> updates)
        {
            if (!IsAuthorized(apiKey))
                return Unauthorized(new { message = "Invalid API key." });

            if (updates == null || updates.Count == 0)
                return BadRequest(new { message = "Updates list cannot be empty." });

            // Validate Team Ids
            var teamIds = updates
                .SelectMany(u => new[] { u.WorldCupTeamId})
                .Distinct()
                .ToList();

            var existingIds = await _worldCupTeamRepository.GetExistingIdsAsync(teamIds);
            var invalidIds = teamIds.Except(existingIds);

            if (invalidIds.Any())
            {
                return BadRequest(new
                {
                    message = $"Invalid team ids: {string.Join(", ", invalidIds)}"
                });
            }

            // Validate Points
            var invalidPoints = updates.Where(m => m.Points < 0 || m.Points > 9).ToList();
            if (invalidPoints.Any())
                return BadRequest(new { message = "Invalid points detected" });


            await _worldCupTeamRepository.UpdatePointsBatchAsync(updates);
            await _worldCupTeamRepository.SaveAsync();

            return Ok(new { message = $"{updates.Count} World Cup teams updated successfully." });
        }
    }
}
