using System.ComponentModel.DataAnnotations;

namespace WCS.Domain.Entities
{
    public class GroupTable
    {
        [MaxLength(1, ErrorMessage = "Code must not exceed 1 character")]
        public string GroupCode { get; set; } = string.Empty;
        public List<GroupTableEntry> Teams { get; set; } = [];
    }
}
