using System;
using GSC.Data.Entities.Common;
using GSC.Shared.Extension;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerBlockHistoryDto : BaseDto
    {
        public DateTime? BlockDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public int VolunteerId { get; set; }
        public int? BlockCategoryId { get; set; }
        public bool IsPermanently { get; set; }
        public bool IsBlock { get; set; }
        public string BlockString { get; set; }
        public string UserName { get; set; }
        public string PermanentlyString { get; set; }
        public string CategoryName { get; set; }
        public string Note { get; set; }
    }

    public class VolunteerBlockHistoryGridDto : BaseAuditDto
    {
        public DateTime? BlockDate { get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
        public bool IsPermanently { get; set; }
        public bool IsBlock { get; set; }
        public string PermanentlyString { get; set; }
        public string CategoryName { get; set; }
        public string Note { get; set; }
    }
}