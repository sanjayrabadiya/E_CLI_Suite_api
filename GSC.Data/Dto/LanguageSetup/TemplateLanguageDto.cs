using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.LanguageSetup
{
    public class TemplateLanguageDto : BaseDto
    {
        public int ProjectDesignTemplateId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public IList<TemplateLanguageDto> templateLanguages { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public Language Language { get; set; }
    }

    public class TemplateLanguageGridDto : BaseAuditDto
    {
        public int ProjectDesignTemplateId { get; set; }
        public string TemplateName { get; set; }
        public string LanguageName { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        public Language Language { get; set; }
    }
}
