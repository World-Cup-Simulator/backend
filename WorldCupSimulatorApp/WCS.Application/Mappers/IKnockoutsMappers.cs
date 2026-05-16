using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.Mappers
{
    public interface IKnockoutsMappers
    {
        List<RatingDataDTO> CreatePreviousKnockoutsResults(List<KnockoutMatchDTO> matches, List<AdaptativeMatchResultDTO> results);
    }
}
