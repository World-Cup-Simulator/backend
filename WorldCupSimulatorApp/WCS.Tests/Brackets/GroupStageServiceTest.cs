using FluentAssertions;
using WCS.Application.DTO.BracketsDTO;
using WCS.Application.DTO.MatchesDTO;
using WCS.Application.Services.Brackets;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Tests.Brackets
{
    public class GroupStageServiceTest
    {
        private readonly GroupStageService _service = new();

        [Fact]
        public void BuildGroups_WithValidTeams_ReturnsGroupedTables()
        {
            var teams = new List<TeamGroupSummaryDTO>
        {
            new(1, "Argentina", 1, "A", 1, 100, 10, 5, 3),
            new(2, "Brasil", 2, "A", 2, 90, 9, 4, 3),
            new(3, "Alemania", 3, "B", 1, 80, 8, 3, 3),
            new(4, "Italia", 4, "B", 2, 70, 7, 2, 3)
        };
            var result = _service.BuildGroups(teams);
            result.Should().HaveCount(2);
            result[0].GroupCode.Should().Be("A");
            result[0].Teams.Should().HaveCount(2);
            result[1].GroupCode.Should().Be("B");
            result[1].Teams.Should().HaveCount(2);
            result[0].Teams[0].Name.Should().Be("Argentina");
            result[0].Teams[0].FifaRank.Should().Be(1);
            result[0].Teams[0].AccumulatedScores.Should().Be(100);
        }

        [Fact]
        public void BuildGroups_WithEmptyList_ThrowsArgumentException()
        {
            Action act = () => _service.BuildGroups(new List<TeamGroupSummaryDTO>());
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Teams list cannot be empty*");
        }

        [Fact]
        public void BuildGroups_WithNullList_ThrowsArgumentNullException()
        {
            Action act = () => _service.BuildGroups(null!);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void UpdateGroups_WithWinA_ResultReflectsWinnerA()
        {
            var groupTables = CreateSingleGroupTable();
            var matches = CreateSingleMatch();
            List<IMatchResult> Simulate(List<SimulationMatchDTO> m) =>
                new List<IMatchResult>
                {
                new SimpleMatchResultDTO
                {
                    TeamAID = 1,
                    TeamBID = 2,
                    Winner = MatchOutcome.WinA,
                    OutcomeProbability = 0.7
                }
                };
            var results = _service.UpdateGroups(matches, groupTables, Simulate);
            results.Should().HaveCount(1);
            results[0].Winner.Should().Be(MatchOutcome.WinA);
            var teamA = groupTables[0].Teams.First(t => t.TeamId == 1);
            teamA.Points.Should().Be(3); // Winner gets 3 points
        }

        [Fact]
        public void UpdateGroups_WithDraw_ResultReflectsDraw()
        {
            var groupTables = CreateSingleGroupTable();
            var matches = CreateSingleMatch();
            List<IMatchResult> Simulate(List<SimulationMatchDTO> m) =>
                new List<IMatchResult>
                {
                new MatchResultDTO
                {
                    TeamAID = 1,
                    TeamBID = 2,
                    Winner = MatchOutcome.Draw,
                    OutcomeProbability = 0.3,
                    GoalsA = 1,
                    GoalsB = 1
                }
                };
            var results = _service.UpdateGroups(matches, groupTables, Simulate);
            var teamA = groupTables[0].Teams.First(t => t.TeamId == 1);
            var teamB = groupTables[0].Teams.First(t => t.TeamId == 2);
            // Draw: each team gets 1 point
            teamA.Points.Should().Be(1);
            teamB.Points.Should().Be(1);
            teamA.GoalsScored.Should().Be(1);
            teamA.GoalsConceded.Should().Be(1);
        }

        [Fact]
        public void UpdateGroups_WithEmptyMatches_ThrowsArgumentException()
        {
            var groupTables = CreateSingleGroupTable();
            Action act = () => _service.UpdateGroups(
                new List<SimulationMatchDTO>(),
                groupTables,
                _ => new List<IMatchResult>());
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Matches list cannot be empty*");
        }

        [Fact]
        public void UpdateGroups_WithNullMatches_ThrowsArgumentNullException()
        {
            Action act = () => _service.UpdateGroups(
                null!,
                new List<GroupTable>(),
                _ => new List<IMatchResult>());
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void BuildRoundOf32_WithValidGroups_ReturnsCompleteBracket()
        {
            var groups = CreateWorldCupGroups();
            var matches = _service.BuildRoundOf32(groups);
            // 2026 format: 12 groups -> 16 knockout matches
            matches.Should().HaveCount(16);
            // Verify all match keys are present
            matches.Select(m => m.Key).Should().ContainInOrder(Enumerable.Range(1, 16));
            // Verify no duplicate team assignments
            var assignedTeamIds = matches
                .SelectMany(m => new[] { m.TeamAID, m.TeamBID })
                .Where(id => id != 0)
                .ToList();
            assignedTeamIds.Should().OnlyHaveUniqueItems();
        }

        [Fact]
        public void BuildRoundOf32_WithEmptyGroups_ThrowsArgumentException()
        {
            Action act = () => _service.BuildRoundOf32(new List<GroupTable>());
            act.Should().Throw<ArgumentException>()
                .WithMessage("*Groups list cannot be empty*");
        }

        private static List<GroupTable> CreateSingleGroupTable()
        {
            return new List<GroupTable>
            {
                new()
                {
                    GroupCode = "A",
                    Teams = new List<GroupTableEntry>
                    {
                        new() { TeamId = 1, Name = "Team A", FifaRank = 1 },
                        new() { TeamId = 2, Name = "Team B", FifaRank = 2 }
                    }
                }
            };
        }

        private static List<SimulationMatchDTO> CreateSingleMatch()
        {
            return new List<SimulationMatchDTO>
            {
                new()
                {
                    TeamAID = 1,
                    TeamA = "Team A",
                    AAccumulatedScores = 1,
                    AAccumulatedWeights = 1,
                    AAccumulatedPenalties = 1,
                    AAccumulatedCount = 1,
                    TeamBID = 2,
                    TeamB = "Team B",
                    BAccumulatedScores = 1,
                    BAccumulatedWeights = 1,
                    BAccumulatedPenalties = 1,
                    BAccumulatedCount = 1
                }
            };
        }

        private static List<GroupTable> CreateWorldCupGroups()
        {
            var groups = new List<GroupTable>();
            var groupCodes = new[] { "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L" };
            for (int g = 0; g < groupCodes.Length; g++)
            {
                var teams = new List<GroupTableEntry>();
                for (int t = 0; t < 4; t++)
                {
                    // Create standings: Winner (9pts), Runner-up (6pts), Third (3pts), Last (0pts)
                    teams.Add(new GroupTableEntry
                    {
                        TeamId = g * 4 + t + 1,
                        Name = $"Team {groupCodes[g]}{t + 1}",
                        FifaRank = t + 1,
                        Points = (3 - t) * 3,
                        GoalsScored = 4 - t,
                        GoalsConceded = t,
                        AccumulatedScores = 100 - t * 10,
                        AccumulatedWeights = 10,
                        AccumulatedPenalties = 5,
                        AccumulatedCount = 3
                    });
                }
                groups.Add(new GroupTable
                {
                    GroupCode = groupCodes[g],
                    Teams = teams
                });
            }
            return groups;
        }
    }
}