using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.IDVerificationSystem
{
    public class IDVerificationFile : BaseEntity, ICommonAduit
    {
        public int IDVerificationId { get; set; }
        public string DocumentName { get; set; }
        public string DocumentPath { get; set; }
        public IDVerification IDVerification { get; set; }
    }
}
