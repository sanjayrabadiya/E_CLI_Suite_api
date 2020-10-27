using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentChatDto
    {
        public int Id { get; set; }
        public int SenderId { get; set; }
        public int ReceiverId { get; set; }
        public string Message { get; set; }
        //public string SenderType { get; set; }
        public DateTime SendDateTime { get; set; }
        public bool IsDelivered { get; set; }
        public DateTime? DeliveredDateTime { get; set; }
        public bool IsRead { get; set; }
        public DateTime? ReadDateTIme { get; set; }
        public bool IsDocument { get; set; }
        public string DocumentPath { get; set; }
    }
}
