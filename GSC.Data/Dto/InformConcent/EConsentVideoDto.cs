using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EConsentVideoDto: BaseDto
    {
        public int SenderUserId { get; set; }
        public int ReceiverUserId { get; set; }
        public string SessionId { get; set; }
        public string Publishertoken { get; set; }
        public string ApiKey { get; set; }
        public string Subscribertoken { get; set; }
        public bool RequestDelivered { get; set; }
        public string SenderUserName { get; set; }
        public string ReceiverUserName { get; set; }
        public VideoCallStatus? CallStatus { get; set; }
        public VideoCallStatusCallEndBy? EndCallBy { get; set; }
        public DateTime? CallStartTime { get; set; }
        public DateTime? CallEndTime { get; set; }
    }
}
