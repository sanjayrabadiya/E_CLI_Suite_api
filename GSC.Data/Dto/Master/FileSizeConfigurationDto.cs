using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class FileSizeConfigurationDto : BaseDto
    {
        public int ScreenId { get; set; }
        public int FileSize { get; set; }
        public int? CompanyId { get; set; }
        public AppScreenDto AppScreens { get; set; }
    }

    public class FileSizeConfigurationGridDto : BaseAuditDto
    {
        public int ScreenId { get; set; }
        public string ScreenCode { get; set; }
        public string ScreenName { get; set; }
        public int FileSize { get; set; }
    }
}
