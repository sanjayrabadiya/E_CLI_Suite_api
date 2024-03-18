using GSC.Common.Base;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Barcode
{
    public class SampleBarcode : BaseEntity
    {
        public int? ProjectId { get; set; }
        public int? VolunteerId { get; set; }
        public int? SiteId { get; set; }
        public int? VisitId { get; set; }
        public int? TemplateId { get; set; }
        public int? BarcodeTypeId { get; set; }
        public PKBarcodeOption? PKBarcodeOption { get; set; }
        public string BarcodeString { get; set; }
        public DateTime? BarcodeDate { get; set; }
        public bool IsBarcodeReprint { get; set; }

        [ForeignKey("VolunteerId")]
        public Volunteer.Volunteer Volunteer { get; set; }
        [ForeignKey("ProjectId")]
        public Master.Project Project { get; set; }
        [ForeignKey("SiteId")]
        public Master.Project Site { get; set; }
        [ForeignKey("VisitId")]
        public ProjectDesignVisit ProjectDesignVisit { get; set; }
        [ForeignKey("TemplateId")]
        public ProjectDesignTemplate ProjectDesignTemplate { get; set; }
        [ForeignKey("BarcodeTypeId")]
        public BarcodeType BarcodeType { get; set; }

    }
}
