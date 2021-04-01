using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Schedule;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Schedule
{
    [Route("api/[controller]")]
    public class ProjectScheduleController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IProjectScheduleRepository _projectScheduleRepository;
        private readonly IProjectScheduleTemplateRepository _projectScheduleTemplateRepository;
        private readonly INumberFormatRepository _numberFormatRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public ProjectScheduleController(IProjectScheduleRepository projectScheduleRepository,
            IProjectScheduleTemplateRepository projectScheduleTemplateRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignRepository projectDesignRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository,
            INumberFormatRepository numberFormatRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository, IGSCContext context)
        {
            _projectScheduleRepository = projectScheduleRepository;
            _projectScheduleTemplateRepository = projectScheduleTemplateRepository;
            _projectDesignRepository = projectDesignRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _numberFormatRepository = numberFormatRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IList<ProjectScheduleDto> Get(bool isDeleted)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectScheduleDto>();

            var projectSchedules = _projectScheduleRepository.FindByInclude(
                    x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                         && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
                         && projectList.Any(c => c == x.Project.Id),
                    x => x.Project, x => x.ProjectDesignPeriod, x => x.ProjectDesignVisit, x => x.ProjectDesignTemplate,
                    x => x.ProjectDesignVariable)
                .Select(x => new ProjectScheduleDto
                {
                    Id = x.Id,
                    ProjectName = x.Project.ProjectName,
                    PeriodName = x.ProjectDesignPeriod.DisplayName,
                    VisitName = x.ProjectDesignVisit.DisplayName,
                    TemplateName = x.ProjectDesignTemplate.TemplateName,
                    VariableName = x.ProjectDesignVariable.VariableName,
                    IsDeleted = x.DeletedDate != null,
                }).OrderByDescending(x => x.Id).ToList();
            
            return projectSchedules;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectSchedule = _projectScheduleRepository.FindByInclude(t => t.Id == id, t => t.Templates,
                t => t.ProjectDesign, t => t.ProjectDesignPeriod, t => t.ProjectDesignVisit).FirstOrDefault();


            var projectScheduleDto = _mapper.Map<ProjectScheduleDto>(projectSchedule);

            if (projectScheduleDto.Templates != null)
                projectScheduleDto.Templates.ToList().ForEach(t =>
                {
                    var Template = _projectDesignTemplateRepository.Find(t.ProjectDesignTemplateId);
                    t.TemplateName = Template.TemplateName;
                    t.TemplateDesignOrder = Template.DesignOrder;
                    t.Variables = _projectDesignVariableRepository.GetVariabeDropDown(t.ProjectDesignTemplateId);
                    t.PeriodName = _projectDesignPeriodRepository.Find(t.ProjectDesignPeriodId).DisplayName;
                    t.VisitName = _projectDesignVisitRepository.Find(t.ProjectDesignVisitId).DisplayName;
                    t.IsVariablLoaded = true;
                });
            projectScheduleDto.Templates = projectScheduleDto.Templates.Where(x => (x.IsDeleted ? x.DeletedDate != null : x.DeletedDate == null)).OrderBy(x => x.ProjectDesignVisitId).ThenBy(x => x.TemplateDesignOrder).ToList();
            projectScheduleDto.IsLock = !projectSchedule.ProjectDesign.IsUnderTesting;
            return Ok(projectScheduleDto);
        }

        [HttpGet("CheckProjectSchedule/{projectDesignVariableId}")]
        public IActionResult CheckProjectSchedule(int projectDesignVariableId)
        {
            if (projectDesignVariableId <= 0) return BadRequest();
            var projectSchedule = _projectScheduleRepository
                .FindBy(t => t.ProjectDesignVariableId == projectDesignVariableId && t.DeletedDate == null)
                .FirstOrDefault();

            if (projectSchedule == null)
                return Ok(0);
            return Ok(projectSchedule.Id);
        }

        [HttpGet("checkProjectScheduleLocked/{projectDesignId}")]
        public IActionResult checkProjectScheduleLocked(int projectDesignId)
        {
            if (projectDesignId <= 0) return BadRequest();
            var projectDesign = _projectDesignRepository
                .FindBy(t => t.Id == projectDesignId && t.DeletedDate == null).FirstOrDefault();

            var projectDesignDto = _mapper.Map<ProjectDesignDto>(projectDesign);
            if (projectDesign != null) projectDesignDto.Locked = !projectDesign.IsUnderTesting;

            return Ok(projectDesignDto);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ProjectScheduleDto projectScheduleDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var number = _projectScheduleRepository.All.Count(x => x.ProjectDesignId == projectScheduleDto.ProjectDesignId);
            projectScheduleDto.AutoNumber = _numberFormatRepository.GetNumberFormat("Schedule", number);
            var projectSchedule = _mapper.Map<ProjectSchedule>(projectScheduleDto);

            projectSchedule.ProjectId = _projectDesignRepository.Find(projectScheduleDto.ProjectDesignId).ProjectId;

            _projectScheduleTemplateRepository.UpdateDesignTemplatesOrder(projectSchedule);

            _projectScheduleRepository.Add(projectSchedule);
            foreach (var item in projectSchedule.Templates)
            {
                _projectScheduleTemplateRepository.Add(item);
            }

            _uow.Save();

            _projectScheduleTemplateRepository.UpdateDesignTemplatesSchedule(projectScheduleDto.ProjectDesignPeriodId);
            _uow.Save();

            return Ok(projectSchedule.Id);
        }

        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] ProjectScheduleDto projectScheduleDto)
        {
            if (projectScheduleDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectSchedule = _mapper.Map<ProjectSchedule>(projectScheduleDto);
            _projectScheduleTemplateRepository.UpdateTemplates(projectSchedule);
            _projectScheduleTemplateRepository.UpdateDesignTemplatesOrder(projectSchedule);

            _projectScheduleRepository.Update(projectSchedule);
            foreach (var item in projectSchedule.Templates)
            {
                if (item.Id > 0)
                    _projectScheduleTemplateRepository.Update(item);
                else
                    _projectScheduleTemplateRepository.Add(item);
            }
            _uow.Save();

            _projectScheduleTemplateRepository.UpdateDesignTemplatesSchedule(projectScheduleDto.ProjectDesignPeriodId);
            _uow.Save();

            return Ok(projectSchedule.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectScheduleRepository.FindByInclude(x => x.Id == id, x => x.ProjectDesign)
                .FirstOrDefault();

            var recordTemplate = _projectScheduleTemplateRepository.FindByInclude(x => x.ProjectScheduleId == id && x.DeletedDate == null).ToList();

            if (record == null && recordTemplate == null)
                return NotFound();

            if (!record.ProjectDesign.IsUnderTesting)
            {
                ModelState.AddModelError("Message", "Can not delete schedule!");
                return BadRequest(ModelState);
            }

            _projectScheduleRepository.Delete(record);
            recordTemplate.ForEach(x =>
            {
                _projectScheduleTemplateRepository.Delete(x);
            });

            _uow.Save();

            _projectScheduleTemplateRepository.UpdateDesignTemplatesSchedule(record.ProjectDesignPeriodId);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _projectScheduleRepository.Find(id);

            if (record == null)
                return NotFound();
            _projectScheduleRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetData/{id}")]
        public IList<ProjectScheduleDto> GetData(int id)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            if (projectList == null || projectList.Count == 0) return new List<ProjectScheduleDto>();

            var projectSchedules = _projectScheduleRepository.FindByInclude(
                    x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                         && x.DeletedDate == null
                         && x.ProjectDesignId == id
                         && projectList.Any(c => c == x.Project.Id),
                    x => x.Project, t => t.ProjectDesign, x => x.ProjectDesignPeriod, x => x.ProjectDesignVisit, x => x.ProjectDesignTemplate,
                    x => x.ProjectDesignVariable)
                .Select(x => new ProjectScheduleDto
                {
                    Id = x.Id,
                    AutoNumber = x.AutoNumber,
                    ProjectId = x.ProjectId,
                    ProjectName = x.Project.ProjectCode,
                    PeriodName = x.ProjectDesignPeriod.DisplayName,
                    VisitName = x.ProjectDesignVisit.DisplayName,
                    TemplateName = x.ProjectDesignTemplate.TemplateName,
                    VariableName = x.ProjectDesignVariable.VariableName,
                    IsDeleted = x.DeletedDate != null,
                    IsLock = !x.ProjectDesign.IsUnderTesting,
                    CreatedByUser = x.CreatedBy == null ? null : _context.Users.Where(y => y.Id == x.CreatedBy).FirstOrDefault().UserName,
                    CreatedDate = x.CreatedDate,
                    ModifiedByUser = x.ModifiedBy == null ? null : _context.Users.Where(y => y.Id == x.ModifiedBy).FirstOrDefault().UserName,
                    ModifiedDate = x.ModifiedDate
                }).OrderByDescending(x => x.Id).ToList();
            
            return projectSchedules;
        }

        [HttpGet("GetDataByPeriod/{periodId}/{projectId}")]
        public IList<ProjectScheduleTemplateDto> GetDataByPeriod(long periodId, long projectId)
        {
            var data = _projectScheduleRepository.GetDataByPeriod(periodId, projectId);
            return data;
        }

        [HttpGet("getRefVariableValuefromTargetVariable/{projectDesignVariableId}")]
        public IActionResult GetRefVariableValuefromTargetVariable(int projectDesignVariableId)
        {
            if (projectDesignVariableId <= 0) return BadRequest();
            //var projectSchedule = _projectScheduleTemplateRepository.FindByInclude(t => t.ProjectDesignVariableId == projectDesignVariableId).FirstOrDefault();

            var data = _projectScheduleRepository.GetRefVariableValuefromTargetVariable(projectDesignVariableId);
            return Ok(data);
        }
    }
}