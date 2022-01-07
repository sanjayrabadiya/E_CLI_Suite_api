using GSC.Common.Base;
using GSC.Common.Common;
using System;

namespace GSC.Data.Entities.LabManagement
{
    public class LabManagementUploadExcelData : BaseEntity, ICommonAduit
    {
        public int LabManagementUploadDataId { get; set; }
        public string ScreeningNo { get; set; }
        public string RandomizationNo { get; set; }
        public string Visit { get; set; }
        public string RepeatSampleCollection { get; set; }
        public string LaboratryName { get; set; }
        public DateTime? DateOfSampleCollection { get; set; }
        public DateTime? DateOfReport { get; set; }
        public string Panel { get; set; }
        public string TestName { get; set; }
        public string Result { get; set; }
        public string Unit { get; set; }
        public string AbnoramalFlag { get; set; }
        public string ReferenceRangeLow { get; set; }
        public string ReferenceRangeHigh { get; set; }
        // public string ClinicallySignificant { get; set; }
        public LabManagementUploadData LabManagementUploadData { get; set; }

    }
}
