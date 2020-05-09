using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IRoleRepository : IGenericRepository<SecurityRole>
    {
        string ValidateRole(SecurityRole objSave);
        void UpdateSecurityRole(int id);
        List<DropDownDto> GetSecurityRoleDropDown();
    }
}