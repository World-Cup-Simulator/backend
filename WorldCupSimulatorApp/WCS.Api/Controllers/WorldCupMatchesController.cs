using Microsoft.AspNetCore.Mvc;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.Services.Brackets;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorldCupMatchesController : ControllerBase
    {
        private readonly IWorldCupMatchRepository _matchRepository;
        private readonly IBracketThirdPlaceService _thirdPlaceService;

        public WorldCupMatchesController(IWorldCupMatchRepository matchRepository, IBracketThirdPlaceService thirdPlaceService)
        {
            _matchRepository = matchRepository;
            _thirdPlaceService = thirdPlaceService;
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

        // Assigns third-place teams to bracket slots.
        [HttpPost("third-places")]
        [ProducesResponseType(typeof(List<ThirdPlaceAssignmentDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public IActionResult AssignThirdPlaces([FromBody] List<ThirdPlaceInputDTO> request)
        {
            if (request == null || request.Count != 8)
                return BadRequest(new { message = "Exactly 8 third-place teams required." });

            var invalidIndices = request.Where(r => r.Index < 0 || r.Index > 7).ToList();
            if (invalidIndices.Any())
                return BadRequest(new { message = "Indices must be between 0 and 7." });

            var distinctIndices = request.Select(r => r.Index).Distinct().Count();
            if (distinctIndices != 8)
                return BadRequest(new { message = "All indices must be unique." });

            try
            {
                var assignments = _thirdPlaceService.AssignThirdPlaces(request);
                return Ok(assignments);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
