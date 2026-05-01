namespace WCS.Application.Services.Ratings
{
    public class RatingHelper
    {
        // Converts FIFA rank into opponent strength weight.
        // Better rank (lower number) => higher weight.
        // Output range limited between 0.5 and 2.0.
        public static double GetRankingWeight(int rank)
        {
            if (rank <= 0)
            {
                return 0.5;
            }

            return Math.Clamp(2.2 - (rank / 100.0), 0.5, 2.0);
        }

        // Applies recency weighting to prioritize recent matches.
        public static double GetRecencyWeight(DateOnly date)
        {
            var years = (DateTime.Today - date.ToDateTime(TimeOnly.MinValue)).TotalDays / 365.25;

            if (years <= 1) return 1.00;
            if (years <= 2) return 0.85;
            if (years <= 3) return 0.70;
            return 0.55;
        }
    }
}
