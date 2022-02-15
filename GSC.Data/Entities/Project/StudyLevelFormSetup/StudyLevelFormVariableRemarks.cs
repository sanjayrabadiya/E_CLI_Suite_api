using GSC.Common.Base;
using GSC.Common.Common;

namespace GSC.Data.Entities.Project.StudyLevelFormSetup
{
    public class StudyLevelFormVariableRemarks : BaseEntity, ICommonAduit
    {
        public int StudyLevelFormVariableId { get; set; }
        public int Range { get; set; }
        public string Remarks { get; set; }
        public int SeqNo { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
    }
}
