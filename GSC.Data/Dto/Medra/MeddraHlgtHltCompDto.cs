using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MeddraHlgtHltCompDto : BaseDto
    {
        public int MedraConfigId { get; set; }
        public long hlgt_code { get; set; }
        public long hlt_code { get; set; }
        public int? CompanyId { get; set; }
    }
}
