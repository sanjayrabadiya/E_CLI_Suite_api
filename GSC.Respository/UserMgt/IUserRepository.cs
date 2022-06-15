using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Shared.Security;

namespace GSC.Respository.UserMgt
{
    public interface IUserRepository : IGenericRepository<User>
    {
        
        string DuplicateUserName(User objSave);
        List<DropDownDto> GetUserName();
        List<UserGridDto> GetUsers(bool isDeleted);
        List<DropDownDto> GetUserNameDropdown();
        List<UserGridDto> GetPatients(PatientDto userDto);
        LoginResponseDto GetLoginDetails();
        Task<UserLockedGridDto> GetLockedUsers();

    }
}