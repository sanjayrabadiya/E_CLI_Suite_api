    using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesign : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int Period { get; set; }
        public int? CompanyId { get; set; }
        public Master.Project Project { get; set; }
        public ICollection<ProjectDesignPeriod> ProjectDesignPeriods { get; set; }
        public List<StudyVersion> StudyVersions { get; set; }
    }
}