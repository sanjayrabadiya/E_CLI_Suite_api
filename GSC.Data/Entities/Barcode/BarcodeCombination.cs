using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Barcode
{
    public class BarcodeCombination : BaseEntity, ICommonAduit
    {
        public int BarcodConfigId { get; set; }
        public int? TableFieldNameId { get; set; }
        public string Combination { get; set; }
        public TableFieldName TableFieldName { get; set; }

        [ForeignKey("BarcodConfigId")]
        public BarcodeConfig BarcodeConfig { get; set; }
    }
}
