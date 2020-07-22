using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Schedule;
using GSC.Data.Entities.Project.Schedule;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Schedule;
using GSC.Respository.ProjectRight;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Schedule
{
    [Route("api/[controller]")]
    public class ProjectScheduleController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
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

        public ProjectScheduleController(IProjectScheduleRepository projectScheduleRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IProjectScheduleTemplateRepository projectScheduleTemplateRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignRepository projectDesignRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectRightRepository projectRightRepository,
            INumberFormatRepository numberFormatRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository)
        {
            _projectScheduleRepository = projectScheduleRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectScheduleTemplateRepository = projectScheduleTemplateRepository;
            _projectDesignRepository = projectDesignRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _numberFormatRepository = numberFormatRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IList<ProjectScheduleDto> Get(bool isDeleted)
        //public IActionResult Get(bool isDeleted)
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
            //return Ok(projectSchedules);
            return projectSchedules;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectSchedule = _projectScheduleRepository.FindByInclude(t => t.Id == id, t => t.Templates,
                t => t.ProjectDesign, t => t.ProjectDesignPeriod, t => t.ProjectDesignVisit).FirstOrDefault();

            //if (projectSchedule.Templates != null)
            //    projectSchedule.Templates = projectSchedule.Templates.Where(x => x.DeletedDate == null).OrderByDescending(t => t.RefTimeInterval).ToList();

            var projectScheduleDto = _mapper.Map<ProjectScheduleDto>(projectSchedule);

            if (projectScheduleDto.Templates != null)
                projectScheduleDto.Templates.ForEach(t =>
                {
                    t.TemplateName = _projectDesignTemplateRepository.Find(t.ProjectDesignTemplateId).TemplateName;
                    t.Variables = _projectDesignVariableRepository.GetVariabeDropDown(t.ProjectDesignTemplateId);
                    t.PeriodName = _projectDesignPeriodRepository.Find(t.ProjectDesignPeriodId).DisplayName;
                    t.VisitName = _projectDesignVisitRepository.Find(t.ProjectDesignVisitId).DisplayName;
                    t.IsVariablLoaded = true;
                });
            projectScheduleDto.Templates = projectScheduleDto.Templates.Where(x => x.IsDeleted == false).ToList();
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
        public IActionResult Post([FromBody] ProjectScheduleDto projectScheduleDto)

        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var number = _projectScheduleRepository.All.Count(x => x.ProjectDesignId == projectScheduleDto.ProjectDesignId);
            projectScheduleDto.AutoNumber = _numberFormatRepository.GetNumberFormat("Schedule", number);
            var projectSchedule = _mapper.Map<ProjectSchedule>(projectScheduleDto);

            projectSchedule.ProjectId = _projectDesignRepository.Find(projectScheduleDto.ProjectDesignId).ProjectId;

            UpdateDesignTemplatesOrder(projectSchedule);

            _projectScheduleRepository.Add(projectSchedule);
            if (_uow.Save() <= 0) throw new Exception("Creating Project Schedule failed on save.");
            return Ok(projectSchedule.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectScheduleDto projectScheduleDto)
        {
            if (projectScheduleDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectSchedule = _mapper.Map<ProjectSchedule>(projectScheduleDto);
            UpdateTemplates(projectSchedule);
            UpdateDesignTemplatesOrder(projectSchedule);

            _projectScheduleRepository.Update(projectSchedule);

            if (_uow.Save() <= 0) throw new Exception("Updating Project Schedule failed on save.");
            return Ok(projectSchedule.Id);
        }

        private void UpdateTemplates(ProjectSchedule projectSchedule)
        {
            var deleteTemplates = _projectScheduleTemplateRepository.FindBy(x =>
                x.ProjectScheduleId == projectSchedule.Id
                && !projectSchedule.Templates.Any(c => c.Id == x.Id)).ToList();
            foreach (var template in deleteTemplates)
            {
                template.DeletedDate = DateTime.Now;
                _projectScheduleTemplateRepository.Update(template);
            }
        }

        private void UpdateDesignTemplatesOrder(ProjectSchedule projectSchedule)
        {
            var orderedList = _projectDesignTemplateRepository
                .FindBy(t => t.ProjectDesignVisitId == projectSchedule.ProjectDesignVisitId && t.DeletedDate == null)
                .OrderBy(t => t.DesignOrder).ToList();

            //var index = 0;
            //foreach (var item in projectSchedule.Templates.OrderByDescending(t => t.RefTimeInterval))
            //{
            //    var template = orderedList.First(t => t.Id == item.ProjectDesignTemplateId);
            //    orderedList.Remove(template);
            //    orderedList.Insert(index, template);
            //    index++;
            //}

            var i = 0;
            foreach (var item in orderedList)
            {
                item.DesignOrder = ++i;
                _projectDesignTemplateRepository.Update(item);
            }
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectScheduleRepository.FindByInclude(x => x.Id == id, x => x.ProjectDesign)
                .FirstOrDefault();

            if (record == null)
                return NotFound();

            if (!record.ProjectDesign.IsUnderTesting)
            {
                ModelState.AddModelError("Message", "Can not delete worklow!");
                return BadRequest(ModelState);
            }

            _projectScheduleRepository.Delete(record);
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
        //public IActionResult Get(bool isDeleted)
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
                    ProjectName = x.Project.ProjectName,
                    PeriodName = x.ProjectDesignPeriod.DisplayName,
                    VisitName = x.ProjectDesignVisit.DisplayName,
                    TemplateName = x.ProjectDesignTemplate.TemplateName,
                    VariableName = x.ProjectDesignVariable.VariableName,
                    IsDeleted = x.DeletedDate != null,
                    CreatedBy = x.CreatedBy,
                    ModifiedBy = x.ModifiedBy,
                    DeletedBy = x.DeletedBy,
                    CreatedDate = x.CreatedDate,
                    DeletedDate = x.DeletedDate,
                    ModifiedDate = x.ModifiedDate,
                    IsLock = !x.ProjectDesign.IsUnderTesting,
                }).OrderByDescending(x => x.Id).ToList();
            projectSchedules.ForEach(b =>
            {
                //  b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.CreatedBy != null)
                    b.CreatedByUser = _userRepository.Find((int)b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            //return Ok(projectSchedules);
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