using FluentAssertions;
using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Application.Services.Probabilities;
using WCS.Domain.Enums;

namespace WCS.Tests.Probabilities
{
    public class MatchProbabilityServiceTest
    {
        [Theory]
        [InlineData(2.0, 1.0, 2.0)] // Attack 2 vs Defense 1 = 2 expected goals
        [InlineData(1.0, 2.0, 0.5)] // Attack 1 vs Defense 2 = 0.5 expected goals
        public void CalculateLambda_ShouldReturnCorrectRatio(double att, double def, double expected)
        {
            var service = new MatchProbabilityService();
            var result = service.CalculateLambda(att, def);
            result.Should().Be(expected);
        }

        [Fact]
        public void CalculateLambda_ShouldThrowException_WhenDefenseIsZero()
        {
            var service = new MatchProbabilityService();
            Action act = () => service.CalculateLambda(1.5, 0);
            act.Should().Throw<ArgumentException>().WithMessage("Defense rating must be greater than zero.");
        }

        [Fact]
        public void CalculateLambda_ShouldThrowException_WhenRatingsAreNegative()
        {
            var service = new MatchProbabilityService();
            Action act = () => service.CalculateLambda(-1, -1);
            act.Should().Throw<ArgumentException>().WithMessage("Ratings must be greater than zero.");
        }

        [Fact]
        public void CalculateMatchProbabilities_ShouldBeSymmetric_WhenLambdasAreEqual()
        {
            var service = new MatchProbabilityService();
            // Equal lambdas should produce symmetric win probabilities.
            var result = service.CalculateMatchProbabilities(5, 1.2, 1.2);

            result.WinA.Should().BeApproximately(result.WinB, 0.0001);
            (result.WinA + result.Draw + result.WinB).Should().BeApproximately(1.0, 0.0001);
            // With maxGoals = 5, score grid includes 0..5 for each side => 6 * 6 = 36 outcomes.
            result.Scores.Count.Should().Be(36);
        }

        [Theory]
        [InlineData(0, 0)]
        [InlineData(-1, -1)]
        public void CalculateMatchProbabilities_ShouldBeDraw_WhenLambdasAreZeroOrNegative(double lambdaA, double lambdaB)
        {
            var service = new MatchProbabilityService();
            
            var result = service.CalculateMatchProbabilities(5, lambdaA, lambdaB);

            // When no goals are possible, only 0-0 should remain.
            result.Draw.Should().BeApproximately(1.0, 0.0001);
            result.Scores.Count.Should().Be(1);
        }

        [Fact]
        public void CalculateMatchProbabilities_ShouldBeDraw_WhenMaxGoalsIsZero()
        {
            var service = new MatchProbabilityService();
            var result = service.CalculateMatchProbabilities(0, 1.5, 2);
            result.Draw.Should().BeApproximately(1.0, 0.0001);
            result.Scores.Count.Should().Be(1);
        }

        [Fact]
        public void CalculateMatchProbabilities_ShouldThrowException_WhenMaxGoalsIsNegative()
        {
            var service = new MatchProbabilityService();
            Action act = () => service.CalculateMatchProbabilities(-1, 1.5, 2);
            act.Should().Throw<ArgumentException>().WithMessage("MaxGoals must be greater than zero.");
        }

        [Fact]
        public void CalculateMatchProbabilities_ShouldCapMaxGoals_WhenInputIsTooHigh()
        {
            var service = new MatchProbabilityService();
            var result = service.CalculateMatchProbabilities(9, 1.5, 2);
            // maxGoals = 6, score grid includes 0..6 for each side => 7 * 7 = 49 outcomes.
            result.Scores.Count.Should().Be(49);
        }

        [Fact]
        public void PickRandomOutcome_ShouldFollowStatisticalDistribution()
        {
            var service = new MatchProbabilityService();
            var prob = new MatchProbabilityDTO
            {
                WinA = 0.70, // 70%
                Draw = 0.20, // 20%
                WinB = 0.10  // 10%
            };

            int countA = 0;
            int iterations = 10000;

            for (int i = 0; i < iterations; i++)
            {
                if (service.PickRandomOutcome(prob) == MatchOutcome.WinA)
                    countA++;
            }

            // 10k iterations with ±2% tolerance to absorb random variance.
            double ratioA = (double)countA / iterations;
            ratioA.Should().BeInRange(0.68, 0.72);
        }

        [Fact]
        public void PickRandomOutcome_ShouldThrowException_WhenMatchIsNull()
        {
            var service = new MatchProbabilityService();
            Action act = () => service.PickRandomOutcome(null!);
            act.Should().Throw<ArgumentException>().WithMessage("Invalid match.");
        }

        [Theory]
        [InlineData(0, 0, 0)]
        [InlineData(-1, -1, -1)]
        public void PickRandomOutcome_ShouldThrowException_WhenTotalProbabilityIsZeroOrNegative(double probA, double probB, double probDraw)
        {
            var match = new MatchProbabilityDTO 
            {
                WinA = probA,
                WinB = probB,
                Draw = probDraw
            };

            var service = new MatchProbabilityService();
            Action act = () => service.PickRandomOutcome(match);
            act.Should().Throw<ArgumentException>().WithMessage("Invalid match.");
        }

        [Fact]
        public void PickRandomScore_ShouldFollowStatisticalDistribution()
        {
            var service = new MatchProbabilityService();
            var scores = new List<ScoreProbabilityDTO>
            {
                new() { GoalsA = 1, GoalsB = 0, Probability = 0.80 }, // 80%
                new() { GoalsA = 0, GoalsB = 0, Probability = 0.20 }  // 20%
            };

            int count1_0 = 0;
            int iterations = 10000;

            for (int i = 0; i < iterations; i++)
            {
                var picked = service.PickRandomScore(scores);
                if (picked.GoalsA == 1 && picked.GoalsB == 0)
                    count1_0++;
            }

            double ratio = (double)count1_0 / iterations;
            // 10k iterations with ±2% tolerance to absorb random variance.
            ratio.Should().BeInRange(0.78, 0.82);
        }

        [Fact]
        public void PickRandomScore_ShouldThrowException_WhenListIsEmpty()
        {
            var service = new MatchProbabilityService();
            Action act = () => service.PickRandomScore([]);
            act.Should().Throw<ArgumentException>().WithMessage("Scores list is empty.");
        }
    }
}
