using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Barcode
{
    public class Centrifugation : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public string MachineName { get; set; }
        public int RCMRPM { get; set; }
        public int Min { get; set; }
        public int Temprature { get; set; }

        public GSC.Data.Entities.Master.Project Project { get; set; }

    }
}
