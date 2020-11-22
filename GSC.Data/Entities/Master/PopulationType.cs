using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class PopulationType : BaseEntity
    {
        public string PopulationName { get; set; }

        public int? CompanyId { get; set; }
    }
}