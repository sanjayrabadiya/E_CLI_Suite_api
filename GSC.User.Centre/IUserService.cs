using GSC.Shared.Security;
using System.Threading.Tasks;

namespace GSC.User.Centre
{
    public interface IUserService
    {
        Task<UserViewModel> ValidateClient(string userName, string password, string clientUrl);
    }
}
