using System.Collections.Generic;
using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.UserMgt;

namespace GSC.Data.Entities.Project.StudyLevelFormSetup
{
    public class StudyLevelForm : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int AppScreenId { get; set; }
        public int ActivityId { get; set; }
        public int VariableTemplateId { get; set; }
        public Master.Project Project { get; set; }
        public AppScreen AppScreen { get; set; }
        public Activity Activity { get; set; }
        public VariableTemplate VariableTemplate { get; set; }
        public IList<StudyLevelFormVariable> Variables { get; set; }
    }
}