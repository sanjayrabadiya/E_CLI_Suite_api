using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IAppScreenRepository : IGenericRepository<AppScreen>
    {
        List<DropDownDto> GetAppScreenParentFromDropDown();
        List<DropDownDto> GetMasterTableName();
    }
}