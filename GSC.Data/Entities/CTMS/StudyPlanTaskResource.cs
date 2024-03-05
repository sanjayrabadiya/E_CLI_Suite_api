using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.CTMS
{
    public class StudyPlanTaskResource : BaseEntity, ICommonAduit
    {
        public int StudyPlanTaskId { get; set; }
        public int SecurityRoleId { get; set; }
        public int UserId { get; set; }
        public StudyPlanTask StudyPlanTask { get; set; }
        public SecurityRole SecurityRole { get; set; }
        public User User { get; set; }

    }
}
