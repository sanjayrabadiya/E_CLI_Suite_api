using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EConsentVideo : BaseEntity, ICommonAduit
    {
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public string SessionId { get; set; }
        public string Publishertoken { get; set; }
        public string Subscribertoken { get; set; }
        public bool RequestDelivered { get; set; }
        public VideoCallStatus? CallStatus { get; set; }
        public VideoCallStatusCallEndBy? EndCallBy { get; set; }
        public DateTime? CallStartTime { get; set; }
        public DateTime? CallEndTime { get; set; }

    }
}
