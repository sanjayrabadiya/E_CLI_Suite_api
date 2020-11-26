using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class Dictionary: BaseEntity, ICommonAduit
    {
        public string DictionaryCode { get; set; }
        public string DictionaryName { get; set; }
        public int? CompanyId { get; set; }
    }
}
