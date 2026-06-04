using Microsoft.AspNetCore.Mvc;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorldCupMatchesController : ControllerBase
    {
        private readonly IWorldCupMatchRepository _matchRepository;

        public WorldCupMatchesController(IWorldCupMatchRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<WorldCupMatchDisplayDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            var matches = await _matchRepository.GetAllForDisplayAsync();

            if (matches.Count == 0)
                return NotFound(new { message = "No matches found." });

            return Ok(matches);
        }

        [HttpGet("group/{groupCode}")]
        [ProducesResponseType(typeof(List<WorldCupMatchDisplayDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> GetByGroup(string groupCode)
        {
            if (string.IsNullOrWhiteSpace(groupCode) || groupCode.Length > 1)
                return BadRequest(new { message = "Group code cannot be empty." });

            var matches = await _matchRepository.GetByGroupCodeAsync(groupCode);

            if (matches.Count == 0)
                return NotFound(new { message = $"No matches found for group {groupCode}." });

            return Ok(matches);
        }
    }
}
