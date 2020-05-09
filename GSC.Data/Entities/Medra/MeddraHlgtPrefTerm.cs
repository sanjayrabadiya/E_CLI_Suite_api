using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraHlgtPrefTerm : BaseEntity
    {
        public int MedraConfigId { get; set; }
        public long hlgt_code { get; set; }
        public string hlgt_name { get; set; }
        public string hlgt_whoart_code { get; set; }
        public long? hlgt_harts_code { get; set; }
        public string hlgt_costart_sym { get; set; }
        public string hlgt_icd9_code { get; set; }
        public string hlgt_icd9cm_code { get; set; }
        public string hlgt_icd10_code { get; set; }
        public string hlgt_jart_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
