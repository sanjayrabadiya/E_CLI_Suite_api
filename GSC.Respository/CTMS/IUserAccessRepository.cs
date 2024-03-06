using GSC.Common.GenericRespository;
using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IUserAccessRepository : IGenericRepository<UserAccess>
    {
        string Duplicate(UserAccessDto userAccessDto);
        List<UserAccessGridDto> GetUserAccessList(bool isDeleted, int studyId, int siteId);
        void AddSiteUserAccesse(UserAccessDto userAccessDto);
        List<DropDownDto> GetRollUserDropDown();
        void AddProjectRight(int projectId, bool isCtms);
        void AddProjectSiteRight(int ParentProjectId, int projectId);
        List<UserAccess> getActive(UserAccessDto userAccessDto);
        List<UserAccessHistoryDto> GetUserAccessHistory(int id);
    }
}