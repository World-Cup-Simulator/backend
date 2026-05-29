using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.DTO.BracketsDTO
{
    public class AdaptativeKnockoutsOutcomeDTO
    {
        public List<KnockoutMatchDTO> NextMatches { get; set; } = [];
        public List<IMatchResult> Results { get; set; } = [];
        public List<RatingDataDTO> PreviousResults { get; set; } = [];
    }
}
