using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Data.Entities.Master
{
    public class IecirbSiteAddress : BaseEntity, ICommonAduit
    {
        public int ManageSiteAddressId { get; set; }
        public int IecirbId { get; set; }
        public ManageSiteAddress ManageSiteAddress { get; set; }
        public Iecirb Iecirb { get; set; }
    }
}
