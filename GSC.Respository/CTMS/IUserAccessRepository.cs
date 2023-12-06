using GSC.Common.GenericRespository;
using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IUserAccessRepository : IGenericRepository<UserAccess>
    {
        string Duplicate(UserAccessDto objSave);
        List<UserAccessGridDto> GetUserAccessList(bool isDeleted, int studyId, int siteId);
        void AddSiteUserAccesse(UserAccessDto userAccessDto);
        List<DropDownDto> GetRollUserDropDown();
        void AddProjectRight(int projectId, bool isCtms);
        void AddProjectSiteRight(int ParentProjectId, int projectIdd);
        List<UserAccess> getActive(UserAccessDto objSave);
        List<UserAccessHistoryDto> GetUserAccessHistory(int id);
    }
}