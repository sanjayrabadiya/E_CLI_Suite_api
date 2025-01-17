﻿using GSC.Data.Entities.Common;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.CTMS
{
    public class HolidayMasterDto : BaseDto
    {
        public int ProjectId { get; set; }
        public int? SiteId { get; set; }

        [Required(ErrorMessage = "Holiday Name is required.")]
        public string HolidayName { get; set; }
        public DateTime FromDate { get; set; }
        public DateTime ToDate { get; set; }
        public bool IsSite { get; set; }
    }
    public class HolidayMasterListDto
    {
        public string Label { get; set; }
        public DateTime From { get; set; }
        public DateTime To { get; set; }
        public string IpAddress { get; set; }
        public string TimeZone { get; set; }
    }
}
