using System;
using System.Collections.Generic;
using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Barcode.Generate
{
    public class BarcodeGenerateDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int ProjectDesignPeriodId { get; set; }
        public int ProejctDesignTemplateId { get; set; }
        public int BarcodeTypeId { get; set; }
        public bool IsFromAttendance { get; set; }
        public int? NoOfSubjectGenerate { get; set; }
        public int[] Ids { get; set; }
        public int? CompanyId { get; set; }
        public string BarcodeLabelString { get; set; }
        public IList<BarcodeSubjectDetailDto> BarcodeSubjects { get; set; }
        public string ProjectCode { get; set; }
        public string TemplateCode { get; set; }
        public string VolunteerName { get; set; }
        public string VolunteerNo { get; set; }
        public string GeneratedBy { get; set; }
        public DateTime? GeneratedOn { get; set; }
        public string SubjectNumber { get; set; }
    }
}