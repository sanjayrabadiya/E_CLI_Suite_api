using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.IDVerificationSystem
{
    public class IDVerificationDto : BaseDto
    {
        public int UserId { get; set; }
        public bool IsUpload { get; set; }
        public int? VerifyOrRejectBy { get; set; }
        public DocumentVerifyStatus VerifyStatus { get; set; }
        public List<IDVerificationFileDto> IDVerificationFiles { get; set; }
    }

    public class IDVerificationUpdateDto : BaseDto
    {
        public int UserId { get; set; }
        public bool IsUpload { get; set; }
        public int? VerifyOrRejectBy { get; set; }
        public DocumentVerifyStatus VerifyStatus { get; set; }
    }
}
