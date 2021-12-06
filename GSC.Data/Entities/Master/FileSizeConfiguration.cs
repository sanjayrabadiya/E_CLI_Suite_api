using GSC.Common.Base;
using GSC.Common.Common;
using GSC.Data.Entities.UserMgt;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class FileSizeConfiguration : BaseEntity, ICommonAduit
    {
        public int ScreenId { get; set; }
        public int FileSize { get; set; }
        public int? CompanyId { get; set; }
        [ForeignKey("ScreenId")]
        public AppScreen AppScreens { get; set; }
    }
}
