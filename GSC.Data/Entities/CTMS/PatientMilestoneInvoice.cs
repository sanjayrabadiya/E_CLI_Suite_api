using GSC.Common.Base;
using GSC.Common.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Data.Entities.CTMS
{
    public class PatientMilestoneInvoice : BaseEntity, ICommonAduit
    {
        public int PatientMilestoneId { get; set; }
        public int PaymentTermsId { get; set; }
        public bool IsInvoiceGenerated { get; set; }
        public DateTime? InvoiceGeneratedDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public string PaymentDescription { get; set; }
        public bool IsPaymentRecived { get; set; }
    }
}
