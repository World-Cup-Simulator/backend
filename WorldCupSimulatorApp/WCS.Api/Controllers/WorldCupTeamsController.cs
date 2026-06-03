using Microsoft.AspNetCore.Mvc;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Infrastructure.Repositories.Interfaces;

namespace WCS.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorldCupTeamsController : ControllerBase
    {
        private readonly IWorldCupTeamRepository _teamRepository;

        public WorldCupTeamsController(IWorldCupTeamRepository teamRepository)
        {
            _teamRepository = teamRepository;
        }

        [HttpGet("groups")]
        [ProducesResponseType(typeof(List<GroupSeedDisplayDTO>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetAllGroups()
        {
            var groups = await _teamRepository.GetAllGroupsForDisplayAsync();

            if (groups.Count == 0)
                return NotFound(new { message = "No groups found." });

            return Ok(groups);
        }
    }
}
