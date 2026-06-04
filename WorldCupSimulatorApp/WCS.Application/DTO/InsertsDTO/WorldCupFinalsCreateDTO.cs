using WCS.Domain.Enums;

namespace WCS.Application.DTO.InsertsDTO
{
    public class WorldCupFinalsCreateDTO
    {
        public int Key { get; set; }
        public Stage Stage { get; set; }
        public DateOnly Date { get; set; }
        public int NextMatchKey { get; set; }
        public int TeamAId { get; set; }
        public int TeamBId { get; set; }
    }
}
