using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class PageConfiguration : BaseEntity, ICommonAduit
    {
        public int ActualField { get; set; }
        public string DisplayField { get; set; }
        public bool IsVisible { get; set; }
        public bool IsRequired { get; set; }
        public bool Dependent { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey("ActualField")]
        public PageConfigurationFields PageConfigurationFields { get; set; }
    }
}
