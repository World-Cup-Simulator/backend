using WCS.Application.DTO.RatingsDTO;

namespace WCS.Application.Services.Ratings
{
    public interface IRatingService
    {
        double CalculateAttack(List<RatingDataDTO> data);
        double CalculateDefense(List<RatingDataDTO> data);

    }
}
