using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
   public interface IProjectWorkplaceDetailRepository : IGenericRepository<ProjectWorkplaceDetail>
    {
        List<DropDownDto> GetCountryByWorkplace(int ParentProjectId);
        List<DropDownDto> GetSiteByWorkplace(int ParentProjectId);
        List<EtmfUserPermissionDto> GetByUserId(int UserId, int ProjectId);
        void Save(List<EtmfUserPermission> EtmfUserPermission);
        void updatePermission(List<EtmfUserPermissionDto> etmfUserPermission);
        void AddEtmfAccessRights(List<ProjectWorkplaceDetail> ProjectWorkplaceDetail);
        List<EtmfUserPermissionDto> GetEtmfPermissionData(int ProjectId);
        void SaveProjectRollbackRight(int projectId, int[] userIds);

        List<EtmfUserPermissionDto> GetEtmfRightHistoryDetails(int projectId, int userId);
        List<DropDownDto> GetSitesForEtmf(int ProjectId);
    }
}
