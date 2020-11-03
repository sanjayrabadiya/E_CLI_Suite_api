using GSC.Centeral.GenericRespository;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CenteralAuth
{
    public interface ICenteralUserPasswordRepository : IGenericCenteralRepository<UserPassword>
    {
        void CreatePassword(string password, int userId);
        string VaidatePassword(string password, int userId);
    }
}
