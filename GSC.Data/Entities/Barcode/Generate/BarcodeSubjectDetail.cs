using GSC.Data.Entities.Attendance;
using GSC.Common.Base;

namespace GSC.Data.Entities.Barcode.Generate
{
    public class BarcodeSubjectDetail : BaseEntity
    {
        public int? BarcodeGenerateId { get; set; }
        public int? ProjectSubjectId { get; set; }
        public string BarcodeLabelString { get; set; }
        public int? CompanyId { get; set; }
        public ProjectSubject ProjectSubject { get; set; }
    }
}