using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Barcode
{
    public class BarcodeDisplayInfo : BaseEntity, ICommonAduit
    {
        public int BarcodConfigId { get; set; }
        public int? TableFieldNameId { get; set; }
        public TableFieldName TableFieldName { get; set; }
        public string DisplayInformation { get; set; }
        public int? OrderNumber { get; set; }
        public int? AlignmentId { get; set; }
        public string Alignment { get; set; }
        public bool? IsSameLine { get; set; }
        [ForeignKey("BarcodConfigId")]
        public BarcodeConfig BarcodeConfig { get; set; }
    }
}
