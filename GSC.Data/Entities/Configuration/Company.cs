﻿using System.ComponentModel.DataAnnotations.Schema;
using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Configuration
{
    public class Company : BaseEntity, ICommonAduit
    {
        public string CompanyCode { get; set; }
        public string CompanyName { get; set; }
        public string Phone1 { get; set; }
        public string Phone2 { get; set; }

        [ForeignKey("LocationId")] public Location.Location Location { get; set; }
        public int LocationId { get; set; }

        public string Logo { get; set; }
    }
}