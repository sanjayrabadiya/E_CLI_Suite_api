using GSC.Shared;

namespace GSC.Data.Entities.UserMgt
{
    public class UserResourceParameters : ResourceParameters
    {
        public UserResourceParameters() : base("UserName")
        {
        }

        public string UserName { get; set; }
    }
}