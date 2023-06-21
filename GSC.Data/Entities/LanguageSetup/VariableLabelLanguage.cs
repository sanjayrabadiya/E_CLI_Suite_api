using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.LanguageSetup
{
    public class VariableLabelLanguage : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVariableId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public ProjectDesignVariable ProjectDesignVariable { get; set; }
        public Language Language { get; set; }
    }
}
