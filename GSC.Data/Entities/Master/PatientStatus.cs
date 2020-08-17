using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class PatientStatus : BaseEntity
    {

        public int Code { get; set; }
        public string StatusName { get; set; }
        public int? CompanyId { get; set; }
    }
}
