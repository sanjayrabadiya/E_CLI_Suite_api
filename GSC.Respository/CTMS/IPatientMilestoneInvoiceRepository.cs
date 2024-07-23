using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public interface IPatientMilestoneInvoiceRepository : IGenericRepository<PatientMilestoneInvoice>
    {
        string Duplicate(PatientMilestoneInvoice patientMilestoneInvoice);
        List<PatientMilestoneInvoiceGridDto> GetPatientMilestoneInvoiceList(bool isDeleted);
        PatientMilestoneInvoiceDto GetPatientMilestoneInvoiceById(int milestoneId);
    }
}
