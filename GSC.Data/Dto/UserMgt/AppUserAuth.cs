using System.Collections.Generic;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Dto.UserMgt
{
    public class AppUserAuth
    {
        public AppUserAuth()
        {
            UserName = "Not authorized";
            BearerToken = string.Empty;
        }

        public string UserName { get; set; }
        public string BearerToken { get; set; }
        public bool IsAuthenticated { get; set; }

        public List<AppUserClaim> Claims { get; set; }
    }
}