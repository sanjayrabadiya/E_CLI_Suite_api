using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MedraVersion:BaseEntity, ICommonAduit
    {
        public int DictionaryId { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public int? CompanyId { get; set; }
        public Dictionary Dictionary { get; set; }
    }
}
