using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Data.Dto.CTMS
{
    public class ResourceMilestoneInvoiceDto : BaseDto
    {
        public int ResourceMilestoneId { get; set; }
        public int PaymentTermsId { get; set; }
        public bool IsInvoiceGenerated { get; set; }
        public DateTime? InvoiceGeneratedDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public string PaymentDescription { get; set; }
        public bool IsPaymentRecived { get; set; }
    }

    public class ResourceMilestoneInvoiceGridDto : BaseAuditDto
    {
        public int ResourceMilestoneId { get; set; }
        public int PaymentTermsId { get; set; }
        public bool IsInvoiceGenerated { get; set; }
        public DateTime? InvoiceGeneratedDate { get; set; }
        public DateTime? PaymentDueDate { get; set; }
        public string PaymentDescription { get; set; }
        public bool IsPaymentRecived { get; set; }
    }
}
