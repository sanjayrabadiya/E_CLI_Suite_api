using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;

namespace GSC.Respository.Master
{
    public class PassthroughSiteContractRepository : GenericRespository<PassthroughSiteContract>, IPassthroughSiteContractRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public PassthroughSiteContractRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public IList<PassthroughSiteContractGridDto> GetPassthroughSiteContractList(bool isDeleted, int siteContractId)
        {
            return All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.SiteContractId == siteContractId).
                ProjectTo<PassthroughSiteContractGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }
    }
}
