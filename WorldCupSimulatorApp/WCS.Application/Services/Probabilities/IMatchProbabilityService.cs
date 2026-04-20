using WCS.Application.DTO.ProbabilitiesDTO;

namespace WCS.Application.Services.Probabilities
{
    public interface IMatchProbabilityService
    {
        double CalculateLambda(double attackRating, double opponentDefenseRating);

        List<ScoreProbabilityDTO> CalculateMatchProbabilities(int maxGoals, double lambdaA, double lambdaB);
    }
}
