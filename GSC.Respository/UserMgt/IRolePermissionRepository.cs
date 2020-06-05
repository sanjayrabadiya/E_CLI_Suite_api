using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IRolePermissionRepository : IGenericRepository<RolePermission>
    {
        void Save(List<RolePermission> rolePermissionDtos);
        void updatePermission(List<RolePermission> rolePermissionDtos);
        List<RolePermissionDto> GetByRoleId(int roleId);
        List<AppScreen> GetByUserId(int userId, int roleId);
        RolePermission GetRolePermissionByScreenCode(string screenCode);
    }
}