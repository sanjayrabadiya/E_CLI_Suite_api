using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVariableRelationRepository : GenericRespository<ProjectDesignVariableRelation>, IProjectDesignVariableRelationRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectDesignVariableRelationRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public ProjectDesignVariableDisplayRelationDto GetProjectDesignVariableRelationById(int Id)
        {
            //return All.Where(x => x.ProjectDesignVariableId == Id).Select(t => new ProjectDesignVariableRelationDto
            //{
            //    Id = t.Id,
            //    ProjectDesignVariable = t.ProjectDesignVariable,
            //    ProjectDesignTemplateId = t.ProjectDesignVariable.ProjectDesignTemplateId,
            //    ProjectDesignVisitId = t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisitId,
            //    ProjectDesignRelationVariableId = t.ProjectDesignRelationVariableId,
            //    ProjectDesignSuggestionVariableId = t.ProjectDesignSuggestionVariableId
            //}).FirstOrDefault();

            return All.Where(x => x.ProjectDesignVariableId == Id && x.DeletedDate == null).Select(t => new ProjectDesignVariableDisplayRelationDto
            {
                Id = t.Id,
                ProjectDesignVariableName = t.ProjectDesignVariable.VariableName,
                ProjectDesignTemplateName = t.ProjectDesignVariable.ProjectDesignTemplate.TemplateName,
                ProjectDesignVisitName = t.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.DisplayName,
                ProjectDesignRelationVariableName = _context.ProjectDesignVariable.Where(r => r.Id == t.ProjectDesignRelationVariableId).FirstOrDefault().VariableName,
                // ProjectDesignSuggestionVariableId = t.ProjectDesignSuggestionVariableId
            }).FirstOrDefault();
        }


    }
}
