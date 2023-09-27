using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.IDVerificationSystem
{
    public class IDVerification : BaseEntity, ICommonAduit
    {
        public int UserId { get; set; }
        public bool IsUpload { get; set; }
        public int? VerifyOrRejectBy { get; set; }
        public DocumentVerifyStatus VerifyStatus { get; set; }
        public User User { get; set; }
        [ForeignKey("VerifyOrRejectBy")]
        public User VerifyOrRejectByUser { get; set; }
        public List<IDVerificationFile> IDVerificationFiles { get; set; }
    }
}
