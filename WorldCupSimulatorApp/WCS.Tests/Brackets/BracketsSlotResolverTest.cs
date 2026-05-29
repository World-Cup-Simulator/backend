using FluentAssertions;
using WCS.Application.Services.Brackets;
using WCS.Domain.Entities;
using WCS.Domain.Enums;

namespace WCS.Tests.Brackets
{
    public class BracketSlotResolverTest
    {
        [Fact]
        public void Resolve_GroupPositionSlot_ReturnsCorrectTeam()
        {
            var rankings = CreateGroupRankings();
            var resolver = new BracketSlotResolver(rankings);

            // Slot: Group A Winner vs Group B Runner-up
            var slot = new BracketSlot(1, 1,
                new GroupPositionSlot("A", GroupPosition.Winner),
                new GroupPositionSlot("B", GroupPosition.RunnerUp));
            var match = resolver.Resolve(slot);
            match.TeamAID.Should().Be(1); // Team A1 (winner of group A)
            match.TeamBID.Should().Be(6); // Team B2 (runner-up of group B)
        }

        [Fact]
        public void Resolve_BestThirdSlot_AssignsHighestRankedEligibleTeam()
        {
            var rankings = CreateGroupRankings();
            var resolver = new BracketSlotResolver(rankings);

            // Slot that accepts best third-place team from groups A, B, or C
            var slot = new BracketSlot(1, 1,
                new GroupPositionSlot("A", GroupPosition.Winner),
                new BestThirdSlot("A", "B", "C"));
            var match = resolver.Resolve(slot);
            match.TeamBID.Should().NotBeNull();
        }

        [Fact]
        public void Resolve_BestThirdSlot_WithNoEligibleTeam_ThrowsInvalidOperationException()
        {
            var rankings = CreateGroupRankings();
            var resolver = new BracketSlotResolver(rankings);

            // Slot with invalid group codes (not in top 8 third-place teams)
            var slot = new BracketSlot(1, 1,
                new GroupPositionSlot("A", GroupPosition.Winner),
                new BestThirdSlot("Z", "Y", "X"));
            Action act = () => resolver.Resolve(slot);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*No eligible third-place team found*");
        }

        [Fact]
        public void Resolve_MissingGroup_ThrowsInvalidOperationException()
        {
            var rankings = CreateGroupRankings();
            var resolver = new BracketSlotResolver(rankings);

            // Slot referencing non-existent group
            var slot = new BracketSlot(1, 1,
                new GroupPositionSlot("Z", GroupPosition.Winner),
                new GroupPositionSlot("A", GroupPosition.RunnerUp));
            Action act = () => resolver.Resolve(slot);
            act.Should().Throw<InvalidOperationException>()
                .WithMessage("*Group 'Z' not found*");
        }

        private static List<GroupRanking> CreateGroupRankings()
        {
            var rankings = new List<GroupRanking>();

            // Create 12 groups (A-L) with 4 teams each
            for (char c = 'A'; c <= 'L'; c++)
            {
                var teams = new List<GroupTableEntry>();
                for (int i = 0; i < 4; i++)
                {
                    teams.Add(new GroupTableEntry
                    {
                        TeamId = (c - 'A') * 4 + i + 1,
                        Name = $"Team {c}{i + 1}",
                        Points = (3 - i) * 3, // 9, 6, 3, 0 points
                        GoalsScored = 4 - i,
                        GoalsConceded = i
                    });
                }
                rankings.Add(new GroupRanking(c.ToString(), teams));
            }
            return rankings;
        }
    }
}