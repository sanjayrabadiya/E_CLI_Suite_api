using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Etmf
{
    public class ETMFWorkplaceDto : BaseDto
    {
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int ProjectId { get; set; }
        public int? CompanyId { get; set; }
    }

    public class ETMFWorkplaceGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }
        public string ProjectCode { get; set; }
        public string ProjectName { get; set; }
        public int? NoofSite { get; set; }
        public string Version { get; set; }
    }
}
