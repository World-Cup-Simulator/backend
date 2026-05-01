using FluentAssertions;
using Moq;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.DTO.ProbabilitiesDTO;
using WCS.Application.DTO.RatingsDTO;
using WCS.Application.Services.Probabilities;
using WCS.Application.Services.Ratings;
using WCS.Application.Services.Simulators;
using WCS.Domain.Enums;

namespace WCS.Tests.Simulators
{
    public class SimulationServiceTest
    {
        private readonly Mock<IMatchProbabilityService> _probMock;
        private readonly Mock<IRatingService> _ratingMock;
        private readonly SimulationService _service;

        public SimulationServiceTest()
        {
            _probMock = new Mock<IMatchProbabilityService>();
            _ratingMock = new Mock<IRatingService>();
            _service = new SimulationService(_probMock.Object, _ratingMock.Object);
        }

        [Fact]
        public void SimpleSimulateGroupsStage_ShouldThrowException_WhenMatchesListIsEmpty()
        {
            Action act = () => _service.SimpleSimulateGroupsStage([]);
            act.Should().Throw<ArgumentException>().WithMessage("Matches list is empty.");
        }

        [Fact]
        public void SimpleSimulateGroupsStage_ShouldReturnCorrectCountAndData_WhenMatchesAreValid()
        {
            var matches = new List<SimpleSimulationMatchDTO>
            {
                new() { TeamAID = 1, TeamA = "Argentina", TeamBID = 2, TeamB = "Arabia Saudita" },
                new() { TeamAID = 3, TeamA = "México", TeamBID = 4, TeamB = "Polonia" }
            };

            // Configure the mock to return generic values and avoid failures
            _probMock.Setup(p => p.CalculateLambda(It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(1.5);

            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(new MatchProbabilityDTO { WinA = 0.5, Draw = 0.3, WinB = 0.2 });

            _probMock.Setup(p => p.PickRandomOutcome(It.IsAny<MatchProbabilityDTO>()))
                     .Returns(MatchOutcome.WinA);

            var results = _service.SimpleSimulateGroupsStage(matches);

            results.Should().HaveCount(2); // Validate count
            results[0].TeamA.Should().Be("Argentina");
            results[0].WinnerID.Should().Be(1); // Team A wins according to our mock
            results[1].TeamA.Should().Be("México");
            results[1].OutcomeProbability.Should().Be(0.5);

            // Verify that dependencies were called the expected number of times
            _probMock.Verify(p => p.PickRandomOutcome(It.IsAny<MatchProbabilityDTO>()), Times.Exactly(2));
        }

        [Fact]
        public void SimpleSimulateKnockouts_ShouldThrowException_WhenMatchesListIsEmpty()
        {
            Action act = () => _service.SimpleSimulateKnockouts([]);
            act.Should().Throw<ArgumentException>().WithMessage("Matches list is empty.");
        }

        [Fact]
        public void SimpleSimulateKnockouts_ShouldReturnWinnerDirectly_WhenNoDrawOccurs()
        {
            var matches = new List<SimpleSimulationMatchDTO> {
                new() { TeamAID = 1, TeamA = "Argentina", TeamBID = 2, TeamB = "Arabia Saudita" },
                new() { TeamAID = 3, TeamA = "México", TeamBID = 4, TeamB = "Polonia" }
            };

            _probMock.Setup(p => p.CalculateLambda(It.IsAny<double>(), It.IsAny<double>())).Returns(1.0);
            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(new MatchProbabilityDTO { WinA = 0.6, WinB = 0.3, Draw = 0.1 });

            // Force Team A to win directly
            _probMock.Setup(p => p.PickRandomOutcome(It.IsAny<MatchProbabilityDTO>()))
                     .Returns(MatchOutcome.WinA);

            var results = _service.SimpleSimulateKnockouts(matches);

            results[0].TeamA.Should().Be("Argentina");
            results[0].WinnerID.Should().Be(1);
            results[1].Winner.Should().Be(MatchOutcome.WinA);
            results[1].OutcomeProbability.Should().Be(0.6);
        }

        [Fact]
        public void SimpleSimulateKnockouts_ShouldUseDecider_WhenInitialOutcomeIsDraw()
        {
            var matches = new List<SimpleSimulationMatchDTO> {
                new() { TeamAID = 1, TeamA = "Fra", TeamBID = 2, TeamB = "Eng" }
            };

            // Initial probabilities: 40% Team A win, 40% Team B win, 20% draw
            var initialProb = new MatchProbabilityDTO { WinA = 0.4, WinB = 0.4, Draw = 0.2 };

            _probMock.Setup(p => p.CalculateLambda(It.IsAny<double>(), It.IsAny<double>())).Returns(1.0);
            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(initialProb);

            // The first outcome is a draw
            _probMock.Setup(p => p.PickRandomOutcome(initialProb))
                     .Returns(MatchOutcome.Draw);

            var results = _service.SimpleSimulateKnockouts(matches);
            var finalResult = results.First();

            finalResult.Winner.Should().NotBe(MatchOutcome.Draw); // The decider should have picked a winner

            // The decider recalculates: WinA / (WinA + WinB) -> 0.4 / (0.4 + 0.4) = 0.5
            finalResult.OutcomeProbability.Should().Be(0.5);
        }

        [Fact]
        public void SimpleSimulateGroupsStageWithScores_ShouldThrowException_WhenMatchesListIsEmpty()
        {
            Action act = () => _service.SimpleSimulateGroupsStageWithScores([]);
            act.Should().Throw<ArgumentException>().WithMessage("Matches list is empty.");
        }

        [Fact]
        public void SimpleSimulateGroupsStageWithScores_ShouldReturnWinnerA_WhenGoalsAIsGreater()
        {
            var matches = new List<SimpleSimulationMatchDTO> { 
                new() { TeamAID = 1, TeamA = "Argentina", TeamBID = 2, TeamB = "Brasil" }
            };

            var fakeProb = new MatchProbabilityDTO { WinA = 0.6, WinB = 0.2, Draw = 0.2, Scores = [] };
            var winningScore = new ScoreProbabilityDTO { GoalsA = 2, GoalsB = 1, Probability = 0.1 };

            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(fakeProb);

            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(winningScore);

            var result = _service.SimpleSimulateGroupsStageWithScores(matches).First();

            result.Winner.Should().Be(MatchOutcome.WinA);
            result.WinnerID.Should().Be(1);
            result.OutcomeProbability.Should().Be(0.6); // Total win probability for Team A
            result.ScoreProbability.Should().Be(0.1);   // Specific probability of the 2-1 score
        }

        [Fact]
        public void SimpleSimulateGroupsStageWithScores_ShouldReturnDraw_WhenScoreIsEqual()
        {
            var matches = new List<SimpleSimulationMatchDTO> {
                new() { TeamAID = 1, TeamA = "Arg", TeamBID = 2, TeamB = "Bra" }
            };

            var fakeProb = new MatchProbabilityDTO { WinA = 0.4, WinB = 0.3, Draw = 0.3, Scores = [] };
            var equalScore = new ScoreProbabilityDTO { GoalsA = 1, GoalsB = 1, Probability = 0.15 };

            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(fakeProb);

            // Force a draw score
            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(equalScore);

            var result = _service.SimpleSimulateGroupsStageWithScores(matches).First();

            result.Winner.Should().Be(MatchOutcome.Draw);
            result.WinnerID.Should().BeNull();
            result.GoalsA.Should().Be(1);
            result.GoalsB.Should().Be(1);
            result.OutcomeProbability.Should().Be(0.3); // Should use the total draw probability
        }

        [Fact]
        public void SimpleSimulateGroupsStageWithScores_ShouldProcessAllMatches()
        {
            var matches = new List<SimpleSimulationMatchDTO> {
                new() { TeamAID = 1, TeamA = "Arg", TeamBID = 2, TeamB = "Bra" },
                new() { TeamAID = 4, TeamA = "Holanda", TeamBID = 2, TeamB = "Bolivia" },
            };

            var fakeProb = new MatchProbabilityDTO { WinA = 0.6, WinB = 0.2, Draw = 0.2, Scores = [] };
            var winningScore = new ScoreProbabilityDTO { GoalsA = 2, GoalsB = 1, Probability = 0.1 };

            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(fakeProb);

            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(winningScore);

            var results = _service.SimpleSimulateGroupsStageWithScores(matches);

            results.Should().HaveCount(2);
            results[0].WinnerID.Should().Be(1);
            results[1].WinnerID.Should().Be(4);
            _probMock.Verify(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()), Times.Exactly(2));
        }

        [Fact]
        public void SimpleSimulateKnockoutsWithScores_ShouldThrowException_WhenMatchesListIsEmpty()
        {
            Action act = () => _service.SimpleSimulateKnockoutsWithScores([]);
            act.Should().Throw<ArgumentException>().WithMessage("Matches list is empty.");
        }

        [Fact]
        public void SimpleSimulateKnockoutsWithScores_ShouldNotUsePenalties_WhenThereIsAWinner()
        {
            var matches = new List<SimpleSimulationMatchDTO> {
                new() { TeamAID = 1, TeamA = "Argentina", TeamBID = 2, TeamB = "Brasil" }
            };

            var winningScore = new ScoreProbabilityDTO { GoalsA = 3, GoalsB = 0, Probability = 0.1 };
            var fakeProb = new MatchProbabilityDTO { WinA = 0.8, WinB = 0.1, Draw = 0.1, Scores = [] };

            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(fakeProb);
            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(winningScore);

            var result = _service.SimpleSimulateKnockoutsWithScores(matches).First();

            result.Winner.Should().Be(MatchOutcome.WinA);
            result.DecidedByPenalties.Should().BeFalse();
            result.OutcomeProbability.Should().Be(0.8);
        }

        [Fact]
        public void SimpleSimulateKnockoutsWithScores_ShouldDecideByPenalties_WhenScoreIsDraw()
        {
            var matches = new List<SimpleSimulationMatchDTO> {
                new() { TeamAID = 1, TeamA = "Arg", TeamBID = 2, TeamB = "Fra" }
            };

            var fakeProb = new MatchProbabilityDTO { WinA = 0.5, WinB = 0.5, Draw = 0.1, Scores = []};
            var drawScore = new ScoreProbabilityDTO { GoalsA = 2, GoalsB = 2, Probability = 0.05 };

            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(fakeProb);

            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(drawScore);

            var result = _service.SimpleSimulateKnockoutsWithScores(matches).First();

            result.GoalsA.Should().Be(2);
            result.GoalsB.Should().Be(2);
            result.Winner.Should().NotBe(MatchOutcome.Draw); // Draws are not allowed in knockout stages
            result.DecidedByPenalties.Should().BeTrue();
            result.WinnerID.Should().NotBeNull();
            // The probability must be normalized (0.5 / (0.5 + 0.5) = 0.5)
            result.OutcomeProbability.Should().Be(0.5);
        }

        [Fact]
        public void SimpleSimulateKnockoutsWithScores_ShouldProcessAllMatches()
        {
            var matches = new List<SimpleSimulationMatchDTO> {
                new() { TeamAID = 1, TeamA = "Arg", TeamBID = 2, TeamB = "Bra" },
                new() { TeamAID = 4, TeamA = "Holanda", TeamBID = 2, TeamB = "Bolivia" },
            };

            var winningScore = new ScoreProbabilityDTO { GoalsA = 3, GoalsB = 0, Probability = 0.1 };
            var fakeProb = new MatchProbabilityDTO { WinA = 0.8, WinB = 0.1, Draw = 0.1, Scores = [] };

            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(fakeProb);
            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(winningScore);

            var results = _service.SimpleSimulateKnockoutsWithScores(matches);

            results.Should().HaveCount(2);
            results[0].WinnerID.Should().Be(1);
            results[1].WinnerID.Should().Be(4);
            _probMock.Verify(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()), Times.Exactly(2));
        }

