namespace WCS.Application.Services.Ratings
{
    public class RatingHelper
    {
        public double GetRankingWeight(int rank)
        {
            return Math.Clamp(2.2 - (rank / 100.0), 0.5, 2.0);
        }

        public double GetRecencyWeight(DateOnly date)
        {
            var years = (DateTime.Today - date.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25;

            if (years <= 1) return 1.00;
            if (years <= 2) return 0.85;
            if (years <= 3) return 0.70;
            return 0.55;
        }
    }
}
