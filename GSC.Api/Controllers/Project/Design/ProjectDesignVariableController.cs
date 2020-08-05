using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignVariableController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IVariableRepository _variableRepository;

        public ProjectDesignVariableController(IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IVariableRepository variableRepository)
        {
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _uow = uow;
            _mapper = mapper;
            _variableRepository = variableRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variable = _projectDesignVariableRepository.FindByInclude(t => t.Id == id, t => t.Values)
                .FirstOrDefault();
            var variableDto = _mapper.Map<ProjectDesignVariableDto>(variable);
            return Ok(variableDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectDesignVariableDto variableDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            variableDto.Id = 0;
            var variable = _mapper.Map<ProjectDesignVariable>(variableDto);
            variable.DesignOrder = 1;

            if (_projectDesignVariableRepository.FindBy(t =>
                    t.ProjectDesignTemplateId == variable.ProjectDesignTemplateId && t.DeletedDate == null).Count() > 0)
                variable.DesignOrder = _projectDesignVariableRepository.FindBy(t =>
                                           t.ProjectDesignTemplateId == variable.ProjectDesignTemplateId &&
                                           t.DeletedDate == null).Max(t => t.DesignOrder) + 1;

            var validate = _projectDesignVariableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectDesignVariableRepository.Add(variable);
            if (_uow.Save() <= 0) throw new Exception("Creating Project Design Variable failed on save.");
            return Ok(variable.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignVariableDto variableDto)
        {
            if (variableDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var variable = _mapper.Map<ProjectDesignVariable>(variableDto);

            var validate = _projectDesignVariableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            UpdateVariableValues(variable);

            _projectDesignVariableRepository.Update(variable);
            if (_uow.Save() <= 0) throw new Exception("Updating Project Design Variable failed on save.");
            return Ok(variable.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignVariableRepository.Find(id);

            if (record == null)
                return NotFound();

            if (record.VariableId != null)
            {
                var variable = _variableRepository.Find((int) record.VariableId);
                if (variable != null && variable.SystemType != null)
                {
                    ModelState.AddModelError("Message", "Can't delete record!");
                    return BadRequest(ModelState);
                }
            }

            _projectDesignVariableRepository.Delete(record);
            _uow.Save();

            if (_projectDesignVariableRepository.FindBy(t =>
                t.ProjectDesignTemplateId == record.ProjectDesignTemplateId && t.DeletedDate == null).Any())
            {
                var minOrder = _projectDesignVariableRepository.FindBy(t =>
                        t.ProjectDesignTemplateId == record.ProjectDesignTemplateId && t.DeletedDate == null)
                    .Min(t => t.DesignOrder);
                var firstId = _projectDesignVariableRepository.FindBy(t =>
                    t.ProjectDesignTemplateId == record.ProjectDesignTemplateId && t.DeletedDate == null &&
                    t.DesignOrder == minOrder).First().Id;
                ChangeVariableDesignOrder(firstId, 0);
            }

            return Ok();
        }

        [HttpGet]
        [Route("GetVariabeDropDown/{projectDesignTemplateId}")]
        public IActionResult GetVariabeDropDown(int projectDesignTemplateId)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeDropDown(projectDesignTemplateId));
        }

        [HttpGet]
        [Route("ChangeVariableDesignOrder/{id}/{index}")]
        public IActionResult ChangeVariableDesignOrder(int id, int index)
        {
            var variable = _projectDesignVariableRepository.Find(id);
            var templateId = variable.ProjectDesignTemplateId;

            var orderedList = _projectDesignVariableRepository
                .FindBy(t => t.ProjectDesignTemplateId == templateId && t.DeletedDate == null)
                .OrderBy(t => t.DesignOrder).ToList();
            orderedList.Remove(orderedList.First(t => t.Id == id));
            orderedList.Insert(index, variable);

            var i = 0;
            foreach (var item in orderedList)
            {
                item.DesignOrder = ++i;
                _projectDesignVariableRepository.Update(item);
            }

            _uow.Save();

            return Ok();
        }

        private void UpdateVariableValues(ProjectDesignVariable variable)
        {
            var data = _projectDesignVariableValueRepository.FindBy(x =>
                x.ProjectDesignVariableId == variable.Id).ToList(); //&& !variable.Values.Any(c => c.Id == x.Id)).ToList();
            var deletevalues = data.Where(t => variable.Values.Where(a => a.Id == t.Id).ToList().Count <= 0).ToList();
                //!variable.Values.Any(c => c.Id == x.Id)).ToList();
            foreach (var value in deletevalues)
                //value.DeletedDate = DateTime.Now;
                //_projectDesignVariableValueRepository.Update(value);

                _uow.Context.Remove(value);
        }

        [HttpGet]
        [Route("GetVariabeAnnotationDropDown/{projectDesignTemplateId}")]
        public IActionResult GetVariabeAnnotationDropDown(int projectDesignTemplateId)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeAnnotationDropDown(projectDesignTemplateId, false));
        }

        [HttpGet]
        [Route("GetVariabeAnnotationDropDown/{projectDesignTemplateId}/{isFormula}")]
        public IActionResult GetVariabeAnnotationDropDown(int projectDesignTemplateId, bool isFormula)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeAnnotationDropDown(projectDesignTemplateId, isFormula));
        }

        [HttpGet]
        [Route("GetVariabeAnnotationByDomainDropDown/{domainId}/{projectId}")]
        public IActionResult GetVariabeAnnotationByDomainDropDown(int domainId, int projectId)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeAnnotationByDomainDropDown(domainId, projectId));
        }

        [HttpGet]
        [Route("GetTargetVariabeAnnotationDropDown/{projectDesignTemplateId}")]
        public IActionResult GetTargetVariabeAnnotationDropDown(int projectDesignTemplateId)
        {
            return Ok(_projectDesignVariableRepository.GetTargetVariabeAnnotationDropDown(projectDesignTemplateId));
        }

        //Added method By Vipul 19022020
        [HttpGet]
        [Route("GetVariabeAnnotationDropDownForProjectDesign/{projectDesignTemplateId}")]
        public IActionResult GetVariabeAnnotationDropDownForProjectDesign(int projectDesignTemplateId)
        {
            return Ok(
                _projectDesignVariableRepository.GetVariabeAnnotationDropDownForProjectDesign(projectDesignTemplateId));
        }

        [HttpGet]
        [Route("GetAnnotationDropDown/{projectDesignId}/{isFormula}")]
        public IActionResult GetAnnotationDropDown(int projectDesignId,bool isFormula)
        {
            return Ok(_projectDesignVariableRepository.GetAnnotationDropDown(projectDesignId, isFormula));
        }

        [HttpGet]
        [Route("GetProjectDesignVariableValueDropDown/{projectDesignVariableId}")]
        public IActionResult GetProjectDesignVariableValueDropDown(int projectDesignVariableId)
        {
            return Ok(
                _projectDesignVariableValueRepository.GetProjectDesignVariableValueDropDown(projectDesignVariableId));
        }

        //Added method By Vipul 25062020

        [HttpGet]
        [Route("GetTargetVariabeAnnotationForScheduleDropDown/{projectDesignTemplateId}")]
        public IActionResult GetTargetVariabeAnnotationForScheduleDropDown(int projectDesignTemplateId)
        {
            return Ok(_projectDesignVariableRepository.GetTargetVariabeAnnotationForScheduleDropDown(projectDesignTemplateId));
        }
    }
}