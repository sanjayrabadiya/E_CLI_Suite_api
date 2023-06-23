using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Presentation;
using DocumentFormat.OpenXml.Wordprocessing;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using System.Text;

namespace GSC.Respository.Project.Workflow
{
    public class WorkflowVisitRepository : GenericRespository<WorkflowVisit>, IWorkflowVisitRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public WorkflowVisitRepository(IGSCContext context,
             IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser) :
            base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
        }

        public List<int> GetDetailById(WorkflowVisitDto workflowVisitDto)
        {
            var result = All.Where(x => x.IsIndependent == workflowVisitDto.IsIndependent && x.ProjectWorkflowLevelId == workflowVisitDto.ProjectWorkflowLevelId
           && x.ProjectWorkflowIndependentId == workflowVisitDto.ProjectWorkflowIndependentId && x.DeletedDate == null).Select(x => x.ProjectDesignVisitId).ToList();

            return result;

        }

        public void updatePermission(WorkflowVisitDto workflowVisitDto)
        {
            var workflowvisit = All.Where(r =>
             r.IsIndependent == workflowVisitDto.IsIndependent
             && r.DeletedDate == null
           && r.ProjectWorkflowIndependentId == workflowVisitDto.ProjectWorkflowIndependentId
           && r.ProjectWorkflowLevelId == workflowVisitDto.ProjectWorkflowLevelId).ToList();

            //add new
            var firstNotSecond = workflowVisitDto.ProjectDesignVisitIds.Except(workflowvisit.Select(x => x.ProjectDesignVisitId)).ToList();
            // no change
            var secondNotFirst = workflowVisitDto.ProjectDesignVisitIds.Except(firstNotSecond).ToList();
            // delete
            var thirdNotFirst = workflowvisit.ToList().Select(x => x.ProjectDesignVisitId).Except(workflowVisitDto.ProjectDesignVisitIds).ToList();

            
            foreach (var item in firstNotSecond)
            {
                var result = _mapper.Map<WorkflowVisit>(workflowVisitDto);
                result.ProjectDesignVisitId = item;
                Add(result);
            }

            foreach (var item in thirdNotFirst)
            {
                var d= workflowvisit.Where(x=>x.ProjectDesignVisitId == item).FirstOrDefault();
                Delete(d);
            }

            _context.Save();

        }
    }
}
