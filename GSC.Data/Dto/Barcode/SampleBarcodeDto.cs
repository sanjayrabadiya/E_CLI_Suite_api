﻿using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Barcode
{
    public class SampleBarcodeDto : BaseDto
    {
        public int? ProjectId { get; set; }
        public int? VolunteerId { get; set; }
        public int? SiteId { get; set; }
        public int? VisitId { get; set; }
        public int? TemplateId { get; set; }
        public int? BarcodeTypeId { get; set; }
        public int? PKBarcodeOption { get; set; }
        public string BarcodeString { get; set; }
        public DateTime? BarcodeDate { get; set; }
        public bool IsBarcodeReprint { get; set; }
    }

    public class SampleBarcodeGridDto : BaseAuditDto
    {
        public bool isSelected { get; set; }
        public bool isBarcodeGenerated { get; set; }
        public string Project { get; set; }
        public string VolunteerName { get; set; }
        public string Site { get; set; }
        public string Visit { get; set; }
        public string Template { get; set; }
        public string ProjectCode { get; set; }
        public string SiteCode { get; set; }
        public string BarcodeType { get; set; }
        public string PKBarcodeOption { get; set; }
        public string BarcodeString { get; set; }
        public DateTime? BarcodeDate { get; set; }
        public bool IsBarcodeReprint { get; set; }
    }
}
