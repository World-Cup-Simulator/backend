using Microsoft.AspNetCore.Mvc;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorldCupFinalsController : ControllerBase
    {
        private readonly IWorldCupFinalsRepository _matchRepository;

        public WorldCupFinalsController(IWorldCupFinalsRepository matchRepository)
        {
            _matchRepository = matchRepository;
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<WorldCupFinalsDisplayDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAll()
        {
            var matches = await _matchRepository.GetAllForDisplayAsync();

            if (matches.Count == 0)
                return NotFound(new { message = "No matches found." });

            return Ok(matches);
        }

        [HttpGet]
        [ProducesResponseType(typeof(List<KnockoutMatchDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllForSimulation()
        {
            var matches = await _matchRepository.GetAllForSimulationAsync();

            if (matches.Count == 0)
                return NotFound(new { message = "No matches found." });

            return Ok(matches);
        }
    }
}
