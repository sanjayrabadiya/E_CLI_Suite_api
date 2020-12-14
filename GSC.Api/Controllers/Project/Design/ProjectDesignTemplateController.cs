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
using GSC.Respository.Project.Schedule;
using GSC.Shared.Extension;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignTemplateController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IUnitOfWork _uow;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly IProjectScheduleTemplateRepository _projectScheduleTemplateRepository;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IProjectDesignTemplateNoteRepository _projectDesignTemplateNoteRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;

        public ProjectDesignTemplateController(IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IVariableTemplateRepository variableTemplateRepository,
            IDomainRepository domainRepository,
            IProjectScheduleTemplateRepository projectScheduleTemplateRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignTemplateNoteRepository projectDesignTemplateNoteRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _mapper = mapper;
            _projectScheduleTemplateRepository = projectScheduleTemplateRepository;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignTemplateNoteRepository = projectDesignTemplateNoteRepository;
            _domainRepository = domainRepository;
        }

        [HttpGet("{projectDesignVisitId}")]
        public IActionResult Get(int projectDesignVisitId)
        {
            if (projectDesignVisitId <= 0) return BadRequest();

            var templates = _projectDesignTemplateRepository.FindByInclude(t =>
                t.ProjectDesignVisitId == projectDesignVisitId
                && t.DeletedDate == null, t => t.Domain).OrderBy(t => t.DesignOrder).ToList();

            var templatesDto = _mapper.Map<IEnumerable<ProjectDesignTemplateDto>>(templates).ToList();
            templatesDto.ForEach(t =>
            {
                t.DomainId = t.DomainId;
                t.DomainName = _domainRepository.Find(t.DomainId)?.DomainName;

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

            _uow.Save();
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
            if (_projectDesignTemplateRepository.All.Any(t => t.ProjectDesignVisitId == projectDesignVisitId))
                designOrder = _projectDesignTemplateRepository.All.Where(t => t.ProjectDesignVisitId == projectDesignVisitId
                && t.DeletedDate == null).Max(t => t.DesignOrder);

            for (var i = 0; i < noOfTemplates; i++)
            {
                var projectDesignTemplate = _mapper.Map<ProjectDesignTemplate>(variableTemplate);
                projectDesignTemplate.Id = 0;
                projectDesignTemplate.ProjectDesignVisitId = projectDesignVisitId;
                projectDesignTemplate.VariableTemplateId = variableTemplateId;
                projectDesignTemplate.DesignOrder = ++designOrder;                
                projectDesignTemplate.Variables = new List<ProjectDesignVariable>();
                projectDesignTemplate.ProjectDesignTemplateNote = new List<ProjectDesignTemplateNote>();

                var variableOrder = 0;
                foreach (var variableDetail in variableTemplate.VariableTemplateDetails)
                {
                    var projectDesignVariable = _mapper.Map<ProjectDesignVariable>(variableDetail.Variable);
                    projectDesignVariable.Id = 0;
                    projectDesignVariable.VariableId = variableDetail.VariableId;
                    projectDesignVariable.DesignOrder = ++variableOrder;
                    projectDesignVariable.Note = variableDetail.Note;
                    _projectDesignVariableRepository.Add(projectDesignVariable);
                    projectDesignTemplate.Variables.Add(projectDesignVariable);

                    projectDesignVariable.Values = new List<ProjectDesignVariableValue>();

                    var valueOrder = 0;
                    foreach (var variableValue in variableDetail.Variable.Values)
                    {
                        var projectDesignVariableValue = _mapper.Map<ProjectDesignVariableValue>(variableValue);
                        projectDesignVariableValue.Id = 0;
                        projectDesignVariableValue.SeqNo = ++valueOrder;
                        _projectDesignVariableValueRepository.Add(projectDesignVariableValue);
                        projectDesignVariable.Values.Add(projectDesignVariableValue);
                    }

                    //Added for Remarks
                    projectDesignVariable.Remarks = new List<ProjectDesignVariableRemarks>();

                    foreach (var variableRemark in variableDetail.Variable.Remarks)
                    {
                        var projectDesignVariableRemark = _mapper.Map<ProjectDesignVariableRemarks>(variableRemark);
                        projectDesignVariableRemark.Id = 0;
                        projectDesignVariable.Remarks.Add(projectDesignVariableRemark);
                    }

                }

                //Added for template Notes
                foreach (var note in variableTemplate.Notes)
                {
                    var projectDesignTemplateNote = _mapper.Map<ProjectDesignTemplateNote>(note);
                    projectDesignTemplateNote.Id = 0;
                    _projectDesignTemplateNoteRepository.Add(projectDesignTemplateNote);
                    projectDesignTemplate.ProjectDesignTemplateNote.Add(projectDesignTemplateNote);
                }

                _projectDesignTemplateRepository.Add(projectDesignTemplate);
            }

            _uow.Save();

            return Ok();
        }

        [HttpGet("CloneTemplate/{projectDesignTempateId}/{noOfClones}")]
        public IActionResult CloneTemplate(int projectDesignTempateId, int noOfClones)
        {
            if (projectDesignTempateId <= 0 || noOfClones <= 0) return BadRequest();

            var projectDesignVisitId =
                _projectDesignTemplateRepository.Find(projectDesignTempateId).ProjectDesignVisitId;

            var designOrder = 0;
            if (_projectDesignTemplateRepository.All.Any(t => t.ProjectDesignVisitId == projectDesignVisitId))
                designOrder = _projectDesignTemplateRepository.All.Where(t => t.ProjectDesignVisitId == projectDesignVisitId).Max(t => t.DesignOrder);

            for (var i = 0; i < noOfClones; i++)
            {
                var temp = _projectDesignTemplateRepository.GetTemplateClone(projectDesignTempateId);

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
                    _projectDesignVariableRepository.Add(variable);
                    variable.Values.ToList().ForEach(r =>
                    {
                        r.Id = 0;
                        _projectDesignVariableValueRepository.Add(r);
                    });
                }

                foreach (var note in projectDesignTemplate.ProjectDesignTemplateNote)
                {
                    note.Id = 0;
                    _projectDesignTemplateNoteRepository.Add(note);
                }

                projectDesignTemplate.ProjectDesignVisit = null;

                _projectDesignTemplateRepository.Add(projectDesignTemplate);
            }

            _uow.Save();

            return Ok();
        }

        [HttpPost("ModifyClonnedTemplates")]
        public IActionResult ModifyClonnedTemplates([FromBody] CloneTemplateDto cloneTemplateDto)
        {
            if (cloneTemplateDto.Id <= 0 || cloneTemplateDto.ClonnedTemplateIds == null ||
                cloneTemplateDto.ClonnedTemplateIds.Count == 0) return BadRequest();

            cloneTemplateDto.ClonnedTemplateIds.ForEach(t =>
            {
                var parent = _projectDesignTemplateRepository.GetTemplateClone(cloneTemplateDto.Id);

                var clonnedTemplate = _projectDesignTemplateRepository.GetTemplateClone(t);
                foreach (var variable in clonnedTemplate.Variables)
                {
                    variable.DeletedDate = DateTime.Now.UtcDate();
                    _projectDesignVariableRepository.Update(variable);
                }


                var variables = parent.Variables.ToList();
                foreach (var variable in variables)
                {
                    variable.Id = 0;
                    foreach (var variableValue in variable.Values)
                    {
                        variableValue.Id = 0;
                        _projectDesignVariableValueRepository.Add(variableValue);
                    }

                    clonnedTemplate.Variables.Add(variable);
                    _projectDesignVariableRepository.Add(variable);
                }

                _projectDesignTemplateRepository.Update(clonnedTemplate);
            });

            _uow.Save();

            return Ok();
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignTemplateRepository.Find(id);

            if (record == null)
                return NotFound();
            // added by vipul validation if template variable use in visit status than it's not deleted on 24092020
            var Exists = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVariable.ProjectDesignTemplateId == id && x.DeletedDate == null).Any();
            if (Exists)
            {
                ModelState.AddModelError("Message", "Template variable use in visit status.");
                return BadRequest(ModelState);
            }
            else
            {
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
        }

        [HttpGet]
        [Route("GetTemplateDropDown/{projectDesignVisitId}")]
        public IActionResult GetTemplateDropDown(int projectDesignVisitId)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDown(projectDesignVisitId));
        }

        [HttpGet]
        [Route("GetTemplateDropDownForProjectSchedule/{projectDesignVisitId}/{collectionSource:int?}/{refVariable:int?}")]
        public IActionResult GetTemplateDropDownForProjectSchedule(int projectDesignVisitId, int? collectionSource = null, int? refVariable = null)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDownForProjectSchedule(projectDesignVisitId, collectionSource, refVariable));
        }

        //added by vipul for get only date time variable template in project design visit on 22092020
        [HttpGet]
        [Route("GetTemplateDropDownForVisitStatus/{projectDesignVisitId}")]
        public IActionResult GetTemplateDropDownForVisitStatus(int projectDesignVisitId)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDownForVisitStatus(projectDesignVisitId));
        }

        [HttpGet]
        [Route("GetClonnedTemplates/{id}")]
        public IActionResult GetClonnedTemplates(int id)
        {
            return Ok(_projectDesignTemplateRepository.GetClonnedTemplateDropDown(id));
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

            if (orderedList.Count() < index)
            {
                index = index - 1;
            }

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
            var projectScheduleTemplate = _projectScheduleTemplateRepository.All.Where(t => t.ProjectDesignTemplateId == id).FirstOrDefault();

            if (projectScheduleTemplate == null)
                return Ok(id);
            return Ok(_projectScheduleTemplateRepository.Find(projectScheduleTemplate.ProjectScheduleId).ProjectDesignTemplateId);
        }

        [HttpGet("GetTemplate/{id}")]
        public IActionResult GetTemplate(int id)
        {
            if (id <= 0) return BadRequest();
            var designTemplate = _projectDesignTemplateRepository.GetTemplate(id);

            // var designTemplateDto = _mapper.Map<ProjectDesignTemplate>(designTemplate);

            return Ok(designTemplate);
        }

        [HttpGet]
        [Route("GetTemplateDropDownByPeriodId/{projectDesignPeriodId}/{variableCategoryType}")]
        public IActionResult GetTemplateDropDownByPeriodId(int projectDesignPeriodId,
            VariableCategoryType variableCategoryType)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDownByPeriodId(projectDesignPeriodId,
                variableCategoryType));
        }

        // Not use any where please check and remove if not use any where comment by vipul
        [HttpGet]
        [Route("GetTemplateDropDownAnnotation/{projectDesignVisitId}")]
        public IActionResult GetTemplateDropDownAnnotation(int projectDesignVisitId)
        {
            return Ok(_projectDesignTemplateRepository.GetTemplateDropDownAnnotation(projectDesignVisitId));
        }
    }
}