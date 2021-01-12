using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.LanguageSetup
{
    public class VariableCategoryLanguage : BaseEntity, ICommonAduit
    {
        public int VariableCategoryId { get; set; }
        public int LanguageId { get; set; }
        public string Display { get; set; }
        public VariableCategory VariableCategory { get; set; }
        public Language Language { get; set; }
    }
}
