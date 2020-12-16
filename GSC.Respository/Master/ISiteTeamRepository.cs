using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Master
{
    public interface ISiteTeamRepository : IGenericRepository<SiteTeam>
    {
        string Duplicate(SiteTeamDto objSave);
        List<SiteTeamGridDto> GetSiteTeamList(int projectid, bool isDeleted);
        List<DropDownDto> GetRoleDropdownForSiteTeam(int projectid);
        List<UserDto> GetUserDropdownForSiteTeam(int projectid,int roleId);
    }
}
