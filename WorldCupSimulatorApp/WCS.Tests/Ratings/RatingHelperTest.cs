using FluentAssertions;
using WCS.Application.Services.Ratings;

namespace WCS.Tests.Ratings
{
    public class RatingHelperTest
    {
        [Theory]
        [InlineData(1, 2.0)]    // Rank 1 should have the maximum rank weight, 2.0
        [InlineData(-1, 0.5)]    // Negative rank should remain at the minimum value of 0.5
        [InlineData(20, 2.0)]   // 2.2 - 0.2 = 2.0
        [InlineData(100, 1.2)]  // 2.2 - 1.0 = 1.2
        [InlineData(170, 0.5)]  // 2.2 - 1.7 = 0.5
        [InlineData(500, 0.5)]  // Very low rank should remain at the minimum value of 0.5
        public void GetRankingWeight_ShouldReturnCorrectWeight_WithinBounds(int rank, double expected)
        {
            double result = RatingHelper.GetRankingWeight(rank);

            result.Should().BeApproximately(expected, 0.0001);
        }

        [Fact]
        public void GetRecencyWeight_ShouldReturnCorrectWeights_BasedOnAge()
        {
            var today = DateOnly.FromDateTime(DateTime.Today);
            var futureDate = today.AddMonths(6);
            var sixMonthsAgo = today.AddMonths(-6);
            var eighteenMonthsAgo = today.AddMonths(-18);
            var twoAndHalfYearsAgo = today.AddYears(-2).AddMonths(-6);
            var tenYearsAgo = today.AddYears(-10);

            RatingHelper.GetRecencyWeight(futureDate).Should().Be(1.00);
            RatingHelper.GetRecencyWeight(sixMonthsAgo).Should().Be(1.00);            
            RatingHelper.GetRecencyWeight(eighteenMonthsAgo).Should().Be(0.85);
            RatingHelper.GetRecencyWeight(twoAndHalfYearsAgo).Should().Be(0.70);
            RatingHelper.GetRecencyWeight(tenYearsAgo).Should().Be(0.55);
        }
    }
}
