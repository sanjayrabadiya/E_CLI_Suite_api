using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using System.Threading.Tasks;

namespace GSC.Respository.UserMgt
{
    public interface IUserOtpRepository : IGenericRepository<UserOtp>
    {
        string VerifyOtp(UserOtpDto userOtpDto);
        Task<string> InsertOtp(string username);
        string ChangePasswordByOtp(UserOtpDto userOtpDto);
    }
}