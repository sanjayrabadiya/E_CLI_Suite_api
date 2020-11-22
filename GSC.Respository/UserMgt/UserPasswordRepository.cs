using System;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.UserMgt
{
    public class UserPasswordRepository : GenericRespository<UserPassword>, IUserPasswordRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public UserPasswordRepository(
            IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public void CreatePassword(string password, int userId)
        {
            var userPassword = new UserPassword();
            userPassword.UserId = userId;
            var saltKey = Cryptography.CreateSaltKey();
            userPassword.Salt = saltKey;
            userPassword.Password = Cryptography.CreatePasswordHash(password, saltKey);
            Add(userPassword);
             _context.Save();
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