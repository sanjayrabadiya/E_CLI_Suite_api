using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Master
{
    public class ProjectSiteAddressRepository : GenericRespository<ProjectSiteAddress>, IProjectSiteAddressRepository
    {
        private readonly IMapper _mapper;
        public ProjectSiteAddressRepository(IGSCContext context, IMapper mapper) : base(context)
        {
            _mapper = mapper;
        }

        public List<ProjectSiteAddressGridDto> GetProjectSiteAddressList(bool isDelete)
        {
            return All.Where(x => isDelete ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ProjectSiteAddressGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<ProjectSiteAddressGridDto> GetProjectSiteAddressByProject(bool isDeleted, int projectId, int manageSiteId)
        {
            var data = All.Where(q => (isDeleted ? q.DeletedDate != null : q.DeletedDate == null) && q.ProjectId == projectId && q.ManageSiteId == manageSiteId)
                 .ProjectTo<ProjectSiteAddressGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return data;
        }

        public string Duplicate(ProjectSiteAddress projectSite)
        {
            if (All.Any(x => x.Id != projectSite.Id && x.ManageSiteAddressId == projectSite.ManageSiteAddressId && x.ProjectId == projectSite.ProjectId && x.DeletedDate == null))
                return "Duplicate Project Site Address";
            return "";
        }
    }
}
