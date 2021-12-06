using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.Project.Design
{
    public class EditCheckIds
    {
        public int EditCheckId { get; set; }
    }

    public class EditCheckTargetValidation
    {
        public EditCheckTargetValidation()
        {
            EditCheckMsg = new List<EditCheckMessage>();
        }
        public ValidationType? OriginalValidationType { get; set; }
        public bool EditCheckDisable { get; set; }
        public bool IsEnable { get; set; }
        public List<EditCheckMessage> EditCheckMsg { get; set; }
        public EditCheckInfoType InfoType { get; set; }
        public bool HasQueries { get; set; }
    }



    public class EditCheckMessage
    {
        public string AutoNumber { get; set; }
        public string Message { get; set; }
        public string ValidateType { get; set; }
        public string SampleResult { get; set; }
        public bool HasQueries { get; set; }
        public EditCheckInfoType InfoType { get; set; }
    }

    public class EditCheckTargetValidationList : EditCheckTargetValidation
    {
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public bool IsValueSet { get; set; }
        public string Note { get; set; }
        public bool IsSoftFetch { get; set; }
        public int ScreeningTemplateId { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        private DateTime? _scheduleDate { get; set; }
        public DateTime? ScheduleDate
        {
            get => _scheduleDate?.UtcDateTime();
            set => _scheduleDate = value?.UtcDateTime();
        }
        public ScreeningTemplateStatus Status { get; set; }
    }
}
