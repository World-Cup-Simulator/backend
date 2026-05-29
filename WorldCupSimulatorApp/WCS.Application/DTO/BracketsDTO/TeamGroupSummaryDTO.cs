namespace WCS.Application.DTO.BracketsDTO
{
    // Flat read-model DTO for building group tables.
    // Projected by the repository layer.
    public record TeamGroupSummaryDTO(
        int TeamId,
        string Name,
        int FifaRank,
        string GroupCode,
        int PositionOrder,
        double AccumulatedScores,
        double AccumulatedWeights,
        double AccumulatedPenalties,
        int AccumulatedCount
    );
}
