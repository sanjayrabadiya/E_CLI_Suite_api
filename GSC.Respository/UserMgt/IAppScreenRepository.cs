using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IAppScreenRepository : IGenericRepository<AppScreen>
    {
        List<DropDownDto> GetAppScreenParentFromDropDown();
        List<DropDownDto> GetMasterTableName();
        List<DropDownDto> GetAppScreenChildParentFromDropDown(int id);
        List<DropDownDto> GetTableColunms(int id);
        List<DropDownDto> GetAppScreenDropDownByParentScreenCode(string parentScreenCode);

        List<DropDownDto> GetTableColunmsIWRS(int id);

    }
}