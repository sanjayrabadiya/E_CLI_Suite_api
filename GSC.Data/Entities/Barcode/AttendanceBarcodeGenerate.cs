using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Barcode
{
    public class AttendanceBarcodeGenerate : BaseEntity
    {
        public int AttendanceId { get; set; }
        public int BarcodeConfigId { get; set; }
        public string BarcodeString { get; set; }
        public bool? IsRePrint { get; set; }
        public BarcodeConfig BarcodeConfig { get; set; }
        public Data.Entities.Attendance.Attendance Attendance { get; set; }
    }
}
