using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Helper;

namespace GSC.Data.Entities.Pharmacy
{
    public class PharmacyVerificationTemplateValue : BaseEntity
    {
        public int PharmacyVerificationEntryId { get; set; }
        public int VariableId { get; set; }
        public string Value { get; set; }
        public string DocPath { get; set; }
        public string MimeType { get; set; }
        public IsFormType? Status { get; set; }
        public ICollection<PharmacyVerificationTemplateValueChild> Children { get; set; }
    }
}