using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Project.Generalconfig
{
    public class UploadLimit:BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int Uploadlimit { get; set;}
    }
}
