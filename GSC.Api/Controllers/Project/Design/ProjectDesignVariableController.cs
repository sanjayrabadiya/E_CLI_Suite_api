using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Report;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using GSC.Respository.LanguageSetup;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.SupplyManagement;
using GSC.Shared.Extension;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignVariableController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;
        private readonly IProjectDesignVariableRemarksRepository _projectDesignVariableRemarksRepository;
        private readonly IUnitOfWork _uow;
        private readonly IVariableRepository _variableRepository;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectDesignVariableEncryptRoleRepository _projectDesignVariableEncryptRoleRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly IVariabeNoteLanguageRepository _variableNoteLanguageRepository;
        private readonly IVariabeValueLanguageRepository _variableValueLanguageRepository;
        private readonly IVariabeLanguageRepository _variabeLanguageRepository;
        private readonly ISupplyManagementAllocationRepository _supplyManagementAllocationRepository;
        public ProjectDesignVariableController(IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IProjectDesignVariableRemarksRepository projectDesignVariableRemarksRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository,
            IProjectDesignVariableEncryptRoleRepository projectDesignVariableEncryptRoleRepository,
            IUnitOfWork uow, IMapper mapper,
            IVariableRepository variableRepository,
            IDomainRepository domainRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IVariabeNoteLanguageRepository variableNoteLanguageRepository,
            IVariabeValueLanguageRepository variableValueLanguageRepository,
            IVariabeLanguageRepository variabeLanguageRepository,
            ISupplyManagementAllocationRepository supplyManagementAllocationRepository
            )
        {
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _projectDesignVariableRemarksRepository = projectDesignVariableRemarksRepository;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _projectDesignVariableEncryptRoleRepository = projectDesignVariableEncryptRoleRepository;
            _uow = uow;
            _mapper = mapper;
            _variableRepository = variableRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _variableNoteLanguageRepository = variableNoteLanguageRepository;
            _variableValueLanguageRepository = variableValueLanguageRepository;
            _variabeLanguageRepository = variabeLanguageRepository;
            _domainRepository = domainRepository;
            _supplyManagementAllocationRepository = supplyManagementAllocationRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variable = _projectDesignVariableRepository.
                FindByInclude(t => t.Id == id, t => t.Values.Where(x => x.DeletedDate == null).OrderBy(x => x.SeqNo), t => t.Roles.Where(x => x.DeletedDate == null)).FirstOrDefault();
            var variableDto = _mapper.Map<ProjectDesignVariableDto>(variable);
            var checkVersion = _projectDesignTemplateRepository.CheckStudyVersionForTemplate(variableDto.ProjectDesignTemplateId);

            variableDto.CollectionValueDisable = checkVersion.VersionNumber == variableDto.StudyVersion;

            if (variableDto.Values != null)
            {
                variableDto.Values.ToList().ForEach(x =>
                {
                    x.AllowActive = checkVersion.VersionNumber == x.InActiveVersion && x.InActiveVersion != null;
                    x.DisplayVersion = x.StudyVersion != null || x.InActiveVersion != null ?
                    "( V : " + x.StudyVersion + (x.StudyVersion != null && x.InActiveVersion != null ? " - " : "") + x.InActiveVersion + ")" : "";
                    x.TableCollectionSourceName = x.TableCollectionSource.GetDescription();
                });
            }

            return Ok(variableDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectDesignVariableDto variableDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            variableDto.Id = 0;

            //For Static Vaiable
            var checkStatic = _projectDesignVariableRepository.NonChangeVariableCode(variableDto);
            if (!string.IsNullOrEmpty(checkStatic))
            {
                ModelState.AddModelError("Message", checkStatic);
                return BadRequest(ModelState);
            }

            var DomainCode = _domainRepository.Find((int)variableDto.DomainId).DomainCode;
            if (DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't add record!");
                return BadRequest(ModelState);
            }

            if (variableDto.Values != null)
                variableDto.Values = variableDto.Values.Where(x => !x.IsDeleted).ToList();

            var variable = _mapper.Map<ProjectDesignVariable>(variableDto);
            variable.DesignOrder = 1;

            if (_projectDesignVariableRepository.All.Any(t => t.ProjectDesignTemplateId == variable.ProjectDesignTemplateId && t.DeletedDate == null && !t.IsHide))
                variable.DesignOrder = _projectDesignVariableRepository.FindBy(t => t.ProjectDesignTemplateId == variable.ProjectDesignTemplateId && !t.IsHide &&
                                           t.DeletedDate == null).Max(t => t.DesignOrder) + 1;

            var validate = _projectDesignVariableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            var checkVersion = _projectDesignTemplateRepository.CheckStudyVersionForTemplate(variable.ProjectDesignTemplateId);
            variable.StudyVersion = checkVersion.VersionNumber;
            _projectDesignVariableRepository.Add(variable);

            var SeqNo = 0;
            foreach (var item in variable.Values)
            {
                item.StudyVersion = checkVersion.VersionNumber;
                item.InActiveVersion = null;
                item.SeqNo = ++SeqNo;
                _projectDesignVariableValueRepository.Add(item);
            }

            if (variable.IsEncrypt)
            {
                foreach (var item in variable.Roles)
                {
                    _projectDesignVariableEncryptRoleRepository.Add(item);
                }
            }

            _uow.Save();
            return Ok(variable.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignVariableDto variableDto)
        {
            if (variableDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //For Static Vaiable
            var checkStatic = _projectDesignVariableRepository.NonChangeVariableCode(variableDto);
            if (!string.IsNullOrEmpty(checkStatic))
            {
                ModelState.AddModelError("Message", checkStatic);
                return BadRequest(ModelState);
            }

            var variable = _mapper.Map<ProjectDesignVariable>(variableDto);

            var lastvariable = _projectDesignVariableRepository.Find(variableDto.Id);
            var DomainCode = _domainRepository.Find((int)lastvariable.DomainId).DomainCode;
            if (DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't edit record!");
                return BadRequest(ModelState);
            }
            if (variable.InActiveVersion == null && _supplyManagementAllocationRepository.All.Any(x => x.ProjectDesignVariableId == variableDto.Id))
            {
                ModelState.AddModelError("Message", "Can't edit record, Already used in Allocation!");
                return BadRequest(ModelState);
            }

            var validate = _projectDesignVariableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            // added by vipul validation if variable use in visit status than data type not except date or datetime deleted on 25092020
            var Exists = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVariableId == variableDto.Id && x.DeletedDate == null).Any();
            if (Exists)
            {
                if (variableDto.CollectionSource != CollectionSources.Date && variableDto.CollectionSource != CollectionSources.DateTime)
                {
                    ModelState.AddModelError("Message", "Variable collection source must be date or date time.");
                    return BadRequest(ModelState);
                }
            }

            UpdateVariableEncryptRole(variable);

            _projectDesignVariableRepository.Update(variable);

            var checkVersion = _projectDesignTemplateRepository.CheckStudyVersionForTemplate(variableDto.ProjectDesignTemplateId);

            _projectDesignVariableValueRepository.UpdateVariableValues(variableDto, variableDto.CollectionValueDisable, checkVersion);

            _uow.Save();

            return Ok(variable.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignVariableRepository.Find(id);

            if (record == null)
                return NotFound();

            record.Domain = _domainRepository.Find((int)record.DomainId);
            if (record.Domain.DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }
            if (record.InActiveVersion == null && _supplyManagementAllocationRepository.All.Any(x => x.ProjectDesignVariableId == id))
            {
                ModelState.AddModelError("Message", "Can't delete record, Already used in Allocation!");
                return BadRequest(ModelState);
            }

            if (record.VariableId != null)
            {
                var variable = _variableRepository.Find((int)record.VariableId);
                if (variable != null && variable.SystemType != null)
                {
                    ModelState.AddModelError("Message", "Can't delete record!");
                    return BadRequest(ModelState);
                }
            }

            // added by vipul validation if variable use in visit status than it's not deleted on 24092020
            var Exists = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVariableId == id && x.DeletedDate == null).Any();
            if (Exists)
            {
                ModelState.AddModelError("Message", "Variable use in visit status.");
                return BadRequest(ModelState);
            }

            var checkVersion = _projectDesignTemplateRepository.CheckStudyVersionForTemplate(record.ProjectDesignTemplateId);
            if (checkVersion.AnyLive)
            {
                record.InActiveVersion = checkVersion.VersionNumber;
                _projectDesignVariableRepository.Update(record);

                var variables = _projectDesignVariableValueRepository.All.Where(x => x.DeletedDate == null
                   && x.ProjectDesignVariableId == id && x.InActiveVersion == null).ToList();
                variables.ForEach(x =>
                {
                    x.InActiveVersion = checkVersion.VersionNumber;
                    _projectDesignVariableValueRepository.Update(x);
                });

            }
            else
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
            if (!variable.IsHide)
            {
                var orderedList = _projectDesignVariableRepository
                    .FindBy(t => t.ProjectDesignTemplateId == templateId && t.DeletedDate == null)
                    .OrderBy(t => t.DesignOrder).ToList();
                orderedList.Remove(orderedList.First(t => t.Id == id));

                if (index != 0)
                    index--;
                orderedList.Insert(index, variable);

                var i = 0;
                foreach (var item in orderedList)
                {
                    if (item.InActiveVersion != null)
                        item.DesignOrder = i;
                    else
                        item.DesignOrder = ++i;
                    _projectDesignVariableRepository.Update(item);
                }

                _uow.Save();
            }
            return Ok();
        }




        private void UpdateVariableEncryptRole(ProjectDesignVariable variable)
        {
            // get role by projectdesign variable id
            var data = _projectDesignVariableEncryptRoleRepository.FindBy(x => x.ProjectDesignVariableId == variable.Id && x.DeletedDate == null).ToList();

            foreach (var item in variable.Roles)
            {
                var role = data.Where(t => t.RoleId == item.RoleId).FirstOrDefault();
                // add role if new select in dropdown
                if (role == null)
                    _projectDesignVariableEncryptRoleRepository.Add(item);
            }
            var RoleIds = variable.Roles.Select(x => new { x.RoleId }).ToList();

            var Exists = data.Where(x => !RoleIds.Any(t => t.RoleId == x.RoleId)).ToList();
            if (Exists.Count != 0)
                foreach (var item in Exists)
                {
                    _projectDesignVariableEncryptRoleRepository.Delete(item);
                }
        }


        [HttpGet]
        [Route("GetVariabeAnnotationDropDownForVisitStatus/{projectDesignTemplateId}")]
        public IActionResult GetVariabeAnnotationDropDownForVisitStatus(int projectDesignTemplateId)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeAnnotationDropDownForVisitStatus(projectDesignTemplateId));
        }

        [HttpGet]
        [Route("GetVariabeAnnotationDropDown/{projectDesignTemplateId}/{isFormula}")]
        public IActionResult GetVariabeAnnotationDropDown(int projectDesignTemplateId, bool isFormula)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeAnnotationDropDown(projectDesignTemplateId, isFormula));
        }

        [HttpGet]
        [Route("GetVariabeAnnotationDropDownforhardsoftfetch/{projectDesignTemplateId}/{variableId}")]
        public IActionResult GetVariabeAnnotationDropDownforhardsoftfetch(int projectDesignTemplateId, int variableId)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeAnnotationDropDownforhardsoftfetch(projectDesignTemplateId, variableId));
        }

        [HttpGet]
        [Route("GetVariabeAnnotationByDomainDropDown/{domainId}/{projectId}")]
        public IActionResult GetVariabeAnnotationByDomainDropDown(int domainId, int projectId)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeAnnotationByDomainDropDown(domainId, projectId));
        }

        [HttpGet]
        [Route("GetAnnotationDropDown/{projectDesignId}/{isFormula}")]
        public IActionResult GetAnnotationDropDown(int projectDesignId, bool isFormula)
        {
            return Ok(_projectDesignVariableRepository.GetAnnotationDropDown(projectDesignId, isFormula));
        }

        // Not Use in front please check and remove if not use comment  by vipul
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

        //For Remarks DDL
        [HttpGet]
        [Route("GetProjectDesignVariableRemarksDropDown/{projectDesignVariableId}")]
        public IActionResult GetProjectDesignVariableRemarksDropDown(int projectDesignVariableId)
        {
            return Ok(
                _projectDesignVariableRemarksRepository.GetProjectDesignVariableRemarksDropDown(projectDesignVariableId));
        }

        [HttpPost]
        [Route("GetVariableByMultipleTemplateDropDown")]
        public IActionResult GetVariableByMultipleTemplateDropDown([FromBody] ProjectDatabaseSearchDto filters)
        {
            return Ok(_projectDesignVariableRepository.GetVariableByMultipleTemplateDropDown(filters.TemplateIds));
        }

        //Get designREport
        [HttpPost]
        [Route("GetDesignReport")]
        public IActionResult GetDesignReport([FromBody] ProjectDatabaseSearchDto search)
        {
            return _projectDesignVariableValueRepository.GetDesignReport(search);
        }


        [HttpGet]
        [Route("GetProjectDesignVariableRelation/{id}")]
        public IActionResult GetProjectDesignVariableRelation(int id)
        {
            var result = _projectDesignVariableRepository.GetProjectDesignVariableRelation(id);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetVariabeBasic/{projectDesignTemplateId}")]
        public IActionResult GetVariabeBasic(int projectDesignTemplateId)
        {
            var checkVersion = _projectDesignTemplateRepository.CheckStudyVersionForTemplate(projectDesignTemplateId);
            return Ok(_projectDesignVariableRepository.GetVariabeBasic(projectDesignTemplateId, checkVersion));
        }

        [HttpPut]
        [Route("SetActiveFromInActive/{id}")]
        public IActionResult SetActiveFromInActive(int id)
        {
            if (id <= 0) return BadRequest();

            var variable = _projectDesignVariableRepository.Find(id);

            if (variable == null) return NotFound();

            var variables = _projectDesignVariableValueRepository.All.Where(x => x.DeletedDate == null && x.ProjectDesignVariableId == id && x.InActiveVersion == variable.InActiveVersion).
                ToList();
            variables.ForEach(x =>
            {
                x.InActiveVersion = null;
                _projectDesignVariableValueRepository.Update(x);
            });

            variable.InActiveVersion = null;
            _projectDesignVariableRepository.Update(variable);

            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVariabeDropDownForLabManagementMapping/{projectDesignTemplateId}")]
        public IActionResult GetVariabeDropDownForLabManagementMapping(int projectDesignTemplateId)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeDropDownForLabManagementMapping(projectDesignTemplateId));
        }

        // added by vipul take only variable which have no collection source for relation variable collection source
        [HttpGet]
        [Route("GetVariabeDropDownForRelationMapping/{projectDesignTemplateId}")]
        public IActionResult GetVariabeDropDownForRelationMapping(int projectDesignTemplateId)
        {
            return Ok(_projectDesignVariableRepository.GetVariabeDropDownForRelationMapping(projectDesignTemplateId));
        }


        //Action add by Tinku Mahato (12-04-2022) for copy variable & varialbe value langualge
        [HttpGet("CopyVariable/{copyVarialbeId}/{saveVarialbeId}")]
        public IActionResult CopyVariable(int copyVarialbeId, int saveVarialbeId)
        {
            var variableValues = _projectDesignVariableValueRepository.FindBy(q => q.ProjectDesignVariableId == saveVarialbeId && q.DeletedDate == null).ToList();

            variableValues.ForEach(s =>
            {
                var copyVarialbeValues = _projectDesignVariableValueRepository.FindBy(a => a.ProjectDesignVariableId == copyVarialbeId && a.ValueCode == s.ValueCode && s.DeletedDate == null).FirstOrDefault();

                var varialbeValueLanguages = _variableValueLanguageRepository.FindBy(q => q.ProjectDesignVariableValueId == copyVarialbeValues.Id && q.DeletedDate == null).ToList();

                varialbeValueLanguages.ForEach(m =>
                {
                    m.Id = 0;
                    m.ProjectDesignVariableValueId = s.Id;
                    m.ModifiedBy = null;
                    m.ModifiedDate = null;
                    m.DeletedBy = null;
                    m.DeletedDate = null;
                    _variableValueLanguageRepository.Add(m);
                    _uow.Save();
                });
            });


            var variableLanguages = _variabeLanguageRepository.FindBy(q => q.ProjectDesignVariableId == copyVarialbeId && q.DeletedDate == null).ToList();
            variableLanguages.ForEach(s =>
            {
                s.Id = 0;
                s.ProjectDesignVariableId = saveVarialbeId;
                s.ModifiedBy = null;
                s.ModifiedDate = null;
                s.DeletedBy = null;
                s.DeletedDate = null;
                _variabeLanguageRepository.Add(s);
                _uow.Save();
            });

            var variableNotLanguages = _variableNoteLanguageRepository.FindBy(q => q.ProjectDesignVariableId == copyVarialbeId && q.DeletedDate == null).ToList();
            variableNotLanguages.ForEach(m =>
            {
                m.Id = 0;
                m.ProjectDesignVariableId = saveVarialbeId;
                m.ModifiedBy = null;
                m.ModifiedDate = null;
                m.DeletedBy = null;
                m.DeletedDate = null;
                _variableNoteLanguageRepository.Add(m);
                _uow.Save();
            });

            _uow.Save();

            return Ok(saveVarialbeId);
        }

    }
}