using System.Collections.Generic;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Pharmacy
{
    public class PharmacyVerificationTemplateValueDto : BaseDto
    {
        public int PharmacyVerificationEntryId { get; set; }
        public int VariableId { get; set; }
        public string Value { get; set; }
        public string DocPath { get; set; }
        public string MimeType { get; set; }
        public IsFormType? Status { get; set; }
        public VariableDto Variables { get; set; }
        public ICollection<PharmacyVerificationTemplateValueChildDto> Children { get; set; }
        public string VariableName { get; set; }
        public string StatusName { get; set; }
        public int? TempId { get; set; }
        public string ValueId { get; set; }
    }

    public class PharmacyVerificationTemplateValueListDto : BaseDto
    {
        public IList<PharmacyVerificationEntryDto> PharmacyVerificationEntry { get; set; }
        public ICollection<VariableDto> VariableList { get; set; }
        public ICollection<PharmacyVerificationTemplateValueDto> PharmacyVerificationTemplateValue { get; set; }
    }
}