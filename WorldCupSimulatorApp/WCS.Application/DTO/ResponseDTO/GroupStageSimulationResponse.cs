using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.DisplaysDTO;
using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.DTO.ResponseDTO
{
    public class GroupStageSimulationResponse
    {
        public List<GroupResultDisplayDTO> Results { get; set; } = [];
        public List<GroupTableDisplayDTO> FinalStandings { get; set; } = [];
        public List<KnockoutMatchDTO> KnockoutBracket { get; set; } = [];
        public List<RatingDataDTO> RatingData { get; set; } = [];
    }
}
