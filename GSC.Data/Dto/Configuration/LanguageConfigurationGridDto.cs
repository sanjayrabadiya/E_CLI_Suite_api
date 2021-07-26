using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Configuration
{
    public class LanguageConfigurationGridDto: BaseAuditDto
    {
        public string KeyCode { get; set; }
        public string KeyName { get; set; }
        public string DefaultMessage { get; set; }
    }

    public class LanguageConfigurationDetailsGridDto : BaseAuditDto
    {
        public int LanguageConfigurationId { get; set; }
        public int LanguageId { get; set; }
        public string LanguageName { get; set; }
        public string Message { get; set; }
    }
}
