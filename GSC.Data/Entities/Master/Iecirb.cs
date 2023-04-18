using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GSC.Common.Base;
using GSC.Common.Common;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace GSC.Data.Entities.Master
{
   public class Iecirb : BaseEntity, ICommonAduit
    {
        public int ManageSiteId { get; set; }
        public string IECIRBName { get; set; }
        public string RegistrationNumber { get; set; }
        public string IECIRBContactName { get; set; }
        public string IECIRBContactEmail { get; set; }
        public string IECIRBContactNumber { get; set; }
        public int? CompanyId { get; set; }
        public ManageSite ManageSite { get; set; }
        public List<IecirbSiteAddress> IecirbSiteAddress { get; set; }
    }
}
