using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public class ResourceMilestoneInvoiceRepository : GenericRespository<ResourceMilestoneInvoice>, IResourceMilestoneInvoiceRepository
    {
        private readonly IMapper _mapper;
        public ResourceMilestoneInvoiceRepository(IGSCContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public string Duplicate(ResourceMilestoneInvoice resourceMilestoneInvoice)
        {
            if (All.Any(x =>
                 x.Id != resourceMilestoneInvoice.Id && x.ResourceMilestoneId == resourceMilestoneInvoice.ResourceMilestoneId && x.DeletedDate == null))
                return "Duplicate Resource Milestone Invoice";
            return "";
        }

        public ResourceMilestoneInvoiceDto GetResourceMilestoneInvoiceById(int milestoneId)
        {
            var value = All.FirstOrDefault(x => x.DeletedDate == null && x.ResourceMilestoneId == milestoneId);
            return _mapper.Map<ResourceMilestoneInvoiceDto>(value);
        }

        public List<ResourceMilestoneInvoiceGridDto> GetResourceMilestoneInvoiceList(bool isDeleted)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                   ProjectTo<ResourceMilestoneInvoiceGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
