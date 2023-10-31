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
        string DuplicateIActive(UserAccess objSave);
        List<UserAccessGridDto> GetUserAccessList(bool isDeleted, int studyId, int siteId);
        void AddSiteUserAccesse(UserAccessDto userAccessDto);
        void UpdateSiteUserAccess(UserAccess userAccess);
        void DeleteSiteUserAccess(int id);
        void ActiveSiteUserAccess(int id);
        List<DropDownDto> GetRollUserDropDown(int projectId);
    }
}