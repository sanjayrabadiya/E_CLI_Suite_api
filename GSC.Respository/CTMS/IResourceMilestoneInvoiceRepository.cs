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
    public interface IResourceMilestoneInvoiceRepository : IGenericRepository<ResourceMilestoneInvoice>
    {
        string Duplicate(ResourceMilestoneInvoice resourceMilestoneInvoice);
        List<ResourceMilestoneInvoiceGridDto> GetResourceMilestoneInvoiceList(bool isDeleted);
        ResourceMilestoneInvoiceDto GetResourceMilestoneInvoiceById(int milestoneId);
    }
}
