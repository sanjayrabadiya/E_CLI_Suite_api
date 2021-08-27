﻿using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
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
    }
}
