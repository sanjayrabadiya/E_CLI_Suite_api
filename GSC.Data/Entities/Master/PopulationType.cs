using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class PopulationType : BaseEntity
    {
        public string PopulationName { get; set; }

        public int? CompanyId { get; set; }
    }
}