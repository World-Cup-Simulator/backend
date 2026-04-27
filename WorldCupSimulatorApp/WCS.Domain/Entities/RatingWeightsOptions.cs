namespace WCS.Domain.Entities
{
    public class RatingWeightsOptions
    {
        public Dictionary<string, double> Competition { get; set; } = [];
        public Dictionary<string, double> Stage { get; set; } = [];
    }
}
