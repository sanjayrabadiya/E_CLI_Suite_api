using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class Dictionary: BaseEntity
    {
        public string DictionaryCode { get; set; }
        public string DictionaryName { get; set; }
        public int? CompanyId { get; set; }
    }
}
