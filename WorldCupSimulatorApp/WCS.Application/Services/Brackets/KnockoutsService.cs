using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Mappers;
using WCS.Application.Services.Simulators;

namespace WCS.Application.Services.Brackets
{
    public class KnockoutsService(ISimulationService simulationService, IKnockoutsMappers knockoutsMappers)
    {
        private readonly ISimulationService _simulationService = simulationService;
        private readonly IKnockoutsMappers _knockoutsMappers = knockoutsMappers;

        public static KnockoutsOutcomeDTO PerformSimpleKnockouts(List<KnockoutMatchDTO> matches,
            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulate)
        {
            var outcome = new KnockoutsOutcomeDTO();

            var simulationList = KnockoutsMappers.CreateKnockoutsSimulationMatches(matches);

            var results = simulate(simulationList);

            var nextMatches = KnockoutsMappers.CreateNextKnockoutsMatches(matches, results);

            outcome.Results = results;
            outcome.NextMatches = nextMatches;

            return outcome;
        }

        public AdaptativeKnockoutsOutcomeDTO PerformAdaptativeKnockouts(List<KnockoutMatchDTO> matches, List<GroupResultDTO> groupResults,
            List<RatingDataDTO> previousResults)
        {
            var outcome = new AdaptativeKnockoutsOutcomeDTO();

            var simulationList = KnockoutsMappers.CreateKnockoutsSimulationMatches(matches);

            var results = new List<IMatchResult>();

            if (groupResults.Count > 0)
            {
                var previousGroupResults = KnockoutsMappers.CreatePreviousResults(groupResults);
                results = _simulationService.SimulateAdaptativeKnockoutsWithScores(simulationList, previousGroupResults);
            } else
            {                
                results = _simulationService.SimulateAdaptativeKnockoutsWithScores(simulationList, previousResults);
            }

            var adaptiveResults = results
                .OfType<AdaptativeMatchResultDTO>()
                .ToList();

            var previousKnockoutsResults = _knockoutsMappers.CreatePreviousKnockoutsResults(matches, adaptiveResults);           

            var nextMatches = KnockoutsMappers.CreateNextKnockoutsMatches(matches, results);

            outcome.Results = results;
            outcome.NextMatches = nextMatches;
            outcome.PreviousResults = previousKnockoutsResults;

            return outcome;
        }
    }
}
