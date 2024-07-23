using AutoMapper;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AutoMapper.QueryableExtensions;

namespace GSC.Respository.CTMS
{
    public class PassthroughMilestoneInvoiceRepository : GenericRespository<PassthroughMilestoneInvoice>, IPassthroughMilestoneInvoiceRepository
    {
        private readonly IMapper _mapper;
        public PassthroughMilestoneInvoiceRepository(IGSCContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public string Duplicate(PassthroughMilestoneInvoice passthroughMilestoneInvoice)
        {
            if (All.Any(x =>
               x.Id != passthroughMilestoneInvoice.Id && x.PassthroughMilestoneId == passthroughMilestoneInvoice.PassthroughMilestoneId && x.DeletedDate == null))
                return "Duplicate Passthrough Milestone Invoice";
            return "";
        }

        public List<PassthroughMilestoneInvoiceGridDto> GetPassthroughMilestoneInvoiceList(bool isDeleted)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                   ProjectTo<PassthroughMilestoneInvoiceGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public PassthroughMilestoneInvoiceDto GetPassthroughMilestoneById(int milestoneId)
        {
            var value = All.FirstOrDefault(x => x.DeletedDate == null && x.PassthroughMilestoneId == milestoneId);
            return _mapper.Map<PassthroughMilestoneInvoiceDto>(value);
        }
    }
}
