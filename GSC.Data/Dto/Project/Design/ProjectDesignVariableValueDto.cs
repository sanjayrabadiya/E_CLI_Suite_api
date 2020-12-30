using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.LanguageSetup;

namespace GSC.Data.Dto.Project.Design
{
    public class ProjectDesignVariableValueDto : BaseDto
    {
        public int ProjectDesignVariableId { get; set; }

        [Required(ErrorMessage = "Value Code is required.")]
        public string ValueCode { get; set; }

        [Required(ErrorMessage = "Value Name is required.")]
        public string ValueName { get; set; }

        public int SeqNo { get; set; }

        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueChildId { get; set; }
        public string ScreeningValueOld { get; set; }
    }

    public class ScreeningVariableValueDto
    {
        public int Id { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string ValueName { get; set; }
        public string ScreeningValue { get; set; }
        public int ScreeningTemplateValueChildId { get; set; }
        public string ScreeningValueOld { get; set; }
        public int SeqNo { get; set; }
        public List<VariableValueLanguage> VariableValueLanguage { get; set; }
    }

    public class ProjectDesignVariableValueDropDown 
    {
        public int Id { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
    }
}