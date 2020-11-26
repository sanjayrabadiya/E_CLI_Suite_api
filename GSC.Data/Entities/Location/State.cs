using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Location
{
    public class State : BaseEntity, ICommonAduit
    {
        public string StateName { get; set; }

        public int CountryId { get; set; }

        public Country Country { get; set; }
        public int? CompanyId { get; set; }
    }
}