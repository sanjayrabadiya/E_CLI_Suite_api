using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.LanguageSetup;
using GSC.Helper;

namespace GSC.Data.Dto.Project.StudyLevelFormSetup
{
    public class StudyLevelFormVariableValueDto : BaseDto
    {
        public int StudyLevelFormVariableId { get; set; }

        [Required(ErrorMessage = "Value Code is required.")]
        public string ValueCode { get; set; }

        [Required(ErrorMessage = "Value Name is required.")]
        public string ValueName { get; set; }
        public double? InActiveVersion { get; set; }
        public double? StudyVersion { get; set; }
        public int SeqNo { get; set; }
        public string Label { get; set; }
        public bool AllowActive { get; set; }
        public string DisplayVersion { get; set; }
        public string VariableValue { get; set; }
        public int CtmsMonitoringReportVariableValueId { get; set; }
        public int CtmsMonitoringReportVariableValueChildId { get; set; }
        public string VariableValueOld { get; set; }
    }
    
    public class StudyLevelFormVariableValueDropDown
    {
        public int Id { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
    }
}