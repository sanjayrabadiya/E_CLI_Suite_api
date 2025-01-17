using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface IDepartmentRepository : IGenericRepository<Department>
    {
        List<DropDownDto> GetDepartmentDropDown();
        string Duplicate(Department objSave);
        List<DepartmentGridDto> GetDepartmentList(bool isDeleted);
    }
}