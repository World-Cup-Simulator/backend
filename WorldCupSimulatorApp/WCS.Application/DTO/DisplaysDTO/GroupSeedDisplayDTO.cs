namespace WCS.Application.DTO.DisplaysDTO
{
    // DTO for displaying group composition and team seeding information.
    public class GroupSeedDisplayDTO
    {
        public string GroupCode { get; set; } = string.Empty;
        public List<SeededTeamDTO> Teams { get; set; } = [];
    }

    public class SeededTeamDTO
    {
        public string TeamName { get; set; } = string.Empty;
        public string? TeamCode { get; set; }  // 3-letter code
        public int Points { get; set; }
    }
}
