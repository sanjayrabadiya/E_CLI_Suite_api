using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.LanguageSetup;
using GSC.Helper;
using System.Collections.Generic;

namespace GSC.Data.Entities.Master
{
    public class VariableCategory : BaseEntity, ICommonAduit
    {
        public string CategoryCode { get; set; }

        public string CategoryName { get; set; }
        public int? CompanyId { get; set; }
        public VariableCategoryType? SystemType { get; set; }
        public List<VariableCategoryLanguage> VariableCategoryLanguage { get; set; }
    }
}