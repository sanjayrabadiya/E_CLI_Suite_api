using GSC.Common.GenericRespository;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IUserPasswordRepository : IGenericRepository<UserPassword>
    {
        void CreatePassword(string password, int userId);
       
    }
}