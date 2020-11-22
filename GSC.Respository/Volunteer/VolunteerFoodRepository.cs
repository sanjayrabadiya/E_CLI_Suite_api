using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Volunteer
{
    public class VolunteerFoodRepository : GenericRespository<VolunteerFood>, IVolunteerFoodRepository
    {
        private readonly IGSCContext _context;
        public VolunteerFoodRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
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
            var foods = _context.FoodType.Where(t => t.DeletedDate == null).Select(s => new VolunteerFoodDto
            {
                FoodTypeId = s.Id,
                FoodTypeName = s.TypeName,
                Selected = _context.VolunteerFood.Any(t => t.VolunteerId == volunteerId && t.FoodTypeId == s.Id)
            }).ToList();

            return foods;
        }
    }
}