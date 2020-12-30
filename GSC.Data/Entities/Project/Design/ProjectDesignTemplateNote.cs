using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.LanguageSetup;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignTemplateNote : BaseEntity, ICommonAduit
    {
        public int ProjectDesignTemplateId { get; set; }
        public string Note { get; set; }
        public bool IsPreview { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public List<TemplateNoteLanguage> TemplateNoteLanguage { get; set; }
    }
}
