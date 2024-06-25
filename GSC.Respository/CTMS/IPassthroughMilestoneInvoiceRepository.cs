using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public interface IPassthroughMilestoneInvoiceRepository : IGenericRepository<PassthroughMilestoneInvoice>
    {
        string Duplicate(PassthroughMilestoneInvoice passthroughMilestoneInvoice);
        List<PassthroughMilestoneInvoiceGridDto> GetPassthroughMilestoneInvoiceList(bool isDeleted);
        PassthroughMilestoneInvoiceDto GetPassthroughMilestoneById(int milestoneId);
    }
}
