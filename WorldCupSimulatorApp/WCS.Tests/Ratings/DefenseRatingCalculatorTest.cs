using FluentAssertions;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Services.Ratings;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Tests.Ratings
{
    public class DefenseRatingCalculatorTest
    {
        private readonly RatingWeightsOptions _mockWeights;

        public DefenseRatingCalculatorTest()
        {
            // Configure test weights
            _mockWeights = new RatingWeightsOptions
            {
                Competition = new Dictionary<string, double> { { "WorldCup", 1.40 }, { "Friendly", 0.85 } },
                Stage = new Dictionary<string, double> { { "Group", 1.00 }, { "Final", 1.30 } }
            };
        }

        [Fact]
        public void Calculate_ShouldReturnEmptyDTO_WhenDataIsInvalid()
        {
            var resultNull = DefenseRatingCalculator.Calculate(null!, _mockWeights, 0, 0);
            var resultEmpty = DefenseRatingCalculator.Calculate([], _mockWeights, 0, 0);

            resultNull.DefenseRating.Should().Be(0);
            resultEmpty.DefenseRating.Should().Be(0);
        }

        [Fact]
        public void Calculate_ShouldCorrectlyAccumulateExistingScores()
        {
            double initialPenalties = 5.0;
            int initialCount = 2;

            var result = DefenseRatingCalculator.Calculate([], _mockWeights, initialPenalties, initialCount);

            result.DefenseRating.Should().BeApproximately(0.71428, 0.0001);
            result.AccumulatedPenalties.Should().Be(5.0);
        }

        [Fact]
        public void Calculate_ShouldTreatNegativeAccumulatedAsZero()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var data = new List<RatingDataDTO>
            {
                new() {
                    GoalsConceded = 1,
                    OpponentAttackRating = 1.0,
                    OpponentFifaRank = 120,// Rank weight = 1.0
                    Date = today
                }
            };

            var result = DefenseRatingCalculator.Calculate(data, null!, -50.0, -5);

            // Expected calculations:
            // Total Weight = 1.0 (recency) * 1.0 (rank) * 0.85 (default comp) * 1.0 (default stage) = 0.85
            // penalty = (1 goal * 0.85) / 1.0 (opponent attack) = 0.85
            // avgPenalty = 0.85 / 1 = 0.85
            // defenseRating = 2.5 / (1 + 0.85) = 1.35135
            result.DefenseRating.Should().BeApproximately(1.35135, 0.0001);
        }

        [Fact]
        public void Calculate_ShouldUseDefaultWeights_WhenWeightsParameterIsNull()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var data = new List<RatingDataDTO>
            {
                new() {
                    GoalsConceded = 1,
                    OpponentAttackRating = 1.0,
                    OpponentFifaRank = 120,// Rank weight = 1.0
                    Date = today
                }
            };

            var result = DefenseRatingCalculator.Calculate(data, null!, 1, 3);

            // Expected calculations:
            // Total Weight = 1.0 (recency) * 1.0 (rank) * 0.85 (default comp) * 1.0 (default stage) = 0.85
            // penalty = (1 goal * 0.85) / 1.0 (opponent attack) = 0.85
            // avgPenalty = (1 + 0.85) / (1 + 3) = 0,4625
            // defenseRating = 2.5 / (1 + 0,4625) = 1,7094
            result.DefenseRating.Should().BeApproximately(1.7094, 0.0001);
        }

        [Fact]
        public void Calculate_ShouldReturnExpectedRating_ForControlledScenario()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var data = new List<RatingDataDTO>
            {
                new() {
                    GoalsConceded = 2,
                    OpponentAttackRating = 2.0,
                    OpponentFifaRank = 20, // Rank weight = 2.0
                    Competition = Competition.WorldCup, // Comp Weight 1.4
                    Stage = Stage.GroupStage, // Stage Weight 1.0
                    Date = today
                }
            };

            // Expected calculations:
            // Total Weight = 1.0 (recency) * 2.0 (rank) * 1.4 (comp) * 1.0 (stage) = 2.8
            // penalty = (2 goals * 2.8) / 2.0 (opponent attack) = 2.8
            // avgPenalty = 2.8 / 1 = 2.8
            // defenseRating = 2.5 / (1 + 2.8) = 0.65789
            var result = DefenseRatingCalculator.Calculate(data, _mockWeights, 0, 0);
            result.DefenseRating.Should().BeApproximately(0.65789, 0.0001);
        }

        [Fact]
        public void Calculate_ShouldAverageMultipleMatchesCorrectly()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var data = new List<RatingDataDTO>
            {
                // Match 1: 1 goals, Penalty = 0.85
                new() { GoalsConceded = 1, OpponentAttackRating = 1.0, Date = today, OpponentFifaRank = 120 }, // Total Weight 0.85
                // Match 2: 3 goals, Penalty = 2.55
                new() { GoalsConceded = 3, OpponentAttackRating = 1.0, Date = today, OpponentFifaRank = 120 }  // Total Weight 0.85
            };

            var result = DefenseRatingCalculator.Calculate(data, null!, 0, 0);

            // Expected calculations:
            // sumPenalties = 0.85 + 2.55 = 3.4
            // count = 2
            // avgPenalty = 3.4 / 2 = 1.7
            // defenseRating = 2.5 / (1 + 1.7) = 0.9259
            result.DefenseRating.Should().BeApproximately(0.9259, 0.0001);
            result.AccumulatedCount.Should().Be(2);
        }
    }
}
