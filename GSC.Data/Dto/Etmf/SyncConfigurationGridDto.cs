using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class SyncConfigurationMasterGridDto : BaseAuditDto
    {
        public string ReportName { get; set; }
        public string Version { get; set; }
    }
}
