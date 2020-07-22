using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignTemplateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IVariableTemplateRepository _variableTemplateRepository;

        public ProjectDesignTemplateController(IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IVariableTemplateRepository variableTemplateRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper)
        {
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{projectDesignVisitId}")]
        public IActionResult Get(int projectDesignVisitId)
        {
            if (projectDesignVisitId <= 0) return BadRequest();

            var templates = _projectDesignTemplateRepository.FindByInclude(t =>
                t.ProjectDesignVisitId == projectDesignVisitId
                && t.DeletedDate == null, t => t.Domain).OrderBy(t => t.DesignOrder).ToList();

            var templatesDto = _mapper.Map<IEnumerable<ProjectDesignTemplateDto>>(templates);
            templatesDto.ForEach(t =>
            {
                t.DomainId = t.DomainId;
                t.DomainName = _uow.Context.Domain.Find(t.DomainId)?.DomainName;
                //if (sheduleTemplate != null)
                //    t.RefTimeInterval = sheduleTemplate.RefTimeInterval;
            });

            return Ok(templatesDto);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignTemplateDto projectDesignTemplateDto)
        {
            if (projectDesignTemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectDesignTemplate = _mapper.Map<ProjectDesignTemplate>(projectDesignTemplateDto);

            _projectDesignTemplateRepository.Update(projectDesignTemplate);

            if (_uow.Save() <= 0) throw new Exception("Updating Project Design Template failed on save.");
            return Ok(projectDesignTemplate.Id);
        }

        [HttpGet("AddTemplate/{projectDesignVisitId}/{variableTemplateId}/{noOfTemplates}")]
        public IActionResult AddTemplate(int projectDesignVisitId, int variableTemplateId, int noOfTemplates)
        {
            if (projectDesignVisitId <= 0 || variableTemplateId <= 0 || noOfTemplates <= 0) return BadRequest();

            if (_projectDesignVisitRepository.Find(projectDesignVisitId) == null) return NotFound();

            var variableTemplate = _variableTemplateRepository.GetTemplate(variableTemplateId);

            if (variableTemplate == null) return NotFound();

            var designOrder = 0;
            if (_uow.Context.ProjectDesignTemplate.Any(t => t.ProjectDesignVisitId == projectDesignVisitId))
                designOrder = _uow.Context.ProjectDesignTemplate
                    .Where(t => t.ProjectDesignVisitId == projectDesignVisitId).Max(t => t.DesignOrder);

            for (var i = 0; i < noOfTemplates; i++)
            {
                var projectDesignTemplate = _mapper.Map<ProjectDesignTemplate>(variableTemplate);
                projectDesignTemplate.Id = 0;
                projectDesignTemplate.ProjectDesignVisitId = projectDesignVisitId;
                projectDesignTemplate.VariableTemplateId = variableTemplateId;
                projectDesignTemplate.DesignOrder = ++designOrder;
                projectDesignTemplate.Variables = new List<ProjectDesignVariable>();

                var variableOrder = 0;
                foreach (var variableDetail in variableTemplate.VariableTemplateDetails)
                {
                    var projectDesignVariable = _mapper.Map<ProjectDesignVariable>(variableDetail.Variable);
                    projectDesignVariable.Id = 0;
                    projectDesignVariable.VariableId = variableDetail.VariableId;
                    projectDesignVariable.DesignOrder = ++variableOrder;

                    projectDesignTemplate.Variables.Add(projectDesignVariable);

                    projectDesignVariable.Values = new List<ProjectDesignVariableValue>();

                    var valueOrder = 0;
                    foreach (var variableValue in variableDetail.Variable.Values)
                    {
                        var projectDesignVariableValue = _mapper.Map<ProjectDesignVariableValue>(variableValue);
                        projectDesignVariableValue.Id = 0;
                        projectDesignVariableValue.SeqNo = ++valueOrder;

                        projectDesignVariable.Values.Add(projectDesignVariableValue);
                    }
                }

                _projectDesignTemplateRepository.Add(projectDesignTemplate);
            }

            if (_uow.Save() <= 0) throw new Exception("Creating Project Design Template failed on save.");

            return Ok();
        }

        [HttpGet("CloneTemplate/{projectDesignTempateId}/{noOfClones}")]
        public IActionResult CloneTemplate(int projectDesignTempateId, int noOfClones)
        {
            if (projectDesignTempateId <= 0 || noOfClones <= 0) return BadRequest();

            var projectDesignVisitId =
                _projectDesignTemplateRepository.Find(projectDesignTempateId).ProjectDesignVisitId;

            var designOrder = 0;
            if (_uow.Context.ProjectDesignTemplate.Any(t => t.ProjectDesignVisitId == projectDesignVisitId))
                designOrder = _uow.Context.ProjectDesignTemplate
                    .Where(t => t.ProjectDesignVisitId == projectDesignVisitId).Max(t => t.DesignOrder);

            for (var i = 0; i < noOfClones; i++)
            {
                var temp = _projectDesignTemplateRepository.GetTemplate(projectDesignTempateId);

                var projectDesignTemplate = _mapper.Map<ProjectDesignTemplate>(temp);
                projectDesignTemplate.Id = 0;
                projectDesignTemplate.ParentId = projectDesignTempateId;
                projectDesignTemplate.DesignOrder = ++designOrder;
                projectDesignTemplate.VariableTemplate = null;
                projectDesignTemplate.Domain = null;
                projectDesignTemplate.TemplateName = projectDesignTemplate.TemplateName + "_" + designOrder;
                foreach (var variable in projectDesignTemplate.Variables)
                {
                    variable.Id = 0;
                    variable.Unit = null;
                    variable.VariableCategory = null;
                    variable.ProjectDesignTemplate = null;
                    foreach (var variableValue in variable.Values) variableValue.Id = 0;
                }

                projectDesignTemplate.ProjectDesignVisit = null;

                _projectDesignTemplateRepository.Add(projectDesignTemplate);
            }

            if (_uow.Save() <= 0) throw new Exception("Creating Project Design Template failed on save.");

            return Ok();
        }

        [HttpPost("ModifyClonnedTemplates")]
        public IActionResult ModifyClonnedTemplates([FromBody] ProjectDesignTemplateDto projectDesignTemplateDto)
        {
            if (projectDesignTemplateDto.Id <= 0 || projectDesignTemplateDto.ClonnedTemplateIds == null ||
                projectDesignTemplateDto.ClonnedTemplateIds.Count == 0) return BadRequest();

            projectDesignTemplateDto.ClonnedTemplateIds.ForEach(t =>
            {
                var parent = _projectDesignTemplateRepository.GetTemplate(projectDesignTemplateDto.Id);

                var clonnedTemplate = _projectDesignTemplateRepository.GetTemplate(t);
                foreach (var variable in clonnedTemplate.Variables)
                    variable.DeletedDate = DateTime.Now.UtcDate();

                var variables = parent.Variables.ToList();
                foreach (var variable in variables)
                {
                    variable.Id = 0;
                    foreach (var variableValue in variable.Values) variableValue.Id = 0;
                    clonnedTemplate.Variables.Add(variable);
                }

                _projectDesignTemplateRepository.Update(clonnedTemplate);
            });

            if (_uow.Save() <= 0) throw new Exception("Modify Clonned Templates failed on save.");

            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectDesignTemplateRepository.Delete(record);
            _uow.Save();

            if (_projectDesignTemplateRepository.FindBy(t =>
                t.ProjectDesignVisitId == record.ProjectDesignVisitId && t.DeletedDate == null).Any())
            {
                var minOrder = _projectDesignTemplateRepository
                    .FindBy(t => t.ProjectDesignVisitId == record.ProjectDesignVisitId && t.DeletedDate == null)
                    .Min(t => t.DesignOrder);
                var firstId = _projectDesignTemplateRepository.FindBy(t =>
                    t.ProjectDesignVisitId == record.ProjectDesignVisitId && t.DeletedDate == null &&
                    t.DesignOrder == minOrder).First().Id;
                ChangeTemplateDesignOrder(firstId, 0);
            }

            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateDropDown/{projectDesignVisitId}")]
        public IActionResult GetTemplateDropDown(int projectDesignVisitId)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDown(projectDesignVisitId));
        }

        [HttpGet]
        [Route("GetTemplateByLockedDropDown")]
        public IActionResult GetTemplateByLockedDropDown([FromQuery]LockUnlockDDDto lockUnlockDDDto)
        {            
            return Ok(_projectDesignTemplateRepository.GetTemplateByLockedDropDown(lockUnlockDDDto));
        }

        [HttpGet]
        [Route("GetTemplateDropDownForProjectSchedule/{projectDesignVisitId}/{collectionSource}/{refVariable}")]
        public IActionResult GetTemplateDropDownForProjectSchedule(int projectDesignVisitId, int? collectionSource, int? refVariable)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDownForProjectSchedule(projectDesignVisitId, collectionSource, refVariable));
        }

        [HttpGet]
        [Route("GetClonnedTemplates/{id}")]
        public IActionResult GetClonnedTemplates(int id)
        {
            return Ok(_projectDesignTemplateRepository.GetClonnedTemplates(id));
        }

        [HttpGet]
        [Route("ChangeTemplateDesignOrder/{id}/{index}")]
        public IActionResult ChangeTemplateDesignOrder(int id, int index)
        {
            var template = _projectDesignTemplateRepository.Find(id);
            var visitId = template.ProjectDesignVisitId;

            var orderedList = _projectDesignTemplateRepository
                .FindBy(t => t.ProjectDesignVisitId == visitId && t.DeletedDate == null).OrderBy(t => t.DesignOrder)
                .ToList();
            orderedList.Remove(orderedList.First(t => t.Id == id));
            orderedList.Insert(index, template);

            var i = 0;
            foreach (var item in orderedList)
            {
                item.DesignOrder = ++i;
                _projectDesignTemplateRepository.Update(item);
            }

            _uow.Save();

            return Ok();
        }

        [HttpGet("GetProjectScheduleTemplateId/{id}")]
        public IActionResult GetProjectScheduleTemplateId(int id)
        {
            if (id <= 0) return BadRequest();
            var projectScheduleTemplate =
                _uow.Context.ProjectScheduleTemplate.FirstOrDefault(t => t.ProjectDesignTemplateId == id);

            if (projectScheduleTemplate == null)
                return Ok(id);
            return Ok(_uow.Context.ProjectSchedule.Find(projectScheduleTemplate.ProjectScheduleId)
                .ProjectDesignTemplateId);
        }

        [HttpGet("GetTemplate/{id}")]
        public IActionResult GetTemplate(int id)
        {
            if (id <= 0) return BadRequest();
            var designTemplate = _projectDesignTemplateRepository.GetTemplate(id);

            var designTemplateDto = _mapper.Map<ProjectDesignTemplateDto>(designTemplate);

            return Ok(designTemplateDto);
        }

        [HttpGet]
        [Route("GetTemplateDropDownByPeriodId/{projectDesignPeriodId}/{variableCategoryType}")]
        public IActionResult GetTemplateDropDownByPeriodId(int projectDesignPeriodId,
            VariableCategoryType variableCategoryType)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDownByPeriodId(projectDesignPeriodId,
                variableCategoryType));
        }


        [HttpGet]
        [Route("GetTemplateDropDownAnnotation/{projectDesignVisitId}")]
        public IActionResult GetTemplateDropDownAnnotation(int projectDesignVisitId)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDownAnnotation(projectDesignVisitId));
        }
    }
}