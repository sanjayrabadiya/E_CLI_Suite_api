using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Domain.Context;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.GeneralConfig
{
    public class ProjectSettingsRepository : GenericRespository<ProjectSettings>, IProjectSettingsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IProjectRightRepository _projectRightRepository;
        public ProjectSettingsRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            IProjectRightRepository projectRightRepository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _projectRightRepository = projectRightRepository;
        }

        public List<ProjectDropDown> GetParentProjectDropDownEicf()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var ProjectList = All.Include(c => c.Project).Where(x =>
                      (x.Project.CompanyId == null || x.Project.CompanyId == _jwtTokenAccesser.CompanyId)
                      && x.Project.ParentProjectId == null
                      && projectList.Any(c => c == x.ProjectId) && x.IsEicf == true && x.DeletedDate == null)
                  .Select(c => new ProjectDropDown
                  {
                      Id = c.ProjectId,
                      Value = c.Project.ProjectCode,
                      Code = c.Project.ProjectCode,
                      IsStatic = c.Project.IsStatic,
                      IsDeleted = c.DeletedDate != null
                  }).Distinct().OrderBy(o => o.Value).ToList();

            return ProjectList;
        }

        public List<ProjectDropDown> GetParentProjectDropDownScreening()
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return null;

            var ProjectList = All.Include(c => c.Project).Where(x =>
                      (x.Project.CompanyId == null || x.Project.CompanyId == _jwtTokenAccesser.CompanyId)
                      && x.Project.ParentProjectId == null
                      && projectList.Any(c => c == x.ProjectId) && x.IsScreening == true && x.DeletedDate == null)
                  .Select(c => new ProjectDropDown
                  {
                      Id = c.ProjectId,
                      Value = c.Project.ProjectCode + " - " + c.Project.ProjectName,
                      Code = c.Project.ProjectCode,
                      IsStatic = c.Project.IsStatic,
                      IsDeleted = c.DeletedDate != null,
                      ParentProjectId = c.Project.ParentProjectId ?? c.ProjectId,
                      AttendanceLimit = c.Project.AttendanceLimit ?? 0
                  }).Distinct().OrderBy(o => o.Value).ToList();

            return ProjectList;
        }
    }
}
