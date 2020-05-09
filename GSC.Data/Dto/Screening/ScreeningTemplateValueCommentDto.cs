using System;
using GSC.Data.Entities.Common;
using GSC.Helper;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateValueCommentDto : BaseDto
    {
        private DateTime? _createdDate;
        public int ScreeningTemplateValueId { get; set; }
        public string Comment { get; set; }
        public string RoleName { get; set; }
        public string CreatedByName { get; set; }

        public DateTime? CreatedDate
        {
            get => _createdDate?.UtcDateTime();
            set => _createdDate = value?.UtcDateTime();
        }
    }
}