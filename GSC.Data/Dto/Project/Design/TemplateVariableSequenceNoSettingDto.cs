using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Project.Design
{
    public class TemplateVariableSequenceNoSettingDto : BaseDto
    {
        public int ProjectDesignId { get; set; }
        public bool IsTemplateSeqNo { get; set; }
        public bool IsVariableSeqNo { get; set; }
        public string RepeatPrefix { get; set; }
        public int? RepeatSeqNo { get; set; }
        public int? RepeatSubSeqNo { get; set; }
        public string SeparateSign { get; set; }
    }
}
