using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVisitStatusRepository : GenericRespository<ProjectDesignVisitStatus>, IProjectDesignVisitStatusRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        public ProjectDesignVisitStatusRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }


        public ProjectDesignVisitStatusDto GetProjectDesignVisitStatusById(int Id)
        {
            return All.Where(x => x.Id == Id).Select(t => new ProjectDesignVisitStatusDto
            {
                Id = t.Id,
                ProjectDesignVisitId = t.ProjectDesignVisitId,
                ProjectDesignTemplateId = t.ProjectDesignVariable.ProjectDesignTemplateId,
                ProjectDesignVariableId = t.ProjectDesignVariableId,
                VisitStatusId = t.VisitStatusId
            }).FirstOrDefault();
        }

        public ProjectDesignVisitStatusDto GetProjectDesignVariableDetail(int visitId, ScreeningVisitStatus screeningVisitStatus)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == visitId
            && x.VisitStatusId == screeningVisitStatus).Select(t => new ProjectDesignVisitStatusDto
            {
                Id = t.Id,
                ProjectDesignVisitId = t.ProjectDesignVisitId,
                ProjectDesignTemplateId = t.ProjectDesignVariable.ProjectDesignTemplateId,
                ProjectDesignVariableId = t.ProjectDesignVariableId,
                VisitStatusId = t.VisitStatusId
            }).FirstOrDefault();
        }

        public List<ProjectDesignVisitStatusGridDto> GetVisits(int VisitId)
        {
            return All.Where(x => x.DeletedDate == null && x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisitId == VisitId).
                   ProjectTo<ProjectDesignVisitStatusGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(ProjectDesignVisitStatusDto objSave)
        {
            if (All.Any(x => x.ProjectDesignVisitId == objSave.ProjectDesignVisitId && x.ProjectDesignVariable.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId &&
            x.DeletedDate == null))
                return "Template already use.";

            if (All.Any(x => x.ProjectDesignVisitId == objSave.ProjectDesignVisitId && x.VisitStatusId == objSave.VisitStatusId &&
            x.DeletedDate == null))
                return "Status already use.";
            return "";
        }

    }
}