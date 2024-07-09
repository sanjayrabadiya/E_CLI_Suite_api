using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
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
        public SiteContractRepository(IGSCContext context,
            IMapper mapper)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public IList<SiteContractGridDto> GetSiteContractList(bool isDeleted, int studyId, int siteId)
        {
            var SiteContractGridData = new List<SiteContractGridDto>();
            if (studyId != 0 && siteId != 0)
            {
                SiteContractGridData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (x.ProjectId == studyId) && x.SiteId == siteId).
                             ProjectTo<SiteContractGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            }
            else
            {
                SiteContractGridData = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && (x.ProjectId == studyId)).
                             ProjectTo<SiteContractGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            }
            foreach (var item in SiteContractGridData)
            {
                item.SiteName = _context.Project.Include(s => s.ManageSite).Where(w => w.Id == item.SiteId).Select(d => d.ProjectCode == null ? d.ManageSite.SiteName : d.ProjectCode).FirstOrDefault();
               
            }
            return SiteContractGridData;
        }
        public string Duplicate(SiteContractDto SiteContractDto)
        {
            if (All.Any(x =>( x.Id != SiteContractDto.Id && x.ProjectId == SiteContractDto.ProjectId && x.SiteId == SiteContractDto.SiteId && x.DeletedDate == null) ||( x.ContractCode == SiteContractDto.ContractCode)))
            {
                return "Duplicate this Site Contract";
            }
            return "";
        }
    }
}
