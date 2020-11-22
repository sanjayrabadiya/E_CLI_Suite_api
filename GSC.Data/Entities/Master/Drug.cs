using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class Drug : BaseEntity
    {
        public string DrugName { get; set; }

        public string Strength { get; set; }

        public string DosageForm { get; set; }
        public int? CompanyId { get; set; }
    }
}