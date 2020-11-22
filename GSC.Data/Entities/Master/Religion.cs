using GSC.Common.Base;

namespace GSC.Data.Entities.Master
{
    public class Religion : BaseEntity
    {
        public string ReligionName { get; set; }
        public int? CompanyId { get; set; }
    }
}