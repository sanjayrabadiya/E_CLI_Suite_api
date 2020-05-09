using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;

namespace GSC.Respository.Volunteer
{
    public interface IVolunteerFoodRepository : IGenericRepository<VolunteerFood>
    {
        List<VolunteerFoodDto> GetFoods(int volunteerId);
        void SaveFoods(VolunteerFoodDto foodDto);
    }
}