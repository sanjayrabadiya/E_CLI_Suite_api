using AutoMapper;
using DocumentFormat.OpenXml.Office2013.Drawing.ChartStyle;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.Workflow;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Api.Controllers.Project.Workflow
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowVisitController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IWorkflowVisitRepository _workflowVisitRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;

        public WorkflowVisitController(IWorkflowVisitRepository workflowVisitRepository,
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context,
        IJwtTokenAccesser jwtTokenAccesser)
        {
            _workflowVisitRepository = workflowVisitRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;

        }

        [HttpPost]
        [Route("GetDatails")]
        public IActionResult GetDatails([FromBody] WorkflowVisitDto workflowVisitDto)
        {
            var result = _workflowVisitRepository.All.Any(x => x.IsIndependent == workflowVisitDto.IsIndependent && x.ProjectWorkflowLevelId == workflowVisitDto.ProjectWorkflowLevelId
           && x.ProjectWorkflowIndependentId == workflowVisitDto.ProjectWorkflowIndependentId);
            return Ok(result);
        }


        [HttpPost]
        [Route("GetData")]
        public IActionResult GetData([FromBody] WorkflowVisitDto workflowVisitDto)
        {
            return Ok(_workflowVisitRepository.GetDetailById(workflowVisitDto));
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] WorkflowVisitDto workflowVisitDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var workflowVisit = _mapper.Map<WorkflowVisit>(workflowVisitDto);

            foreach (var item in workflowVisitDto.ProjectDesignVisitIds)
            {
                var result = workflowVisit.DeepCopy();
                result.ProjectDesignVisitId = item;
                _workflowVisitRepository.Add(result);
            }
            _uow.Save();
            return Ok();
        }

        [TransactionRequired]
        [HttpPut]
        public IActionResult Put([FromBody] WorkflowVisitDto workflowVisitDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _workflowVisitRepository.updatePermission(workflowVisitDto);

            return Ok();
        }
    }
}
