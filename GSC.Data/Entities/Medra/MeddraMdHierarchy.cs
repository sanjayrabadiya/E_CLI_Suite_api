using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraMdHierarchy : BaseEntity
    {
        public int MedraConfigId { get; set; }
        public long pt_code { get; set; }
        public long hlt_code { get; set; }
        public long hlgt_code { get; set; }
        public long soc_code { get; set; }
        public string pt_name { get; set; }
        public string hlt_name { get; set; }
        public string hlgt_name { get; set; }
        public string soc_name { get; set; }
        public string soc_abbrev { get; set; }
        public string null_field { get; set; }
        public long? pt_soc_code { get; set; }
        public string primary_soc_fg { get; set; }
        public int? CompanyId { get; set; }
    }
}
