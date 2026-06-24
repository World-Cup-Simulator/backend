namespace WCS.Application.DTO.BracketsDTO
{
    public class ThirdPlaceInputDTO
    {
        public int Index { get; set; }      // 0-7 (ranking order from frontend)
        public string Group { get; set; } = string.Empty;  // "A", "B", etc.
    }
}
