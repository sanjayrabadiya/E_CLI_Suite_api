using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;

namespace GSC.Data.Entities.Master
{
    public class Freezer : BaseEntity, ICommonAduit
    {
        public string FreezerName { get; set; }

        public FreezerType FreezerType { get; set; }

        public string Location { get; set; }

        public string Temprature { get; set; }

        public int Capacity { get; set; }

        public string Note { get; set; }
        public int? CompanyId { get; set; }
    }
}