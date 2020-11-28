using GSC.Data.Entities.Common;
using GSC.Helper;
using GSC.Shared.Extension;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraCodingCommentDto : BaseDto
    {
        public int MeddraCodingId { get; set; }
        public string Value { get; set; }
        public string OldValue { get; set; }
        public int? ReasonId { get; set; }
        public string ReasonOth { get; set; }
        public CommentStatus CommentStatus { get; set; }
        public string Note { get; set; }
        public int UserRole { get; set; }
        public int? CompanyId { get; set; }
        public int ScreeningTemplateValueId { get; set; }
        public int MeddraLowLevelTermId { get; set; }
        public int? MeddraSocTermId { get; set; }
        public long? OldPTCode { get; set; }
        public long? NewPTCode { get; set; }
        public string ReasonName { get; set; }
        public string StatusName { get; set; }
        private DateTime? _createdDate;
        
        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
        public string CreatedByName { get; set; }
        public ButtonShow ShowButton { get; set; }
    }

    public class ButtonShow
    {
        public bool? ShowCommentButton { get; set; }
        public bool? ShowRespondButton { get; set; }

    }
}
