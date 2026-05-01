using FluentAssertions;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Services.Ratings;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Tests.Ratings
{
    public class AttackRatingCalculatorTest
    {
        private readonly RatingWeightsOptions _mockWeights;

        public AttackRatingCalculatorTest()
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
            var resultNull = AttackRatingCalculator.Calculate(null!, _mockWeights, 0, 0);
            var resultEmpty = AttackRatingCalculator.Calculate([], _mockWeights, 0, 0);

            resultNull.AttackRating.Should().Be(0);
            resultEmpty.AttackRating.Should().Be(0);
        }

        [Fact]
        public void Calculate_ShouldCorrectlyAccumulateExistingScores()
        {
            double initialScores = 10.5;
            double initialWeights = 5.0;

            var result = AttackRatingCalculator.Calculate([], _mockWeights, initialScores, initialWeights);

            result.AccumulatedScores.Should().Be(initialScores);
            result.AccumulatedWeights.Should().Be(initialWeights);
        }

        [Fact]
        public void Calculate_ShouldTreatNegativeAccumulatedAsZero()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var data = new List<RatingDataDTO>
            {
                new() {
                    GoalsScored = 3,
                    Date = today,
                    OpponentFifaRank = 20 // Rank weight = 2.0
                }
            };

            var result = AttackRatingCalculator.Calculate(data, null!, -10, -5);

            // Expected calculations:
            // Match Weight = 1.0 (recency) * 2.0 (rank) * 0.85 (default comp) * 1.0 (default stage) = 1.7
            // Match Score = 3 goals * 1.7 = 5.1
            // Total Scores = 0 (accumulated) + 5.1 = 5.1
            // Total Weight = 0 (accumulated) + 1.7 = 1.7
            // attackRating = 5.1 / 1.7 ≈ 3
            result.AccumulatedScores.Should().BeApproximately(5.1, 0.0001);
            result.AccumulatedWeights.Should().BeApproximately(1.7, 0.0001);
            result.AttackRating.Should().BeApproximately(3, 0.0001);
        }

        [Fact]
        public void Calculate_ShouldUseDefaultWeights_WhenWeightsParameterIsNull()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var data = new List<RatingDataDTO>
            {
                new() {
                    GoalsScored = 3,
                    Date = today,
                    OpponentFifaRank = 20 // Rank weight = 2.0
                }
            };

            var result = AttackRatingCalculator.Calculate(data, null!, 10, 5);

            // Expected calculations using defaults:
            // Match Weight = 1.0 (recency) * 2.0 (rank) * 0.85 (default comp) * 1.0 (default stage) = 1.7
            // Match Score = 3 goals * 1.7 = 5.1
            // Total Scores = 10 (accumulated) + 5.1 = 15.1
            // Total Weight = 5 (accumulated) + 1.7 = 6.7
            // attackRating = 15.1 / 6.7 ≈ 2.2537
            result.AccumulatedScores.Should().BeApproximately(15.1, 0.0001);
            result.AccumulatedWeights.Should().BeApproximately(6.7, 0.0001);
            result.AttackRating.Should().BeApproximately(2.2537, 0.0001);
        }

        [Fact]
        public void Calculate_ShouldReturnExpectedRating_ForControlledScenario()
        {
            // GetRecencyWeight = 1.0
            var today = DateOnly.FromDateTime(DateTime.Today);

            var data = new List<RatingDataDTO>
            {
                new() {
                    GoalsScored = 2,
                    Date = today,
                    OpponentFifaRank = 20, // Rank weight 2.0
                    Competition = Competition.WorldCup, // Competition weight = 1.4
                    Stage = Stage.GroupStage // Stage weight = 1.0
                }
            };

            // Expected calculations:
            // Total Weight = 1.0 (recency) * 2.0 (rank) * 1.4 (comp) * 1.0 (stage) = 2.8
            // Match Score = 2 (goals) * 2.8 = 5.6
            // attackRating = 5.6 / 2.8 = 2.0
            var result = AttackRatingCalculator.Calculate(data, _mockWeights, 0, 0);

            result.AttackRating.Should().BeApproximately(2.0, 0.0001);
            result.AccumulatedScores.Should().BeApproximately(5.6, 0.0001);
            result.AccumulatedWeights.Should().BeApproximately(2.8, 0.0001);
        }

        [Fact]
        public void Calculate_ShouldAverageMultipleMatchesCorrectly()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);

            var data = new List<RatingDataDTO>
            {
            // Match 1: 2 goals, Total Weight = 1.0
                new RatingDataDTO {
                    GoalsScored = 2, Date = today, OpponentFifaRank = 120, // Rank Weight 1.0
                    Competition = Competition.Friendly, Stage = Stage.GroupStage // Stage weight = 1.0
                },
            // Match 2: 4 goals, Total Weight = 2.0
                new RatingDataDTO {
                    GoalsScored = 4, Date = today, OpponentFifaRank = 20, // Rank Weight 2.0
                    Competition = Competition.Friendly, Stage = Stage.GroupStage // Stage weight = 1.0
                }
            };

            // Score1: 2 * 1.0 = 2 | Score2: 4 * 2.0 = 8
            // SumScores = 10 | SumWeights = 3.0
            // attackRating = 10 / 3 = 3.3333
            var result = AttackRatingCalculator.Calculate(data, _mockWeights, 0, 0);

            result.AttackRating.Should().BeApproximately(3.3333, 0.0001);
        }
    }
}
