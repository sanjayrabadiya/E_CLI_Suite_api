using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.Design
{
    public class TemplateVariableSequenceNoSetting : BaseEntity, ICommonAduit
    {
        public int ProjectDesignId { get; set; }

        public bool IsTemplateSeqNo { get; set; }
        public bool IsVariableSeqNo { get; set; }
        public string RepeatPrefix { get; set; }
        public int? RepeatSeqNo { get; set; }
        public int? RepeatSubSeqNo { get; set; }
        public string SeparateSign { get; set; }
        public ProjectDesign ProjectDesign { get; set; }
    }
}
