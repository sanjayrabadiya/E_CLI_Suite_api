using GSC.Common.Base;

namespace GSC.Data.Entities.Pharmacy
{
    public class PharmacyVerificationTemplateValueChild : BaseEntity
    {
        public int PharmacyVerificationTemplateValueId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
    }
}