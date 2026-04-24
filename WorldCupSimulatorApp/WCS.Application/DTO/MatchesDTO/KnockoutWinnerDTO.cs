using WCS.Domain.Enums;

namespace WCS.Application.DTO.MatchesDTO
{
    public class KnockoutWinnerDTO
    {
        public MatchOutcome MatchOutcome { get; set; }
        public double AProbability { get; set; }
        public double BProbability { get; set; }
    }
}
