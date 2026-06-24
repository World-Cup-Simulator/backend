using WCS.Application.DTO.BracketsDTO;

namespace WCS.Application.Services.Brackets
{
    public interface IBracketThirdPlaceService
    {
        // Assigns third-place teams to bracket slots using backtracking.
        List<ThirdPlaceAssignmentDTO> AssignThirdPlaces(List<ThirdPlaceInputDTO> thirdPlaces);
    }
}
