using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.LabReportManagement
{
    public class LabReportDto : BaseDto
    {
        public int UserId { get; set; }
        [Required]
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        [Required]
        public string DocumentBase64String { get; set; }
    }

    public class LabReportGridDto : BaseAuditDto
    {
        public int UserId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
    }
}
