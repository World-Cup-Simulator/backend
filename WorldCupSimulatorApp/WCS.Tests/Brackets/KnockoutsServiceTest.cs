using FluentAssertions;
using Moq;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Services.Brackets;
using WCS.Application.Services.Simulators;
using WCS.Domain.Enums;

namespace WCS.Tests.Brackets
{
    public class KnockoutsServiceTest
    {
        private readonly Mock<ISimulationService> _simulationMock = new();
        private readonly KnockoutsService _service;

        public KnockoutsServiceTest()
        {
            _service = new KnockoutsService(_simulationMock.Object);
        }

        [Fact]
        public void PerformSimpleKnockouts_WithWinner_CreatesNextRound()
        {
            var matches = CreateRoundOf16Matches(); // Now creates 2 matche
                                                    // s
            List<IMatchResult> Simulate(List<SimulationMatchDTO> m) =>
                new List<IMatchResult>
                {
                new SimpleMatchResultDTO { TeamAID = 1, TeamBID = 2, Winner = MatchOutcome.WinA },
                new SimpleMatchResultDTO { TeamAID = 3, TeamBID = 4, Winner = MatchOutcome.WinA }
                };

            var result = KnockoutsService.PerformSimpleKnockouts(matches, Simulate);

            result.Results.Should().HaveCount(2);
            result.NextMatches.Should().HaveCount(1); // 2 matches → 1 next round match
            result.NextMatches[0].Stage.Should().Be(3); // Stage 2 → Stage 3
            result.NextMatches[0].TeamAID.Should().Be(1); // Winner of match 1
            result.NextMatches[0].TeamBID.Should().Be(3); // Winner of match 2
        }

        [Fact]
        public void PerformSimpleKnockouts_WithNullMatches_ThrowsArgumentNullException()
        {
            Action act = () => KnockoutsService.PerformSimpleKnockouts(null!, _ => new List<IMatchResult>());
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void PerformSimpleKnockouts_WithEmptyMatches_ThrowsArgumentException()
        {
            Action act = () => KnockoutsService.PerformSimpleKnockouts(
                new List<KnockoutMatchDTO>(),
                _ => new List<IMatchResult>());

            act.Should().Throw<ArgumentException>();
        }
        [Fact]

        public void PerformSimpleKnockouts_WithSingleMatch_ReturnsEmptyNextRound()
        {
            // A single match cannot produce a next round (needs pairs)
            var matches = new List<KnockoutMatchDTO>
        {
            new()
            {
                Key = 1,
                Stage = 2,
                TeamAID = 1,
                TeamBID = 2
            }
        };

            List<IMatchResult> Simulate(List<SimulationMatchDTO> m) =>
                new List<IMatchResult>
                {
                new SimpleMatchResultDTO { TeamAID = 1, TeamBID = 2, Winner = MatchOutcome.WinA }
                };

            // Should throw because incomplete pair
            Action act = () => KnockoutsService.PerformSimpleKnockouts(matches, Simulate);
            act.Should().Throw<InvalidOperationException>();
        }

        [Fact]
        public void PerformAdaptativeKnockouts_CallsSimulationService()
        {
            var matches = CreateRoundOf16Matches();
            var previousResults = new List<RatingDataDTO>();

            _simulationMock.Setup(s => s.SimulateAdaptativeKnockoutsWithScores(
                    It.IsAny<List<SimulationMatchDTO>>(),
                    It.IsAny<List<RatingDataDTO>>()))
                .Returns(new List<IMatchResult>
                {
                new AdaptativeMatchResultDTO { TeamAID = 1, TeamBID = 2, Winner = MatchOutcome.WinA, GoalsA = 2, GoalsB = 0 },
                new AdaptativeMatchResultDTO { TeamAID = 3, TeamBID = 4, Winner = MatchOutcome.WinA, GoalsA = 1, GoalsB = 0 }
                });

            var result = _service.PerformAdaptativeKnockouts(matches, previousResults);

            _simulationMock.Verify(s => s.SimulateAdaptativeKnockoutsWithScores(
                It.IsAny<List<SimulationMatchDTO>>(),
                previousResults), Times.Once);

            result.PreviousResults.Should().HaveCount(2); // One per match result
        }

        [Fact]
        public void PerformAdaptativeKnockouts_WithNullMatches_ThrowsArgumentNullException()
        {
            Action act = () => _service.PerformAdaptativeKnockouts(null!, new List<RatingDataDTO>());
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void ConvertGroupResultsToRatingData_WithValidResults_ReturnsRatingData()
        {
            var groupResults = CreateGroupResults();
            var result = KnockoutsService.ConvertGroupResultsToRatingData(groupResults);
            result.Should().HaveCount(2);
            result.All(r => r.Stage == Stage.GroupStage).Should().BeTrue();
        }

        [Fact]
        public void ConvertGroupResultsToRatingData_WithNull_ThrowsArgumentNullException()
        {
            Action act = () => KnockoutsService.ConvertGroupResultsToRatingData(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        private static List<KnockoutMatchDTO> CreateRoundOf16Matches()
        {
            return new List<KnockoutMatchDTO>
        {
            new()
            {
                Key = 1,
                Stage = 2, // Round of 16
                NextMatchKey = 1,
                TeamAID = 1,
                TeamA = "Team A",
                TeamBID = 2,
                TeamB = "Team B",
                TeamAFifaRank = 1,
                TeamBFifaRank = 2
            },
            new()
            {
                Key = 2,
                Stage = 2, // Round of 16
                NextMatchKey = 1,
                TeamAID = 3,
                TeamA = "Team C",
                TeamBID = 4,
                TeamB = "Team D",
                TeamAFifaRank = 3,
                TeamBFifaRank = 4
            }
        };
        }

        private static List<GroupResultDTO> CreateGroupResults()
        {
            return new List<GroupResultDTO>
        {
            new()
            {
                TeamAID = 1,
                TeamBID = 2,
                GoalsA = 2,
                GoalsB = 1,
                AAttackRating = 1.5,
                BAttackRating = 1.2
            }
        };
        }
    }
}