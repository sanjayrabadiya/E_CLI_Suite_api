using GSC.Common.Base;
using GSC.Data.Entities.UserMgt;


namespace GSC.Data.Entities.Project.Generalconfig
{
    public class EmailConfigurationEditCheckSendMailHistory : BaseEntity
    {
        public int EmailConfigurationEditCheckId { get; set; }
        public int RoleId { get; set; }
        public int UserId { get; set; }
        public EmailConfigurationEditCheck EmailConfigurationEditCheck { get; set; }
        public SecurityRole Role { get; set; }
        public User User { get; set; }
    }
}
