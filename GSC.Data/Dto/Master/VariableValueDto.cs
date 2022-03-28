using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Master
{
    public class VariableValueDto : BaseDto
    {
        [Required(ErrorMessage = "Value Code is required.")]
        public string ValueCode { get; set; }

        [Required(ErrorMessage = "Value Name is required.")]
        public string ValueName { get; set; }

        public int SeqNo { get; set; }
        public string Label { get; set; }
        public bool IsDefault { get; set; }
    }

    public class VerificationApprovalVariableValueDto
    {
        public int Id { get; set; }
        public int StudyLevelFormVariableId { get; set; }
        public string ValueName { get; set; }
        public string VerificationApprovalValue { get; set; }
        public int VerificationApprovalTemplateValueChildId { get; set; }
        public string VerificationApprovalValueOld { get; set; }
        public string Label { get; set; }
        public int SeqNo { get; set; }
    }
}