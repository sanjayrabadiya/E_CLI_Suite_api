using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class PageConfigurationFieldsDto : BaseDto
    {
        public int AppScreenId { get; set; }
        public string FieldName { get; set; }
        public string DisplayLable { get; set; }
        public bool Dependent { get; set; }
        public int? CompanyId { get; set; }
    }
    public class PageConfigurationFieldsGridDto : BaseAuditDto
    {
        public int AppScreenId { get; set; }
        public string FieldName { get; set; }
        public string DisplayLable { get; set; }
        public string AppScreen { get; set; }
        public bool Dependent { get; set; }
        public int? CompanyId { get; set; }
        public AppScreenDto AppScreens { get; set; }
    }
}
