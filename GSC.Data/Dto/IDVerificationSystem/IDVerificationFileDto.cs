using GSC.Data.Entities.Common;
using GSC.Data.Entities.IDVerificationSystem;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.IDVerificationSystem
{
    public class IDVerificationFileDto : BaseDto
    {
        public int IDVerificationId { get; set; }
        [Required]
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        [Required]
        public string DocumentBase64String { get; set; }
    }
}
