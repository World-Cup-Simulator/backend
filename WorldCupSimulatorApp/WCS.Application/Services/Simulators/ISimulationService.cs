using WCS.Application.DTO.SimulatorsDTO;
using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.Services.Simulators
{
    public interface ISimulationService 
    {
        List<IMatchResult> SimpleSimulateGroupsStage(List<SimulationMatchDTO> matches);

        List<IMatchResult> SimpleSimulateKnockouts(List<SimulationMatchDTO> matches);

        List<IMatchResult> SimpleSimulateGroupsStageWithScores(List<SimulationMatchDTO> matches);

        List<IMatchResult> SimpleSimulateKnockoutsWithScores(List<SimulationMatchDTO> matches);

        List<IMatchResult> SimulateAdaptativeKnockoutsWithScores(List<SimulationMatchDTO> matches,
            List<RatingDataDTO> previousResults);
    }
}
