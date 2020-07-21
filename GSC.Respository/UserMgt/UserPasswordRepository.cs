using System;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.UserMgt
{
    public class UserPasswordRepository : GenericRespository<UserPassword, GscContext>, IUserPasswordRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public UserPasswordRepository(
            IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public void CreatePassword(string password, int userId)
        {
            var userPassword = new UserPassword();
            userPassword.UserId = userId;
            var saltKey = Cryptography.CreateSaltKey();
            userPassword.Salt = saltKey;
            userPassword.Password = Cryptography.CreatePasswordHash(password, saltKey);
            Add(userPassword);
            Context.SaveChanges(_jwtTokenAccesser);
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