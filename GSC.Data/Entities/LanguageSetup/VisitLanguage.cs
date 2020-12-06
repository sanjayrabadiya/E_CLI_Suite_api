using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Data.Entities.LanguageSetup
{
    public class VisitLanguage : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVisitId { get; set; }

        public int LanguageId { get; set; }

        public string Display { get; set; }

        public int? CompanyId { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public Language Language { get; set; }
    }
}
