using GSC.Common.Base;

namespace GSC.Data.Entities.CTMS
{
    public class BudgetPaymentFinalCost : BaseEntity
    {
        public int ProjectId { get; set; }

        public decimal ProfessionalCostAmount { get; set; }
        public decimal PatientCostAmount { get; set; }

        public decimal PassThroughCost { get; set; }
        public decimal TotalAmount { get; set; }

        public int ProfessionalCostPerc { get; set; }

        public int PatientCostPerc { get; set; }

        public int PassThroughPerc { get; set; }

        public decimal ProfessionalCostActualPayableAmount { get; set; }

        public decimal PatientCostActualPayableAmount { get; set; }

        public decimal PassThroughCostPayableAmount { get; set; }

        public string IpAddress { get; set; }

        public string TimeZone { get; set; }
        public Entities.Master.Project Project { get; set; }

    }
}
