using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace GSC.Data.Entities.UserMgt
{
    public class AppScreenPatientRights : BaseEntity, ICommonAduit
    {
        public int ProjectId { get; set; }
        public int AppScreenPatientId { get; set; }
        [ForeignKey("AppScreenPatientId")] 
        public AppScreenPatient AppScreenPatient { get; set; }
        
    }
}
