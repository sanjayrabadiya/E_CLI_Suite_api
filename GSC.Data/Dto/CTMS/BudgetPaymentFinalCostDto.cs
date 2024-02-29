using GSC.Data.Entities.Common;
using GSC.Helper;
using System;

namespace GSC.Data.Dto.CTMS
{
    public class BudgetPaymentFinalCostDto : BaseDto
    {
        public int ProjectId { get; set; }

        public decimal ProfessionalCostAmount { get; set; }
        public decimal PatientCostAmount { get; set; }

        public decimal PassThroughCost { get; set; }
        public decimal TotalAmount { get; set; }

        public int ProfessionalCostPerc { get; set; }

        public int PatientCostPerc { get; set; }

        public int PassThroughPerc { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

        public decimal ProfessionalCostActualPayableAmount { get; set; }

        public decimal PatientCostActualPayableAmount { get; set; }

        public decimal PassThroughCostPayableAmount { get; set; }
    }

    public class BudgetPaymentFinalCostGridDto : BaseAuditDto
    {
        public int ProjectId { get; set; }

        public  string ProjectCode { get; set; }
        public decimal ProfessionalCostAmount { get; set; }
        public decimal PatientCostAmount { get; set; }

        public decimal PassThroughCost { get; set; }
        public decimal TotalAmount { get; set; }

        public int ProfessionalCostPerc { get; set; }

        public int PatientCostPerc { get; set; }

        public int PassThroughPerc { get; set; }
        public string IpAddress { get; set; }

        public string TimeZone { get; set; }

        public decimal ProfessionalCostActualPayableAmount { get; set; }

        public decimal PatientCostActualPayableAmount { get; set; }

        public decimal PassThroughCostPayableAmount { get; set; }
    }
}
