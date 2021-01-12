using GSC.Data.Entities.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.LanguageSetup
{
    public class VariableCategoryLanguageDto : BaseDto
    {
        public int VariableCategoryId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public IList<VariableCategoryLanguageDto> variableCategoryLanguages { get; set; }
        public VariableCategory VariableCategory { get; set; }
        public Language Language { get; set; }
    }

    public class VariableCategoryLanguageGridDto : BaseAuditDto
    {
        public int VariableCategoryId { get; set; }
        public string CategoryName { get; set; }
        public string LanguageName { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string DefaultDisplay { get; set; }
        public VariableCategory VariableCategory { get; set; }
        public Language Language { get; set; }
    }
}
