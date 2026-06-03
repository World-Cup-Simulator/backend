using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.DTO.SimulatorsDTO;

namespace WCS.Application.Services.Brackets
{
    public interface IKnockoutsService
    {
        KnockoutsOutcomeDTO PerformSimpleKnockouts(List<KnockoutMatchDTO> matches,
            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulate);

        AdaptativeKnockoutsOutcomeDTO PerformAdaptativeKnockouts(List<KnockoutMatchDTO> matches, List<RatingDataDTO> previousResults);

        List<RatingDataDTO> ConvertGroupResultsToRatingData(List<GroupResultDTO> groupResults);
    }
}
