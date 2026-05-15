using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.Mappers;

namespace WCS.Application.Services.Brackets
{
    public class KnockoutsService
    {
        public static List<KnockoutMatchDTO> PerformSimpleKnockouts(List<KnockoutMatchDTO> matches,
            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulate)
        {
            var simulationList = KnockoutsMappers.CreateKnockoutsSimulationMatches(matches);

            var results = simulate(simulationList);

            var nextMatches = KnockoutsMappers.CreateNextKnockoutsMatches(matches, results);

            return nextMatches;
        }
    }
}
