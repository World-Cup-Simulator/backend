using WCS.Application.DTO.SimulatorsDTO;

namespace WCS.Application.DTO.BracketsDTO
{
    public class KnockoutsOutcomeDTO
    {
        public List<KnockoutMatchDTO> NextMatches { get; set; } = new List<KnockoutMatchDTO>();
        public List<IMatchResult> Results { get; set; } = new List<IMatchResult>();
    }
}
