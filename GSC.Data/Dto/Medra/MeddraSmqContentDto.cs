using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraSmqContentDto : BaseDto
    {
        public int MedraConfigId { get; set; }
        public long smq_code { get; set; }
        public long term_code { get; set; }
        public int term_level { get; set; }
        public int term_scope { get; set; }
        public string term_category { get; set; }
        public int term_weight { get; set; }
        public string term_status { get; set; }
        public string term_addition_version { get; set; }
        public string term_last_modified_version { get; set; }
        public int? CompanyId { get; set; }
    }
}
