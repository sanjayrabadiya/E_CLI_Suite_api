using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class ProjectSettings : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public bool IsCtms { get; set; }
        public bool IsEicf { get; set; }
        public bool IsScreening { get; set; }
        public bool IsPatientEngagement { get; set; }
        public Master.Project Project { get; set; }
    }
}
