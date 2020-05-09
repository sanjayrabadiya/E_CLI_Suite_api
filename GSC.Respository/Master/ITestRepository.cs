using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ITestRepository : IGenericRepository<Test>
    {
        List<DropDownDto> GetTestDropDown();

        List<DropDownDto> GetTestDropDownByTestGroup(int id);
        string Duplicate(Test objSave);
    }
}