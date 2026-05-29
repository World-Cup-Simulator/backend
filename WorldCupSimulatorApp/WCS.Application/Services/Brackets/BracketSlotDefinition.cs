using WCS.Domain.Enums;

namespace WCS.Application.Services.Brackets
{
    // Defines a single slot in the knockout bracket.
    public record BracketSlot(
        int MatchKey,
        int NextMatchKey,
        TeamSlot HomeTeamSlot,
        TeamSlot AwayTeamSlot
    );

    // Abstract base for team slot definitions.
    public abstract record TeamSlot;

    // Represents a team at a specific position within a group.
    public record GroupPositionSlot(string GroupCode, GroupPosition Position) : TeamSlot;

    // Represents a slot filled by the best available third-placed team from the specified eligible groups.
    public record BestThirdSlot(params string[] EligibleGroups) : TeamSlot;
}
