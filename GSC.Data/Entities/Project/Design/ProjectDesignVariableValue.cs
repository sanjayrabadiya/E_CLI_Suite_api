using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.LanguageSetup;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Entities.Project.Design
{
    public class ProjectDesignVariableValue : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVariableId { get; set; }
        public string ValueCode { get; set; }
        public string ValueName { get; set; }
        public double? InActiveVersion{ get; set; }
        public double? StudyVersion { get; set; }
        public int SeqNo { get; set; }
        public string Label { get; set; }
        public TableCollectionSource? TableCollectionSource { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public List<VariableValueLanguage> VariableValueLanguage { get; set; }
    }
}