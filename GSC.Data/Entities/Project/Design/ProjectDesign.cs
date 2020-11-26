using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesign : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int Period { get; set; }
        public string Version { get; set; }
        public bool IsActiveVersion { get; set; }
        public bool IsCompleteDesign { get; set; }
        public int? CompanyId { get; set; }
        public Master.Project Project { get; set; }
        public ICollection<ProjectDesignPeriod> ProjectDesignPeriods { get; set; }
        public bool IsUnderTesting { get; set; }
    }
}