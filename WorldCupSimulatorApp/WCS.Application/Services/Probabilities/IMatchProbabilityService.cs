using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Probabilities
{
    public interface IMatchProbabilityService
    {
        double CalculateLambda(double attackRating, double opponentDefenseRating);

        MatchProbabilityDTO CalculateMatchProbabilities(int maxGoals, double lambdaA, double lambdaB);

        ScoreProbabilityDTO PickRandomScore(List<ScoreProbabilityDTO> scores);

        MatchOutcome PickRandomOutcome(MatchProbabilityDTO match);
    }
}
