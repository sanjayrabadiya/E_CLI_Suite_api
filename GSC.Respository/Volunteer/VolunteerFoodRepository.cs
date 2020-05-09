using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Volunteer
{
    public class VolunteerFoodRepository : GenericRespository<VolunteerFood, GscContext>, IVolunteerFoodRepository
    {
        public VolunteerFoodRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public void SaveFoods(VolunteerFoodDto foodDto)
        {
            var existingFoods = FindBy(t => t.VolunteerId == foodDto.VolunteerId).ToList();
            existingFoods.ForEach(Remove);

            if (foodDto.FoodTypeIds?.Count > 0)
                foodDto.FoodTypeIds.ForEach(t =>
                {
                    var volunteerFood = new VolunteerFood
                    {
                        VolunteerId = foodDto.VolunteerId,
                        FoodTypeId = t
                    };
                    Add(volunteerFood);
                });
        }

        public List<VolunteerFoodDto> GetFoods(int volunteerId)
        {
            var foods = Context.FoodType.Where(t => t.DeletedDate == null).Select(s => new VolunteerFoodDto
            {
                FoodTypeId = s.Id,
                FoodTypeName = s.TypeName,
                Selected = Context.VolunteerFood.Any(t => t.VolunteerId == volunteerId && t.FoodTypeId == s.Id)
            }).ToList();

            return foods;
        }
    }
}