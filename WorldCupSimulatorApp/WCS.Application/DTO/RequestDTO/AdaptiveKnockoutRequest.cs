using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.DTO.RequestDTO
{
    /// <summary>
    /// Request body for adaptive knockout simulation.
    /// </summary>
    public class AdaptiveKnockoutRequest
    {
        public List<KnockoutMatchDTO> Matches { get; set; } = [];
        public List<RatingDataDTO> PreviousResults { get; set; } = [];
    }
}
