using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
   public class SyncConfigurationRepository : GenericRespository<SyncConfiguration>, ISyncConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectRightRepository _projectRightRepository;
        public SyncConfigurationRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper, IProjectRightRepository projectRightRepository)
           : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _projectRightRepository = projectRightRepository;
        }

        public List<SyncConfigurationGridDto> GetsyncConfigurationList(bool isDeleted,int ProjectId)
        {
            var result = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId==ProjectId).OrderByDescending(x => x.Id).
                     ProjectTo<SyncConfigurationGridDto>(_mapper.ConfigurationProvider).ToList();
            return result;
        }

        public List<DropDownEnum> GetProjectWorkPlaceDetails(int ProjectId,short WorkPlaceFolderId)
        {   
            var ProjectWorkplaceId = _context.ProjectWorkplace.Where(x => x.ProjectId == ProjectId).FirstOrDefault().Id;
            var details = _context.ProjectWorkplaceDetail.Where(x => x.ProjectWorkplaceId == ProjectWorkplaceId && x.WorkPlaceFolderId== WorkPlaceFolderId && x.DeletedDate==null).Select(x => new DropDownEnum {
                Id = Convert.ToInt16(x.Id),
                Value = x.ItemName
            }).ToList();
            return details;
        }

        public List<ProjectDropDown> GetProjectDropDownEtmf()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var parentProjectList = _context.ProjectWorkplace.Where(x => x.DeletedDate == null && projectList.Any(c => c == x.ProjectId)).Select(x =>
                  new ProjectDropDown
                  {
                      Id = x.Project.Id,
                      Value = x.Project.ProjectCode,
                      Code = x.Project.ProjectCode,
                      IsStatic = x.Project.IsStatic,
                      ParentProjectId = x.Project.ParentProjectId ?? x.Id,
                      IsDeleted = x.Project.DeletedDate != null
                  }).ToList();
            return parentProjectList;
        }

    }   
}
