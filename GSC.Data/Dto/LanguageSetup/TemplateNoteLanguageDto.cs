using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.LanguageSetup
{
    public class TemplateNoteLanguageDto : BaseDto
    {
        public int ProjectDesignTemplateNoteId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public IList<TemplateNoteLanguageDto> templateNoteLanguages { get; set; }
        public ProjectDesignTemplateNote ProjectDesignTemplateNote { get; set; }
        public Language Language { get; set; }
    }

    public class TemplateNoteLanguageGridDto : BaseAuditDto
    {
        public int ProjectDesignTemplateNoteId { get; set; }
        public string Note { get; set; }
        public string LanguageName { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public ProjectDesignTemplateNote ProjectDesignTemplateNote { get; set; }
        public Language Language { get; set; }
    }
}