        [Fact]
        public void SimulateAdaptativeKnockoutsWithScores_ShouldThrowException_WhenMatchesListIsEmpty()
        {
            Action act = () => _service.SimulateAdaptativeKnockoutsWithScores([], []);
            act.Should().Throw<ArgumentException>().WithMessage("Matches list is empty.");
        }

        [Fact]
        public void SimulateAdaptativeKnockoutsWithScores_ShouldUseRatingsFromService()
        {
            var matches = new List<AdaptativeSimulationMatchDTO> {
                new() { TeamAID = 1, TeamA = "Arg", TeamBID = 2, TeamB = "Ger" }
            };

            var previousResults = new List<RatingDataDTO>();

            // Configure RatingService to return specific ratings
            var expectedAttack = new AttackRatingDTO { AttackRating = 2.5, AccumulatedScores = 100 };
            var expectedDefense = new DefenseRatingDTO { DefenseRating = 1.0, AccumulatedCount = 5 };

            _ratingMock.Setup(r => r.CalculateAttack(It.IsAny<List<RatingDataDTO>>(), It.IsAny<double>(), It.IsAny<double>()))
                       .Returns(expectedAttack);
            _ratingMock.Setup(r => r.CalculateDefense(It.IsAny<List<RatingDataDTO>>(), It.IsAny<double>(), It.IsAny<int>()))
                       .Returns(expectedDefense);

            // Verify that Poisson receives the 2.5 and 1.0 values from RatingService
            _probMock.Setup(p => p.CalculateLambda(2.5, 1.0)).Returns(2.5);
            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(new MatchProbabilityDTO { WinA = 0.8, WinB = 0.1, Draw = 0.1, Scores = [] });
            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(new ScoreProbabilityDTO { GoalsA = 2, GoalsB = 0 });

            var result = _service.SimulateAdaptativeKnockoutsWithScores(matches, previousResults).First();

            // Verify that the winner's accumulated values (Team A) are mapped to the result
            result.WinnerAccumulatedScores.Should().Be(100);
            result.WinnerAccumulatedCount.Should().Be(5);

            // Verify that RatingService was called twice per team (Attack and Defense)
            _ratingMock.Verify(r => r.CalculateAttack(It.IsAny<List<RatingDataDTO>>(), It.IsAny<double>(), It.IsAny<double>()),
                Times.Exactly(2));
        }

