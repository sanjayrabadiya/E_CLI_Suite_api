using GSC.Common.Base;

namespace GSC.Data.Entities.Pharmacy
{
    public class PharmacyTemplateValueChild : BaseEntity
    {
        public int PharmacyTemplateValueId { get; set; }
        public int VariableValueId { get; set; }
        public string Value { get; set; }
    }
}