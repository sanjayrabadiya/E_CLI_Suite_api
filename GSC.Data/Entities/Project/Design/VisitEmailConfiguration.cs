using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Entities.Project.Design
{
    public class VisitEmailConfiguration : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVisitId { get; set; }
        public string EmailBody { get; set;}
        public string Subject { get; set; }
        public ScreeningVisitStatus VisitStatusId { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public List<VisitEmailConfigurationRoles> VisitEmailConfigurationRoles { get; set; }
    }
}
