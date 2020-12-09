using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Common;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Screening
{
    public class ScreeningTemplateValueMobileDto : BaseDto
    {
        public int ScreeningTemplateId { get; set; }
        public int ProjectDesignVariableId { get; set; }
        public string Value { get; set; }
        public string Text { get; set; }
        public string OldValue { get; set; }
        public string TimeZone { get; set; }
        public ICollection<ScreeningTemplateValueChildDto> Children { get; set; }
        public string ValueName { get; set; }
        public bool IsNa { get; set; }
        public int? UserRoleId { get; set; }
        public bool IsSystem { get; set; }
        public List<EditCheckIds> EditCheckIds { get; set; }
        public CollectionSources? CollectionSource { get; set; }
        public ScreeningTemplateStatus ScreeningStatus { get; set; }
        public int ProjectDesignTemplateId { get; set; }
    }
}
