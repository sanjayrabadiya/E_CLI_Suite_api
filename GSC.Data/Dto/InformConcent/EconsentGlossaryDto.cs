using GSC.Data.Entities.Common;
using GSC.Shared.DocumentService;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentGlossaryDto : BaseAuditDto
    {
        public int EconsentSetupId { get; set; }
        public string Word { get; set; }
        public string WordMeaning { get; set; }
    }

    public class EconsentGlossaryGridDto : BaseAuditDto
    {
        public string Project { get; set; }
        public string Document { get; set; }
        public string Word { get; set; }
        public string WordMeaning { get; set; }
    }
}
