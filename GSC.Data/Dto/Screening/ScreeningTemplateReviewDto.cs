using System;
using GSC.Shared;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateReviewDto
    {
        public string Status { get; set; }
        public short ReviewLevel { get; set; }
        private DateTime? _date { get; set; }
        public string OfficerName { get; set; }
        public string RoleName { get; set; }

        public DateTime? Date
        {
            get => _date.UtcDate();
            set => _date = value == DateTime.MinValue ? value : value.UtcDate();
        }

        public bool IsRepeat { get; set; }
        public int ProjectDesignTemplateId { get; set; }
    }
}