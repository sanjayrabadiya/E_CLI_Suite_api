using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.User.Centre
{
    public interface IUserService
    {
        UserViewModel ValidateClient(string userName, string password);
    }
}
