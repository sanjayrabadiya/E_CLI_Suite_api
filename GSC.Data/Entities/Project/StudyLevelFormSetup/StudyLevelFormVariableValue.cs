using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.LanguageSetup;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Entities.Project.StudyLevelFormSetup
{
    public class StudyLevelFormVariableValue : BaseEntity, ICommonAduit
    {
        public int StudyLevelFormVariableId { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public int SeqNo { get; set; }
        public string Label { get; set; }
        public string Style { get; set; }
        public TableCollectionSource? TableCollectionSource { get; set; }
        public StudyLevelFormVariable StudyLevelFormVariable { get; set; }
    }
}