using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.DTO.ResponseDTO
{
    public class KnockoutSimulationResponse
    {
        public List<KnockoutResultDisplayDTO> Results { get; set; } = [];
        public List<KnockoutMatchDTO> NextMatches { get; set; } = [];
        public List<RatingDataDTO> PreviousResults { get; set; } = [];
        public bool IsFinal { get; set; }
    }
}
