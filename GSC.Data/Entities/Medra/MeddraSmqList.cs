using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MeddraSmqList : BaseEntity
    {
        public int MedraConfigId { get; set; }
        public long smq_code { get; set; }
        public string smq_name { get; set; }
        public int smq_level { get; set; }
        public string smq_description { get; set; }
        public string smq_source { get; set; }
        public string smq_note { get; set; }
        public string MedDRA_version { get; set; }
        public string status { get; set; }
        public string smq_algorithm { get; set; }
        public int? CompanyId { get; set; }
    }
}
