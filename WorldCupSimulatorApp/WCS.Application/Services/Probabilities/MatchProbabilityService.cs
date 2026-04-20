using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Application.Services.Common;

namespace WCS.Application.Services.Probabilities
{
    public class MatchProbabilityService : IMatchProbabilityService
    {
        public double CalculateLambda(double attackRating, double opponentDefenseRating)
        {
            if (opponentDefenseRating == 0)
                throw new ArgumentException("Defense rating must be greater than zero.");

            return (attackRating / opponentDefenseRating);
        }

        private static double CalculateProbability (double lambdaA, double lambdaB, int goalsA, int goalsB)
        {
            if (goalsA < 0 || goalsB < 0)
                throw new ArgumentException("Goals cannot be negative.");

            var factorialA = MathHelpers.CalculateFactorial(goalsA);
            var factorialB = MathHelpers.CalculateFactorial(goalsB);

            double probGoalsA = 
                (Math.Exp(-lambdaA) *
                Math.Pow(lambdaA, goalsA))
                / factorialA;

            double probGoalsB = 
                (Math.Exp(-lambdaB) *
                Math.Pow(lambdaB, goalsB))
                / factorialB;
            
            return probGoalsA * probGoalsB;
        }

        private static List<ScoreProbabilityDTO> GenerateTable (int maxGoals)
        {
            if (maxGoals < 0)
                throw new ArgumentException("maxGoals cannot be negative.");

            var scores = new List<ScoreProbabilityDTO>();

            for (int i = 0; i <= maxGoals; i++)
            {
                for (int j = 0; j <= maxGoals; j++)
                {
                    var score = new ScoreProbabilityDTO
                    {
                        GoalsA = i,
                        GoalsB = j,
                    };

                    scores.Add(score);
                }                
            }

            return scores;
        }

        public List<ScoreProbabilityDTO> CalculateMatchProbabilities (int maxGoals, double lambdaA, double lambdaB)
        {
            if (maxGoals < 0)
                throw new ArgumentException("maxGoals cannot be negative.");

            var scores = GenerateTable(maxGoals);

            foreach (var score in scores)
            {
                var probability = CalculateProbability(lambdaA, lambdaB, score.GoalsA, score.GoalsB);
                score.Probability = probability;
            }

            var totalProbability = scores.Sum(x => x.Probability);

            foreach (var score in scores)
            {
                score.Probability /= totalProbability;
            }

            return scores;
        }
    }
}
