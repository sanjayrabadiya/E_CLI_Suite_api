using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Master
{
    public class SiteContractRepository : GenericRespository<SiteContract>, ISiteContractRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public SiteContractRepository(IGSCContext context, IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }

        public IList<SiteContractGridDto> GetSiteContractList(bool isDeleted, int studyId, int siteId)
        {
            var query = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == studyId);

            if (siteId != 0)
            {
                query = query.Where(x => x.SiteId == siteId);
            }

            var siteContractGridData = query.ProjectTo<SiteContractGridDto>(_mapper.ConfigurationProvider)
                                            .OrderByDescending(x => x.Id)
                                            .ToList();

            foreach (var item in siteContractGridData)
            {
                item.SiteName = _context.Project
                                        .Include(s => s.ManageSite)
                                        .Where(w => w.Id == item.SiteId)
                                        .Select(d => d.ProjectCode ?? d.ManageSite.SiteName)
                                        .FirstOrDefault();
            }

            return siteContractGridData;
        }
    }
}