        [Fact]
        public void SimulateAdaptativeKnockoutsWithScores_ShouldAssignWinnerBData_WhenTeamBWins()
        {
            var matches = new List<AdaptativeSimulationMatchDTO> { new() { TeamAID = 1, TeamBID = 2 } };
            var bAttack = new AttackRatingDTO { AttackRating = 3.0, AccumulatedScores = 999 };

            // Setup so Team B returns 999
            _ratingMock.Setup(r => r.CalculateAttack(It.IsAny<List<RatingDataDTO>>(), It.IsAny<double>(), It.IsAny<double>()))
                       .Returns(bAttack);
            _ratingMock.Setup(r => r.CalculateDefense(It.IsAny<List<RatingDataDTO>>(), It.IsAny<double>(), It.IsAny<int>()))
                       .Returns(new DefenseRatingDTO());

            // Force Team B to win (0-1)
            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(new ScoreProbabilityDTO { GoalsA = 0, GoalsB = 1 });
            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(new MatchProbabilityDTO { WinB = 0.7 });

            var result = _service.SimulateAdaptativeKnockoutsWithScores(matches, []).First();

            result.WinnerID.Should().Be(2);
            result.WinnerAccumulatedScores.Should().Be(999);
        }

        [Fact]
        public void SimulateAdaptativeKnockoutsWithScores_ShouldHandlePenaltiesCorrectly()
        {
            var matches = new List<AdaptativeSimulationMatchDTO> { new() { TeamAID = 1, TeamBID = 2 } };
            _probMock.Setup(p => p.CalculateMatchProbabilities(It.IsAny<int>(), It.IsAny<double>(), It.IsAny<double>()))
                     .Returns(new MatchProbabilityDTO { WinA = 0.5, WinB = 0.5 });

            // Score is 0-0
            _probMock.Setup(p => p.PickRandomScore(It.IsAny<List<ScoreProbabilityDTO>>()))
                     .Returns(new ScoreProbabilityDTO { GoalsA = 0, GoalsB = 0 });

            _ratingMock.Setup(r => r.CalculateAttack(It.IsAny<List<RatingDataDTO>>(), It.IsAny<double>(), It.IsAny<double>()))
                       .Returns(new AttackRatingDTO());
            _ratingMock.Setup(r => r.CalculateDefense(It.IsAny<List<RatingDataDTO>>(), It.IsAny<double>(), It.IsAny<int>()))
                       .Returns(new DefenseRatingDTO());

            var result = _service.SimulateAdaptativeKnockoutsWithScores(matches, []).First();

            result.DecidedByPenalties.Should().BeTrue();
            result.Winner.Should().NotBe(MatchOutcome.Draw);
        }
    }
}
