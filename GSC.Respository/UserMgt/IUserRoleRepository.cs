using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IUserRoleRepository : IGenericRepository<UserRole>
    {
        IList<DropDownDto> GetRoleByUserName(string userName);
        IList<MenuDto> GetMenuList();

        IList<DropDownDto> GetUserNameByRoleId(int roleId);
    }
}