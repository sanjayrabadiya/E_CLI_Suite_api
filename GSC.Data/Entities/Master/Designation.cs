using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class Designation : BaseEntity, ICommonAduit
    {
        public string DesignationCod { get; set; }
        public string NameOFDesignation { get; set; }
        public string YersOfExperience { get; set; }
        public int? DepartmentId { get; set; }
        public Department Department { get; set; }
    }
}
