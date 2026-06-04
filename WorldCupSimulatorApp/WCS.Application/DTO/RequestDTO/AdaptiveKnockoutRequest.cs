using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.DTO.RequestDTO
{
    // Request body for adaptive knockout simulation.
    public class AdaptiveKnockoutRequest
    {
        public List<KnockoutMatchDTO> Matches { get; set; } = [];
        public List<RatingDataDTO> PreviousResults { get; set; } = [];
    }
}
