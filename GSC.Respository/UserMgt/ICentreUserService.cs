using GSC.Data.Dto.UserMgt;
using GSC.Shared.Security;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.UserMgt
{
    public interface ICentreUserService
    {
        Task<UserViewModel> ValidateClient(LoginDto loginDto, string clientUrl);
    }
}
