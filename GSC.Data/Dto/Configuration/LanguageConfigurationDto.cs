using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Configuration
{
    public class LanguageConfigurationDto: BaseDto
    {
        public string KeyCode { get; set; }
        public string KeyName { get; set; }
        public string Message { get; set; }
        //public IList<LanguageConfigurationDetailsDto> LanguageConfigurationDetailslist { get; set; }
    }

    public class LanguageConfigurationDetailsDto : BaseDto
    {
        public int LanguageConfigurationId { get; set; }
        public int LanguageId { get; set; }       
        public string Message { get; set; }
    }
}
