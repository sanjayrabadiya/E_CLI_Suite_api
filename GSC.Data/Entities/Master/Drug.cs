using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Drug : BaseEntity, ICommonAduit
    {
        public string DrugName { get; set; }

        public string Strength { get; set; }

        public string DosageForm { get; set; }
        public int? CompanyId { get; set; }
    }
}