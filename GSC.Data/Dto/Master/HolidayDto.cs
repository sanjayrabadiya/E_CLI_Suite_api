﻿using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.ComponentModel.DataAnnotations;

namespace GSC.Data.Dto.Master
{
    public class HolidayDto : BaseDto
    {
        public int InvestigatorContactId { get; set; }
        [Required(ErrorMessage = "Holiday Type is required.")]
        public HolidayType HolidayType { get; set; }
        [Required(ErrorMessage = "Holiday Name is required.")]
        public string HolidayName { get; set; }
        [Required(ErrorMessage = "Holiday Date is required.")]
        //public DateTime? HolidayDate { get; set; }
        private DateTime? _approveDate;
        public DateTime? HolidayDate
        {
            get => _approveDate?.UtcDateTime();
            set => _approveDate = value?.UtcDateTime();
        }
        public string Description { get; set; }
        public int? CompanyId { get; set; }
        public string HolidayTypeName { get; set; }

    }
}