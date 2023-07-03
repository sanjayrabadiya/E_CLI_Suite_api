using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Respository.Project.Workflow;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class WorkflowTemplateRepository : GenericRespository<WorkflowTemplate>, IWorkflowTemplateRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public WorkflowTemplateRepository(IGSCContext context,
             IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser) :
            base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
        }

        public List<int> GetDetailById(WorkflowTemplateDto workflowTemplateDto)
        {
            var result = All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == workflowTemplateDto.ProjectDesignTemplateId).Select(x => x.LevelNo).ToList();
            return result;
        }

        public void updatePermission(WorkflowTemplateDto workflowTemplateDto)
        {
            var workflowTemplate = All.Where(r => r.DeletedDate == null
           && r.ProjectDesignTemplateId == workflowTemplateDto.ProjectDesignTemplateId).ToList();

            //add new
            var firstNotSecond = workflowTemplateDto.LevelNos.Except(workflowTemplate.Select(x => x.LevelNo)).ToList();
            // no change
            var secondNotFirst = workflowTemplateDto.LevelNos.Except(firstNotSecond).ToList();
            // delete
            var thirdNotFirst = workflowTemplate.ToList().Select(x => x.LevelNo).Except(workflowTemplateDto.LevelNos).ToList();

            foreach (var item in firstNotSecond)
            {
                var result = _mapper.Map<WorkflowTemplate>(workflowTemplateDto);
                result.LevelNo = item;
                Add(result);
            }

            foreach (var item in thirdNotFirst)
            {
                var d = workflowTemplate.Where(x => x.LevelNo == item).FirstOrDefault();
                Delete(d);
            }
            _context.Save();
        }

        public bool CheckforTemplateisExists(int projectDesignVisitId)
        {
            var result = All.Any(x => x.DeletedDate == null && x.ProjectDesignTemplate.ProjectDesignVisitId == projectDesignVisitId);
            return result;
        }
    }
}
