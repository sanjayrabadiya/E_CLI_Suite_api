using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Dto.Master
{
    public class PageConfigurationDto : BaseDto
    {
        public int ActualField { get; set; }
        public string DisplayField { get; set; }
        public bool IsVisible { get; set; }
        public bool IsRequired { get; set; }
        public bool Dependent { get; set; }
        public int? CompanyId { get; set; }
    }

    public class PageConfigurationCommon
    {
        public int ActualField { get; set; }
        public string DisplayField { get; set; }
        public bool IsVisible { get; set; }
        public bool IsRequired { get; set; }
        public bool Dependent { get; set; }
        public string ActualFieldName { get; set; }
    }
    public class PageConfigurationGridDto : BaseAuditDto
    {
        public int ActualField { get; set; }
        public string DisplayField { get; set; }
        public string ActualFieldName { get; set; }
        public bool IsVisible { get; set; }
        public bool IsRequired { get; set; }
        public bool Dependent { get; set; }
        public int? CompanyId { get; set; }
        public PageConfigurationFieldsDto PageConfigurationFields { get; set; }
    }
}
