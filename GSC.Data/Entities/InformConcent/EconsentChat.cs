using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.InformConcent
{
    public class EconsentChat : BaseEntity, ICommonAduit
    {
        //public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
        public string Salt { get; set; }
        //public string SenderType { get; set; }
        public DateTime SendDateTime { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime? DeliveredDateTime { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDateTIme { get; set; }
        public bool IsDocument { get; set; }
        public string DocumentPath { get; set; }
        [ForeignKey("SenderId")]
        public User Sender { get; set; }
        [ForeignKey("ReceiverId")]
        public User Receiver { get; set; }
    }
}
