using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Master
{
    public class AnnotationType : BaseEntity, ICommonAduit
    {
        public string AnnotationeCode { get; set; }

        public string AnnotationeName { get; set; }
        public int? CompanyId { get; set; }
    }
}