using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Data.Entities.LanguageSetup
{
    public class TemplateNoteLanguage : BaseEntity, ICommonAduit
    {
        public int ProjectDesignTemplateNoteId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public int? CompanyId { get; set; }
        public ProjectDesignTemplateNote ProjectDesignTemplateNote { get; set; }
        public Language Language { get; set; }
    }
}
