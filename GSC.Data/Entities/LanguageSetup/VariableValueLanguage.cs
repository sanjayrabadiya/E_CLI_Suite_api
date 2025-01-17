﻿using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using GSC.Data.Entities.Project.Design;
using System.Collections.Generic;

namespace GSC.Data.Entities.LanguageSetup
{
    public class VariableValueLanguage : BaseEntity, ICommonAduit
    {
        public int ProjectDesignVariableValueId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public string LabelName { get; set; }
        public ProjectDesignVariableValue ProjectDesignVariableValue { get; set; }
        public Language Language { get; set; }
    }
}
