using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.LanguageSetup
{
    public class VisitLanguageDto : BaseDto
    {
        public int ProjectDesignVisitId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public IList<VisitLanguageDto> visitLanguages { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public Language Language { get; set; }
    }

    public class VisitLanguageGridDto : BaseAuditDto
    {
        public int ProjectDesignVisitId { get; set; }
        public string VisitName { get; set; }
        public string LanguageName { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        public Language Language { get; set; }
    }
}
