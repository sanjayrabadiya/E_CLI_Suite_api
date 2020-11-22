using GSC.Common.Base;
using GSC.Data.Entities.Master;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Medra
{
    public class MedraConfig : BaseEntity
    {
        public int MedraVersionId { get; set; }
        public int LanguageId { get; set; }
        public string PathName { get; set; }
        public string MimeType { get; set; }
        public string Description { get; set; }
        public string Password { get; set; }
        public int? CompanyId { get; set; }
        public bool IsActive { get; set; }
        public MedraVersion MedraVersion { get; set; }
        public MedraLanguage Language { get; set; }
        public int? CreatedRole { get; set; }
    }
}
