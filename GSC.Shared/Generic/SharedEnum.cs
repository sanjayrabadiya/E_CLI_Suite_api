using System.ComponentModel;

namespace GSC.Shared.Generic
{
    public enum UserMasterUserType : short
    {
        [Description("SuperAdmin")] SuperAdmin = 1,
        [Description("Administrator")] Administrator = 2,
        [Description("Patient")] Patient = 3,
        [Description("User")] User = 4
    }

    public enum AuditAction : short
    {
        [Description("Inserted")] Inserted = 1,
        [Description("Updated")] Updated = 2,
        [Description("Deleted")] Deleted = 3,
        [Description("Activated")] Activated = 4
    }

    public enum PrefLanguage : short
    {
        [Description("English")] en = 1,
        [Description("Germany")] ge = 2
    }

}
