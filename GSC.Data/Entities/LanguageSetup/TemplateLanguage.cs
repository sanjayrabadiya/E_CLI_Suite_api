using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Data.Entities.LanguageSetup
{
    public class TemplateLanguage : BaseEntity, ICommonAduit
    {
        public int ProjectDesignTemplateId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public int? CompanyId { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public Language Language { get; set; }
    }
}
