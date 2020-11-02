using GSC.Centeral.Context;
using GSC.Centeral.GenericRespository;
using GSC.Centeral.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.CenteralAuth
{
    public class CenteralUserPasswordRepository : GenericCenteralRespository<UserPassword, CenteralContext>, ICenteralUserPasswordRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUnitOfWorkCenteral<CenteralContext> _uow;
        public CenteralUserPasswordRepository(
            IUnitOfWorkCenteral<CenteralContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
        }

        public void CreatePassword(string password, int userId)
        {
            var userPassword = new UserPassword();
            userPassword.UserId = userId;
            var saltKey = Cryptography.CreateSaltKey();
            userPassword.Salt = saltKey;
            userPassword.Password = Cryptography.CreatePasswordHash(password, saltKey);
            Add(userPassword);
            _uow.Save();
        }

        public string VaidatePassword(string password, int userId)
        {
            var userPassword = FindBy
                    (x => x.UserId == userId)
                .OrderByDescending(x => x.Id).FirstOrDefault();
            if (userPassword != null)
            {
            }


            if (userPassword != null && !string.Equals(userPassword.Password, Cryptography.CreatePasswordHash(password, userPassword.Salt),
                    StringComparison.Ordinal)) return "Invalid Password";
            return "";
        }
    }
}
