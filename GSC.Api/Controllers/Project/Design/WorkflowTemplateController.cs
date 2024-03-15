using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
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
using System.Linq;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkflowTemplateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IWorkflowTemplateRepository _workflowTemplateRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public WorkflowTemplateController(IWorkflowTemplateRepository workflowTemplateRepository,
            IUnitOfWork uow, IMapper mapper,
            IGSCContext context)
        {
            _workflowTemplateRepository = workflowTemplateRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;

        }

        [HttpPost]
        [Route("GetDatails")]
        public IActionResult GetDatails([FromBody] WorkflowTemplateDto workflowTemplateDto)
        {
            var result = _workflowTemplateRepository.All.Any(x => x.ProjectDesignTemplateId == workflowTemplateDto.ProjectDesignTemplateId);
            return Ok(result);
        }


        [HttpPost]
        [Route("GetData")]
        public IActionResult GetData([FromBody] WorkflowTemplateDto workflowTemplateDto)
        {
            return Ok(_workflowTemplateRepository.GetDetailById(workflowTemplateDto));
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] WorkflowTemplateDto workflowTemplateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var Workflowtemplate = _mapper.Map<WorkflowTemplate>(workflowTemplateDto);

            foreach (var item in workflowTemplateDto.LevelNos)
            {
                var result = Workflowtemplate.DeepCopy();
                result.LevelNo = item;
                _workflowTemplateRepository.Add(result);
            }
            _uow.Save();
            return Ok();
        }

        [TransactionRequired]
        [HttpPut]
        public IActionResult Put([FromBody] WorkflowTemplateDto workflowTemplateDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            _workflowTemplateRepository.updatePermission(workflowTemplateDto);
            return Ok();
        }


        [HttpGet]
        [Route("CheckForVisitWorkflow/{projectDesignVisitId}")]
        public IActionResult CheckForVisitWorkflow(int projectDesignVisitId)
        {
            var result = _context.WorkflowVisit.Any(x => x.DeletedDate == null && x.ProjectDesignVisitId == projectDesignVisitId);
            return Ok(result);
        }
    }
}
