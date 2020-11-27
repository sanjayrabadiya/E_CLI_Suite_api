

using System.ComponentModel;

namespace GSC.Shared
{
    public enum UserMasterUserType : short
    {
        [Description("SuperAdmin")] SuperAdmin = 1,
        [Description("Administrator")] Administrator = 2,
        [Description("Patient")] Patient = 3,
        [Description("User")] User = 4
    }
}
