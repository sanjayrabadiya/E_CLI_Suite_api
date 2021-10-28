using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class SyncConfigurationGridDto: BaseDto
    {
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public string ModuleName { get; set; }
    }
}
