using WCS.Application.DTO.RatingsDTO;

namespace WCS.Infrastructure.Repositories.Interfaces
{
    public interface IHistoricalMatchRepository
    {
        // Retrieves all historical matches and transforms each into two RatingDataDTO objects
        // (one from each team's perspective). This is used for initial ratings calculation.
        Task<List<RatingDataDTO>> GetAllForInitialRatingsAsync();
    }
}
