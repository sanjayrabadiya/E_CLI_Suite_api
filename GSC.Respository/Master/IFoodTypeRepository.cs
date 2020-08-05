using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IFoodTypeRepository : IGenericRepository<FoodType>
    {
        List<DropDownDto> GetFoodTypeDropDown();
        string Duplicate(FoodType objSave);
        List<FoodTypeGridDto> GetFoodTypeList(bool isDeleted);
    }
}