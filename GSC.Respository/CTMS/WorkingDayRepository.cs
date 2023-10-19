using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class WorkingDayRepository : GenericRespository<WorkingDay>, IWorkingDayRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public WorkingDayRepository(IGSCContext context,
            IMapper mapper) : base(context)
        {
            _mapper = mapper;
            _context = context;
        }
        public List<WorkingDayListDto> GetWorkingDayList(bool isDeleted)
        {
            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).
            ProjectTo<WorkingDayListDto>(_mapper.ConfigurationProvider).ToList();
            var data = result.Select(r =>
            {
                r.ProjectCode =  _context.Project.Where(x => x.Id == r.ParentProjectId).Select(s => s.ProjectCode).FirstOrDefault();
                return r;
            }).ToList();

            return result;
        }
        public void AddSiteType(WorkingDayDto workingDayListDto)
        {
            if (workingDayListDto.siteTypes != null && workingDayListDto.siteTypes[0].ProjectId !=0)
            {
                foreach (var item in workingDayListDto.siteTypes)
                 {
                     item.WorkingDayId = workingDayListDto.Id;
                    _context.SiteTypes.Add(item);
                    _context.Save();
                  }
            }
        }
    }
}
