using System.ComponentModel.DataAnnotations;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Master
{
    public class VariableValueDto : BaseDto
    {
        
        public string TableCollectionSourceName { get; set; }
        public int VariableId { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public int SeqNo { get; set; }
        public string Label { get; set; }
        public bool IsDefault { get; set; }
        public string Style { get; set; }
        public TableCollectionSource? TableCollectionSource { get; set; }
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
        public string Style { get; set; }
    }
}