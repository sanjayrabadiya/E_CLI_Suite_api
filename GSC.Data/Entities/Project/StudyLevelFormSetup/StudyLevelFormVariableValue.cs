using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.LanguageSetup;
using System.Collections.Generic;

namespace GSC.Data.Entities.Project.StudyLevelFormSetup
{
    public class StudyLevelFormVariableValue : BaseEntity, ICommonAduit
    {
        public int StudyLevelFormVariableId { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public double? InActiveVersion{ get; set; }
        public double? StudyVersion { get; set; }
        public int SeqNo { get; set; }
        public string Label { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
    }
}