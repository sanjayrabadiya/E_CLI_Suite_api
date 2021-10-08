using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;
namespace GSC.Data.Entities.Configuration
{
    public class LanguageConfiguration : BaseEntity, ICommonAduit
    {
        public string KeyCode { get; set; }
        public string KeyName { get; set; }
        public string DefaultMessage { get; set; }
        public bool IsReadOnlyDefaultMessage { get; set; }
        public IList<LanguageConfigurationDetails> LanguageConfigurationDetailslist { get; set; }
    }

    public class LanguageConfigurationDetails : BaseEntity, ICommonAduit
    {
        public int LanguageConfigurationId { get; set; }
        public int LanguageId { get; set; }
        public string Message { get; set; }
        public LanguageConfiguration LanguageConfiguration { get; set; }
        public Language Language { get; set; }
    }
}
