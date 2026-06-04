namespace WCS.Application.DTO.DisplaysDTO
{
    public class GroupTableDisplayDTO
    {
        public string GroupCode { get; set; } = string.Empty;
        public List<GroupTableTeamDisplayDTO> Teams { get; set; } = [];
    }
}
