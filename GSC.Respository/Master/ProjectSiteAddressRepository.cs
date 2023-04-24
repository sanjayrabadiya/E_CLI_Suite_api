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
        private readonly IProjectRepository _projectRepository;
        private readonly IManageSiteAddressRepository _manageSiteAddressRepository;
        private readonly IManageSiteRepository _manageSiteRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        public ProjectSiteAddressRepository(IGSCContext context, IProjectRepository projectRepository, IManageSiteAddressRepository manageSiteAddressRepository, IManageSiteRepository manageSiteRepository, IMapper mapper) : base(context)
        {
            _projectRepository = projectRepository;
            _manageSiteAddressRepository = manageSiteAddressRepository;
            _manageSiteRepository = manageSiteRepository;
            _mapper = mapper;
            _context = context;
        }

        public List<ProjectSiteAddressGridDto> GetProjectSiteAddressList(bool isDeleted)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                   ProjectTo<ProjectSiteAddressGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public List<ProjectSiteAddressGridDto> GetProjectSiteAddressByProject(bool isDeleted, int projectId, int manageSiteId)
        {
            var data = All.Where(q => (isDeleted ? q.DeletedDate != null : q.DeletedDate == null) && q.ProjectId == projectId && q.ManageSiteId == manageSiteId)
                 .ProjectTo<ProjectSiteAddressGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return data;
        }

        public string Duplicate(ProjectSiteAddress objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ManageSiteAddressId == objSave.ManageSiteAddressId && x.ProjectId == objSave.ProjectId && x.DeletedDate == null))
                return "Duplicate Project Site Address";
            return "";
        }
    }
}
