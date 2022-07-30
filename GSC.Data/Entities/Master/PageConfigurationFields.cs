using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class PageConfigurationFields : BaseEntity, ICommonAduit
    {
        public int AppScreenId { get; set; }
        public string ActualFieldName { get; set; }
        public string FieldName { get; set; }
        public string DisplayLable { get; set; }
        public bool Dependent { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey("AppScreenId")]
        public AppScreen AppScreens { get; set; }
    }
}
