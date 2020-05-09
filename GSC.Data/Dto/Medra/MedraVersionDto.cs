using GSC.Data.Entities.Common;
using GSC.Data.Entities.Medra;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Medra
{
    public class MedraVersionDto : BaseDto
    {
        public int DictionaryId { get; set; }
        public string Version { get; set; }
        public string Description { get; set; }
        public int? CompanyId { get; set; }
        public string DictionaryName { get; set; }
        public Dictionary Dictionary { get; set; }
    }
}
