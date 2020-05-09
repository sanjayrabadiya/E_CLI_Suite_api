using System.Collections.Generic;
using GSC.Data.Entities.Common;

namespace GSC.Data.Entities.Barcode.Generate
{
    public class BarcodeGenerate : BaseEntity
    {
        public int BarcodeTypeId { get; set; }
        public bool IsFromAttendance { get; set; }
        public int? NoOfSubjectGenerate { get; set; }
        public int ProejctDesignTemplateId { get; set; }
        public int? CompanyId { get; set; }
        public IList<BarcodeSubjectDetail> BarcodeSubjects { get; set; }
    }
}