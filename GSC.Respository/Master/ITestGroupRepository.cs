using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Respository.Master
{
    public interface ITestGroupRepository : IGenericRepository<TestGroup>
    {
        List<DropDownDto> GetTestGroupDropDown();
        string Duplicate(TestGroup objSave);
    }
}