using GSC.Data.Entities.Common;

namespace GSC.Data.Dto.Barcode.Generate
{
    public class BarcodeSubjectDetailDto : BaseDto
    {
        public int? BarcodeGenerateId { get; set; }
        public int? ProjectSubjectId { get; set; }
        public string BarcodeLabelString { get; set; }
        public int? CompanyId { get; set; }
        public string ProjectNo { get; set; }
        public string PeriodNo { get; set; }
        public string VolunteerCode { get; set; }
        public string RandomizationNo { get; set; }
        public string TemmplateNo { get; set; }
        public string SubjectNo { get; set; }
    }
}