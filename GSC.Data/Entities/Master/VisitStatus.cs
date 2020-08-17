using GSC.Data.Entities.Common;


namespace GSC.Data.Entities.Master
{
    public class VisitStatus : BaseEntity
    {
        public int Code { get; set; }
        public string StatusName { get; set; }

        public int? CompanyId { get; set; }
    }
}
