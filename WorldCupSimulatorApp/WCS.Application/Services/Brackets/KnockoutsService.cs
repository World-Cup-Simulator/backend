using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.SimulatorsDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Mappers;
using WCS.Application.Services.Simulators;

namespace WCS.Application.Services.Brackets
{
    public class KnockoutsService(ISimulationService simulationService) : IKnockoutsService
    {
        private readonly ISimulationService _simulationService = simulationService;

        // Performs simple knockout simulation using the specified simulator function.
        public KnockoutsOutcomeDTO PerformSimpleKnockouts(List<KnockoutMatchDTO> matches,
            Func<List<SimulationMatchDTO>, List<IMatchResult>> simulate)
        {
            ValidateMatches(matches);

            var simulationList = KnockoutsMappers.CreateKnockoutsSimulationMatches(matches);
            var results = simulate(simulationList);
            var nextMatches = KnockoutsMappers.CreateNextKnockoutsMatches(matches, results);

            return new KnockoutsOutcomeDTO
            {
                Results = results,
                NextMatches = nextMatches
            };
        }


        // Performs adaptive knockout simulation with historical results.
        public AdaptativeKnockoutsOutcomeDTO PerformAdaptativeKnockouts(List<KnockoutMatchDTO> matches, List<RatingDataDTO> previousResults)
        {
            ValidateMatches(matches);
            ValidatePreviousResults(previousResults);

            var simulationList = KnockoutsMappers.CreateKnockoutsSimulationMatches(matches);
            var results = _simulationService.SimulateAdaptativeKnockoutsWithScores(simulationList, previousResults);
            var adaptiveResults = results.OfType<AdaptativeMatchResultDTO>().ToList();
            var previousKnockoutsResults = KnockoutsMappers.CreatePreviousKnockoutsResults(matches, adaptiveResults);
            var nextMatches = KnockoutsMappers.CreateNextKnockoutsMatches(matches, results);

            return new AdaptativeKnockoutsOutcomeDTO
            {
                Results = results,
                NextMatches = nextMatches,
                PreviousResults = previousKnockoutsResults
            };
        }

        // Converts group results to rating data format for adaptive simulation.
        public List<RatingDataDTO> ConvertGroupResultsToRatingData(List<GroupResultDTO> groupResults)
        {
            if (groupResults is null)
                throw new ArgumentNullException(nameof(groupResults));
            return KnockoutsMappers.CreatePreviousResults(groupResults);
        }

        private static void ValidateMatches(List<KnockoutMatchDTO> matches)
        {
            if (matches is null)
                throw new ArgumentNullException(nameof(matches));
            if (matches.Count == 0)
                throw new ArgumentException("Matches list cannot be empty.", nameof(matches));
        }

        private static void ValidatePreviousResults(List<RatingDataDTO> previousResults)
        {
            if (previousResults is null)
                throw new ArgumentNullException(nameof(previousResults));
        }
    }
}