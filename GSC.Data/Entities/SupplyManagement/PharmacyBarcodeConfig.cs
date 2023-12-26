using GSC.Common.Base;
using GSC.Common.Common;

using GSC.Helper;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Barcode
{
    public class PharmacyBarcodeConfig : BaseEntity
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }
        public BarcodeModuleType BarcodeModuleType { get; set; }
        public BarcodeTypes BarcodeType { get; set; }
        public bool DisplayValue { get; set; }
        public int? FontSize { get; set; }
        public int? DisplayInformationLength { get; set; }
        
        public GSC.Data.Entities.Master.Project Project { get; set; }
        [ForeignKey("SiteId")]
        public GSC.Data.Entities.Master.Project Site { get; set; }
       
        public IList<PharmacyBarcodeDisplayInfo> BarcodeDisplayInfo { get; set; } = null;
        

    }
}