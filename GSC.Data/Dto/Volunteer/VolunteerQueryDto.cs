﻿using GSC.Data.Entities.Common;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;

namespace GSC.Data.Dto.Volunteer
{
    public class VolunteerQueryDto : BaseDto
    {
        public int VolunteerId { get; set; }
        public int TableId { get; set; }
        public string FieldName { get; set; }
        public string Comment { get; set; }
        public CommentStatus QueryStatus { get; set; }
        public QueryTypes QueryType { get; set; }
        public int ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public int UserRole { get; set; }
        public int? CompanyId { get; set; }
        public string ReasonName { get; set; }
        public string StatusName { get; set; }
        public string LatestFieldName { get; set; }
        public string VolunteerNo { get; set; }
        public bool IsDriect { get; set; }
        public bool IsAnswered { get; set; }
        public int CreatedBy { get; set; }
        private DateTime? _createdDate;
        public string OldValue { get; set; }
        public string NewValue { get; set; }
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
        public string CreatedByName { get; set; }
        public string QueryTypeName { get; set; }
        public ButtonQueryShow ShowButton { get; set; }
        public List<ScreeningHistory> ScreeningHistory { get; set; }
    }

    public class ButtonQueryShow
    {
        public bool? ShowEditButton { get; set; }
        public bool? ShowRespondButton { get; set; }
    }

    public class VolunteerQuerySearchDto
    {
        public int Id { get; set; }
        public DateTime? FromRegistration { get; set; }
        public DateTime? ToRegistration { get; set; }
        public CommentStatus? Status { get; set; }
        public int? User { get; set; }
        public int? Role { get; set; }
        public int? StudyId { get; set; }
    }
}
