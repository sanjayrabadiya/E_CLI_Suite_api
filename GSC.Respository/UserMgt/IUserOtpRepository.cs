using GSC.Common.GenericRespository;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;

namespace GSC.Respository.UserMgt
{
    public interface IUserOtpRepository : IGenericRepository<UserOtp>
    {
        string VerifyOtp(UserOtpDto userOtpDto);
        string InsertOtp(string username);
        string ChangePasswordByOtp(UserOtpDto userOtpDto);
    }
}