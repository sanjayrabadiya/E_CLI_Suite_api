using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class ManageSiteAddressDto : BaseDto
    {
        public string SiteAddress { get; set; }
        public int ManageSiteId { get; set; }
        public ManageSiteDto ManageSite { get; set; }
    }
}
