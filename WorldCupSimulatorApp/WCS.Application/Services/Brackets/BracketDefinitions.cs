using WCS.Domain.Enums;

namespace WCS.Application.Services.Brackets
{
    // Declarative bracket definitions for the 2026 FIFA World Cup.
    public static class BracketDefinitions
    {
        // Round of 32 bracket slots for the 2026 World Cup.
        // Third-place teams are assigned greedily in this sequence.
        public static readonly IReadOnlyList<BracketSlot> RoundOf32 = new List<BracketSlot>
    {
        // Match 1: Group E Winner vs Best 3rd (from A/B/C/D/F)
        new(1, 1, new GroupPositionSlot("E", GroupPosition.Winner),
            new BestThirdSlot("A", "B", "C", "D", "F")),
        // Match 2: Group I Winner vs Best 3rd (from C/D/F/G/H)
        new(2, 1, new GroupPositionSlot("I", GroupPosition.Winner),
            new BestThirdSlot("C", "D", "F", "G", "H")),
        // Match 3: Group A Runner-up vs Group B Runner-up
        new(3, 2, new GroupPositionSlot("A", GroupPosition.RunnerUp),
            new GroupPositionSlot("B", GroupPosition.RunnerUp)),
        // Match 4: Group F Winner vs Group C Runner-up
        new(4, 2, new GroupPositionSlot("F", GroupPosition.Winner),
            new GroupPositionSlot("C", GroupPosition.RunnerUp)),
        // Match 5: Group K Runner-up vs Group L Runner-up
        new(5, 3, new GroupPositionSlot("K", GroupPosition.RunnerUp),
            new GroupPositionSlot("L", GroupPosition.RunnerUp)),
        // Match 6: Group H Winner vs Group J Runner-up
        new(6, 3, new GroupPositionSlot("H", GroupPosition.Winner),
            new GroupPositionSlot("J", GroupPosition.RunnerUp)),
        // Match 7: Group D Winner vs Best 3rd (from B/E/F/I/J)
        new(7, 4, new GroupPositionSlot("D", GroupPosition.Winner),
            new BestThirdSlot("B", "E", "F", "I", "J")),
        // Match 8: Group G Winner vs Best 3rd (from A/E/H/I/J)
        new(8, 4, new GroupPositionSlot("G", GroupPosition.Winner),
            new BestThirdSlot("A", "E", "H", "I", "J")),
        // Match 9: Group C Winner vs Group F Runner-up
        new(9, 5, new GroupPositionSlot("C", GroupPosition.Winner),
            new GroupPositionSlot("F", GroupPosition.RunnerUp)),
        // Match 10: Group E Runner-up vs Group I Runner-up
        new(10, 5, new GroupPositionSlot("E", GroupPosition.RunnerUp),
            new GroupPositionSlot("I", GroupPosition.RunnerUp)),
        // Match 11: Group A Winner vs Best 3rd (from G/E/F/H/I)
        new(11, 6, new GroupPositionSlot("A", GroupPosition.Winner),
            new BestThirdSlot("G", "E", "F", "H", "I")),
        // Match 12: Group L Winner vs Best 3rd (from E/H/I/J/K)
        new(12, 6, new GroupPositionSlot("L", GroupPosition.Winner),
            new BestThirdSlot("E", "H", "I", "J", "K")),
        // Match 13: Group J Winner vs Group H Runner-up
        new(13, 7, new GroupPositionSlot("J", GroupPosition.Winner),
            new GroupPositionSlot("H", GroupPosition.RunnerUp)),
        // Match 14: Group D Runner-up vs Group G Runner-up
        new(14, 7, new GroupPositionSlot("D", GroupPosition.RunnerUp),
            new GroupPositionSlot("G", GroupPosition.RunnerUp)),
        // Match 15: Group B Winner vs Best 3rd (from E/F/G/I/J)
        new(15, 8, new GroupPositionSlot("B", GroupPosition.Winner),
            new BestThirdSlot("E", "F", "G", "I", "J")),
        // Match 16: Group K Winner vs Best 3rd (from D/E/I/J/L)
        new(16, 8, new GroupPositionSlot("K", GroupPosition.Winner),
            new BestThirdSlot("D", "E", "I", "J", "L"))
    };
    }
}
