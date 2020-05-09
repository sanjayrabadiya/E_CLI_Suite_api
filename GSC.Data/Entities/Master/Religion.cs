using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class Religion : BaseEntity
    {
        public string ReligionName { get; set; }
        public int? CompanyId { get; set; }
    }
}