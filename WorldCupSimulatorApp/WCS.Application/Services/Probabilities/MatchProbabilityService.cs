using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Application.Services.Common;
using WCS.Domain.Enums;

namespace WCS.Application.Services.Probabilities
{
    public class MatchProbabilityService : IMatchProbabilityService
    {
        // Expected goals value used as Poisson mean for a team.
        public double CalculateLambda(double attackRating, double opponentDefenseRating)
        {
            if (opponentDefenseRating == 0)
                throw new ArgumentException("Defense rating must be greater than zero.");

            if (attackRating < 0 || opponentDefenseRating < 0)
                throw new ArgumentException("Ratings must be greater than zero.");

            return (attackRating / opponentDefenseRating);
        }

        // Calculates the probability for each score in the table.
        public MatchProbabilityDTO CalculateMatchProbabilities(int maxGoals, double lambdaA, double lambdaB)
        {
            if (maxGoals < 0)
                throw new ArgumentException("MaxGoals must be greater than zero.");

            if ((lambdaA <= 0 && lambdaB <= 0) || (lambdaA < 0 || lambdaB < 0))
            {
                return new MatchProbabilityDTO
                {
                    Scores =
                    {
                        new ScoreProbabilityDTO
                        {
                            GoalsA = 0,
                            GoalsB = 0,
                            Probability = 1
                        }
                    },
                    WinA = 0,
                    WinB = 0,
                    Draw = 1
                };
            }

            var scores = GenerateTable(maxGoals);
            double winA = 0;
            double draw = 0;
            double winB = 0;

            foreach (var score in scores)
            {
                var probability = CalculateProbability(lambdaA, lambdaB, score.GoalsA, score.GoalsB);
                score.Probability = probability;
            }

            var totalProbability = scores.Sum(x => x.Probability);

            foreach (var score in scores)
            {
                // Probabilities are normalized because the table is truncated at maxGoals.
                score.Probability /= totalProbability;
                if (score.GoalsA > score.GoalsB)
                {
                    winA += score.Probability;
                }
                else if (score.GoalsA < score.GoalsB)
                {
                    winB += score.Probability;
                }
                else
                {
                    draw += score.Probability;
                }
            }

            return new MatchProbabilityDTO
            {
                Scores = scores,
                WinA = winA,
                Draw = draw,
                WinB = winB
            };
        }

        // Selects one score proportionally to its probability.
        public ScoreProbabilityDTO PickRandomScore(List<ScoreProbabilityDTO> scores)
        {
            if (scores == null || scores.Count == 0)
                throw new ArgumentException("Scores list is empty.");

            double roll = Random.Shared.NextDouble();
            double cumulative = 0;

            foreach (var score in scores)
            {
                cumulative += score.Probability;

                if (roll <= cumulative)
                    return score;
            }

            return scores[^1];
        }

        // Select a winner based on their probability.
        public MatchOutcome PickRandomOutcome(MatchProbabilityDTO match)
        {
            if (match == null || (match.WinA == 0 && match.Draw == 0 && match.WinB == 0))
                throw new ArgumentException("Invalid match.");

            if (match.WinA < 0 || match.Draw < 0 || match.WinB < 0)
                throw new ArgumentException("Invalid match.");

            double roll = Random.Shared.NextDouble();

            if (roll <= match.WinA)
                return MatchOutcome.WinA;

            if (roll <= match.WinA + match.Draw)
                return MatchOutcome.Draw;

            return MatchOutcome.WinB;
        }

        // Joint probability of an exact score using independent Poisson distributions.
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

        // Generates all score combinations from 0..maxGoals for both teams.
        // Limits the matrix size and captures the most likely outcomes.
        private static List<ScoreProbabilityDTO> GenerateTable (int maxGoals)
        {
            if (maxGoals < 0)
                throw new ArgumentException("MaxGoals must be greater than zero.");

            if (maxGoals > 6)
            {
                // Caps maxGoals at 6 to optimize performance. 
                // Scores beyond 6 goals have negligible probability in professional football 
                // and significantly increase the matrix computation cost.
                maxGoals = 6;
            }

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
    }
}
