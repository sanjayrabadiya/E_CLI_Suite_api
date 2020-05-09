using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Master
{
    public class AnnotationType : BaseEntity
    {
        public string AnnotationeCode { get; set; }

        public string AnnotationeName { get; set; }
        public int? CompanyId { get; set; }
    }
}