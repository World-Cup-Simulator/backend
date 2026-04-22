using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.Services.Simulators
{
    public interface I  
    {
        List<SimpleMatchResultDTO> SimpleSimulateGroupsStage(List<SimpleSimulationMatchDTO> matches);

        List<SimpleMatchResultDTO> SimpleSimulateKnockouts(List<SimpleSimulationMatchDTO> matches);

        List<MatchResultDTO> SimpleSimulateGroupsStageWithScores(List<SimpleSimulationMatchDTO> matches);

        List<MatchResultDTO> SimpleSimulateKnockoutsWithScores(List<SimpleSimulationMatchDTO> matches);

        List<AdaptativeMatchResultDTO> SimulateAdaptativeKnockoutsWithScores(List<AdaptativeSimulationMatchDTO> matches,
            List<RatingDataDTO> previousResults);
    }
}
