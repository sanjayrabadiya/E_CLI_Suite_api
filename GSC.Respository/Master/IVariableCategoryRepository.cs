using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IVariableCategoryRepository : IGenericRepository<VariableCategory>
    {
        List<DropDownDto> GetVariableCategoryDropDown();
        string Duplicate(VariableCategory objSave);
        List<VariableCategoryGridDto> GetVariableCategoryList(bool isDeleted);
    }
}