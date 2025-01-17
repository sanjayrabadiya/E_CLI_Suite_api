﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
   public interface IEtmfUserPermissionRepository : IGenericRepository<EtmfUserPermission>
    {
        List<EtmfUserPermissionDto> GetByUserId(int UserId, int RoleId, int ProjectId, int? ParentProject);
        int Save(List<EtmfUserPermission> EtmfUserPermission);
        void updatePermission(List<EtmfUserPermissionDto> EtmfUserPermissionDto);
        void AddEtmfAccessRights(List<EtmfProjectWorkPlace> ProjectWorkplaceDetail);
        List<EtmfUserPermissionDto> GetEtmfPermissionData(int ProjectId);
        void SaveProjectRollbackRight(int projectId, int[] userIds);
        List<EtmfUserPermissionDto> GetEtmfRightHistoryDetails(int projectId, int userId);
        List<DropDownDto> GetSitesForEtmf(int ProjectId);
        List<DropDownDto> GetUsersByEtmfRights(int ProjectId, int ProjectDetailsId);
    }
}
