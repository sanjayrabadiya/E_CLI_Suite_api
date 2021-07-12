using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerQueryDto : BaseDto
    {
        public int VolunteerId { get; set; }
        public int TableId { get; set; }
        public string FieldName { get; set; }
        public string Comment { get; set; }
        public CommentStatus QueryStatus { get; set; }
        public int ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public int UserRole { get; set; }
        public int? CompanyId { get; set; }
        public string ReasonName { get; set; }
        public string StatusName { get; set; }
        public string LatestFieldName { get; set; }
        public string VolunteerNo { get; set; }
        public bool IsDriect { get; set; }

        private DateTime? _createdDate;

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
        public string CreatedByName { get; set; }

        public ButtonQueryShow ShowButton { get; set; }
    }

    public class ButtonQueryShow
    {
        public bool? ShowEditButton { get; set; }
        public bool? ShowRespondButton { get; set; }

    }
}
