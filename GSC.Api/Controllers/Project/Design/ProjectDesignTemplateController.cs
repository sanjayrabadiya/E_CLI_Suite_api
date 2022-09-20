using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using GSC.Respository.LanguageSetup;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Schedule;
using GSC.Respository.SupplyManagement;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
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
        private readonly IProjectDesignVariableRemarksRepository _projectDesignVariableRemarksRepository;
        private readonly ITemplateLanguageRepository _templateLanguageRepository;
        private readonly ITemplateNoteLanguageRepository _templateNoteLanguageRepository;
        private readonly IVariabeLanguageRepository _variableLanguageRepository;
        private readonly IVariabeNoteLanguageRepository _variableNoteLanguageRepository;
        private readonly IVariabeValueLanguageRepository _variableValueLanguageRepository;
        private readonly IProjectDesignVariableEncryptRoleRepository _projectDesignVariableEncryptRoleRepository;
        private readonly IProjectDesingTemplateRestrictionRepository _templatePermissioRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly ISupplyManagementAllocationRepository _supplyManagementAllocationRepository;
        public ProjectDesignTemplateController(IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IVariableTemplateRepository variableTemplateRepository,
            IDomainRepository domainRepository,
            IProjectScheduleTemplateRepository projectScheduleTemplateRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignTemplateNoteRepository projectDesignTemplateNoteRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IProjectDesignVariableRemarksRepository projectDesignVariableRemarksRepository,
            ITemplateLanguageRepository templateLanguageRepository,
            ITemplateNoteLanguageRepository templateNoteLanguageRepository,
            IVariabeLanguageRepository variableLanguageRepository,
            IVariabeNoteLanguageRepository variableNoteLanguageRepository,
            IVariabeValueLanguageRepository variableValueLanguageRepository,
            IProjectDesignVariableEncryptRoleRepository projectDesignVariableEncryptRoleRepository,
            IProjectDesingTemplateRestrictionRepository templatePermissioRepository,
        IUnitOfWork uow, IMapper mapper,
         IJwtTokenAccesser jwtTokenAccesser,
         ISupplyManagementAllocationRepository supplyManagementAllocationRepository)
        {
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _variableTemplateRepository = variableTemplateRepository;
            _uow = uow;
            _mapper = mapper;
            _projectScheduleTemplateRepository = projectScheduleTemplateRepository;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _projectDesignVariableRemarksRepository = projectDesignVariableRemarksRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _templateLanguageRepository = templateLanguageRepository;
            _templateNoteLanguageRepository = templateNoteLanguageRepository;
            _variableLanguageRepository = variableLanguageRepository;
            _variableNoteLanguageRepository = variableNoteLanguageRepository;
            _variableValueLanguageRepository = variableValueLanguageRepository;
            _projectDesignTemplateNoteRepository = projectDesignTemplateNoteRepository;
            _domainRepository = domainRepository;
            _projectDesignVariableEncryptRoleRepository = projectDesignVariableEncryptRoleRepository;
            _templatePermissioRepository = templatePermissioRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _supplyManagementAllocationRepository = supplyManagementAllocationRepository;
        }

        [HttpGet("{projectDesignVisitId}")]
        public IActionResult Get(int projectDesignVisitId)
        {
            if (projectDesignVisitId <= 0) return BadRequest();

            var templates = _projectDesignTemplateRepository.GetTemplateByVisitId(projectDesignVisitId);
            templates.ForEach(t =>
            {
                t.DomainName = _domainRepository.Find(t.DomainId)?.DomainName;

            });

            return Ok(templates);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignTemplateDto projectDesignTemplateDto)
        {
            if (projectDesignTemplateDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var lastForm = _projectDesignTemplateRepository.Find(projectDesignTemplateDto.Id);
            var DomainCode = _domainRepository.Find((int)lastForm.DomainId).DomainCode;
            if (DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't edit record!");
                return BadRequest(ModelState);
            }
            if (_supplyManagementAllocationRepository.All.Any(x => x.ProjectDesignTemplateId == projectDesignTemplateDto.Id))
            {
                ModelState.AddModelError("Message", "Can't edit record, Already used in Allocation!");
                return BadRequest(ModelState);
            }
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

            var checkVersion = _projectDesignTemplateRepository.CheckStudyVersion(projectDesignVisitId);

            var designOrder = 0;
            if (_projectDesignTemplateRepository.All.Any(t => t.ProjectDesignVisitId == projectDesignVisitId && t.DeletedDate == null))
                designOrder = _projectDesignTemplateRepository.All.Where(t => t.ProjectDesignVisitId == projectDesignVisitId
                && t.DeletedDate == null).Max(t => t.DesignOrder);

            for (var i = 0; i < noOfTemplates; i++)
            {
                var projectDesignTemplate = _mapper.Map<ProjectDesignTemplate>(variableTemplate);
                projectDesignTemplate.Id = 0;
                projectDesignTemplate.ProjectDesignVisitId = projectDesignVisitId;
                projectDesignTemplate.StudyVersion = checkVersion.VersionNumber;
                projectDesignTemplate.VariableTemplateId = variableTemplateId;
                projectDesignTemplate.DesignOrder = ++designOrder;
                projectDesignTemplate.Variables = new List<ProjectDesignVariable>();
                projectDesignTemplate.ProjectDesignTemplateNote = new List<ProjectDesignTemplateNote>();

                var variableOrder = 0;
                foreach (var variableDetail in variableTemplate.VariableTemplateDetails)
                {
                    var projectDesignVariable = _mapper.Map<ProjectDesignVariable>(variableDetail.Variable);
                    projectDesignVariable.Id = 0;
                    projectDesignVariable.StudyVersion = checkVersion.VersionNumber;
                    projectDesignVariable.VariableId = variableDetail.VariableId;
                    if (projectDesignVariable.InActiveVersion != null)
                        projectDesignVariable.DesignOrder = variableOrder;
                    else
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
                        projectDesignVariableValue.StudyVersion = checkVersion.VersionNumber;
                        projectDesignVariableValue.SeqNo = ++valueOrder;
                        _projectDesignVariableValueRepository.Add(projectDesignVariableValue);
                        projectDesignVariable.Values.Add(projectDesignVariableValue);
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
        [TransactionRequired]
        public IActionResult CloneTemplate(int projectDesignTempateId, int noOfClones)
        {
            if (projectDesignTempateId <= 0 || noOfClones <= 0) return BadRequest();

            var projectDesignVisitId =
                _projectDesignTemplateRepository.Find(projectDesignTempateId).ProjectDesignVisitId;

            var designOrder = 0;
            if (_projectDesignTemplateRepository.All.Any(t => t.ProjectDesignVisitId == projectDesignVisitId))
                designOrder = _projectDesignTemplateRepository.All.Where(t => t.ProjectDesignVisitId == projectDesignVisitId).Max(t => t.DesignOrder);

            var checkVersion = _projectDesignTemplateRepository.CheckStudyVersion(projectDesignVisitId);

            for (var i = 0; i < noOfClones; i++)
            {
                var temp = _projectDesignTemplateRepository.GetTemplateClone(projectDesignTempateId);

                var projectDesignTemplate = _mapper.Map<ProjectDesignTemplate>(temp);
                projectDesignTemplate.Id = 0;
                projectDesignTemplate.ParentId = projectDesignTempateId;
                projectDesignTemplate.DesignOrder = ++designOrder;
                projectDesignTemplate.VariableTemplate = null;
                projectDesignTemplate.Domain = null;
                projectDesignTemplate.StudyVersion = checkVersion.VersionNumber;
                projectDesignTemplate.InActiveVersion = null;
                projectDesignTemplate.TemplateName = projectDesignTemplate.TemplateName + "_" + designOrder;

                foreach (var variable in projectDesignTemplate.Variables)
                {
                    variable.Id = 0;
                    variable.Unit = null;
                    variable.VariableCategory = null;
                    variable.ProjectDesignTemplate = null;
                    variable.StudyVersion = checkVersion.VersionNumber;
                    variable.InActiveVersion = null;
                    _projectDesignVariableRepository.Add(variable);

                    //For variable clone language
                    variable.VariableLanguage.ToList().ForEach(r =>
                     {
                         r.Id = 0;
                         _variableLanguageRepository.Add(r);
                     });

                    //For variable note clone language
                    variable.VariableNoteLanguage.ToList().ForEach(r =>
                    {
                        r.Id = 0;
                        _variableNoteLanguageRepository.Add(r);
                    });

                    var Seq = 0;
                    variable.Values.ToList().ForEach(r =>
                      {
                          r.Id = 0;
                          r.StudyVersion = checkVersion.VersionNumber;
                          r.InActiveVersion = null;
                          r.SeqNo = ++Seq;
                          _projectDesignVariableValueRepository.Add(r);
                          //For variable value clone language
                          r.VariableValueLanguage.ToList().ForEach(x =>
                            {
                                x.Id = 0;
                                _variableValueLanguageRepository.Add(x);
                            });
                      });

                    // For encrypt clone
                    variable.Roles.ToList().ForEach(r =>
                    {
                        r.Id = 0;
                        _projectDesignVariableEncryptRoleRepository.Add(r);
                    });
                }

                foreach (var note in projectDesignTemplate.ProjectDesignTemplateNote)
                {
                    note.Id = 0;
                    _projectDesignTemplateNoteRepository.Add(note);

                    //For template note clone language
                    note.TemplateNoteLanguage.ToList().ForEach(x =>
                    {
                        x.Id = 0;
                        _templateNoteLanguageRepository.Add(x);
                    });

                }

                foreach (var permission in projectDesignTemplate.ProjectDesingTemplateRestriction)
                {
                    permission.Id = 0;
                    _templatePermissioRepository.Add(permission);
                }

                projectDesignTemplate.ProjectDesignVisit = null;

                _projectDesignTemplateRepository.Add(projectDesignTemplate);

                //For template clone language
                temp.TemplateLanguage.ToList().ForEach(x =>
                {
                    x.Id = 0;
                    _templateLanguageRepository.Add(x);
                });
            }

            _uow.Save();

            return Ok();
        }

        [HttpPost("ModifyClonnedTemplates")]
        [TransactionRequired]
        public IActionResult ModifyClonnedTemplates([FromBody] CloneTemplateDto cloneTemplateDto)
        {
            if (cloneTemplateDto.Id <= 0 || cloneTemplateDto.ClonnedTemplateIds == null ||
                cloneTemplateDto.ClonnedTemplateIds.Count == 0) return BadRequest();

            var parent = _projectDesignTemplateRepository.GetTemplateClone(cloneTemplateDto.Id);

            var checkVersion = _projectDesignTemplateRepository.CheckStudyVersion(parent.ProjectDesignVisitId);

            cloneTemplateDto.ClonnedTemplateIds.ForEach(t =>
            {
                var clonnedTemplate = _projectDesignTemplateRepository.GetTemplateClone(t);

                var variables = parent.Variables.ToList();
                foreach (var variable in variables)
                {
                    variable.ProjectDesignTemplateId = t;
                    var parentVariable = clonnedTemplate.Variables.FirstOrDefault(x => x.VariableCode == variable.VariableCode);
                    if (parentVariable == null)
                    {
                        variable.Id = 0;
                        variable.StudyVersion = checkVersion.VersionNumber;
                        _projectDesignVariableRepository.Add(variable);

                    }
                    else
                    {
                        variable.Id = parentVariable.Id;
                        _projectDesignVariableRepository.Update(variable);
                    }

                    //For variable clone language
                    variable.VariableLanguage.ToList().ForEach(r =>
                    {
                        r.Id = 0;
                        r.ProjectDesignVariableId = variable.Id;
                        r.ProjectDesignVariable = variable;

                        if (parentVariable != null)
                        {
                            var clone = parentVariable.VariableLanguage.Where(x => x.Display == r.Display).FirstOrDefault();
                            r.Id = clone?.Id ?? 0;
                        }

                        if (r.Id == 0)
                            _variableLanguageRepository.Add(r);
                        else
                            _variableLanguageRepository.Update(r);

                    });

                    //For variable note clone language
                    variable.VariableNoteLanguage.ToList().ForEach(r =>
                    {
                        r.Id = 0;
                        r.ProjectDesignVariableId = variable.Id;
                        r.ProjectDesignVariable = variable;

                        if (parentVariable != null)
                        {
                            var clone = parentVariable.VariableNoteLanguage.Where(x => x.Display == r.Display).FirstOrDefault();
                            r.Id = clone?.Id ?? 0;
                        }

                        if (r.Id == 0)
                            _variableNoteLanguageRepository.Add(r);
                        else
                            _variableNoteLanguageRepository.Update(r);

                    });

                    // For encrypt clone
                    variable.Roles.ToList().ForEach(r =>
                    {
                        r.Id = 0;
                        r.ProjectDesignVariableId = variable.Id;
                        r.ProjectDesignVariable = variable;

                        if (parentVariable != null)
                        {
                            var clone = parentVariable.Roles.Where(x => x.RoleId == r.RoleId).FirstOrDefault();
                            r.Id = clone?.Id ?? 0;
                        }

                        if (r.Id == 0)
                            _projectDesignVariableEncryptRoleRepository.Add(r);
                        else
                            _projectDesignVariableEncryptRoleRepository.Update(r);
                    });

                    var Seq = 0;
                    foreach (var r in variable.Values)
                    {
                        r.Id = 0;
                        r.SeqNo = ++Seq;
                        r.ProjectDesignVariableId = variable.Id;
                        r.ProjectDesignVariable = variable;
                        ProjectDesignVariableValue cloneValue = null;
                        if (parentVariable != null)
                        {
                            cloneValue = parentVariable.Values.Where(x => x.ValueCode == r.ValueCode).FirstOrDefault();
                            r.Id = cloneValue?.Id ?? 0;
                        }

                        if (r.Id == 0)
                        {
                            r.StudyVersion = checkVersion.VersionNumber;
                            _projectDesignVariableValueRepository.Add(r);
                        }

                        else
                            _projectDesignVariableValueRepository.Update(r);

                        //For variable value clone language
                        r.VariableValueLanguage.ToList().ForEach(x =>
                        {
                            x.Id = 0;
                            x.ProjectDesignVariableValue = r;
                            x.ProjectDesignVariableValueId = r.Id;

                            if (cloneValue != null)
                            {
                                var clone = cloneValue.VariableValueLanguage.Where(b => b.LanguageId == x.LanguageId).FirstOrDefault();
                                x.Id = clone?.Id ?? 0;
                            }
                            // Change by Tinku Mahato (14-04-2022)
                            if (x.Id == 0)
                                _variableValueLanguageRepository.Add(x);
                            else
                                _variableValueLanguageRepository.Update(x);
                        });
                    }
                }

                _uow.Save(); //Change by Tinku Mahato
            });
            //_uow.Save(); (Error occured when save clone template)

            return Ok();
        }


        [HttpDelete("{id}")]
        [TransactionRequired()]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignTemplateRepository.Find(id);

            if (record == null)
                return NotFound();

            record.Domain = _domainRepository.Find((int)record.DomainId);
            if (record.Domain.DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }
            if (_supplyManagementAllocationRepository.All.Any(x => x.ProjectDesignTemplateId == id))
            {
                ModelState.AddModelError("Message", "Can't delete record, Already used in Allocation!");
                return BadRequest(ModelState);
            }

            // added by vipul validation if template variable use in visit status than it's not deleted on 24092020
            var Exists = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVariable.ProjectDesignTemplateId == id && x.DeletedDate == null).Any();
            if (Exists)
            {
                ModelState.AddModelError("Message", "Template variable use in visit status.");
                return BadRequest(ModelState);
            }
            else
            {
                var checkVersion = _projectDesignTemplateRepository.CheckStudyVersion(record.ProjectDesignVisitId);
                if (checkVersion.AnyLive)
                {
                    record.InActiveVersion = checkVersion.VersionNumber;
                    _projectDesignTemplateRepository.Update(record);

                    var variables = _projectDesignVariableRepository.All.Where(x => x.DeletedDate == null
                    && x.ProjectDesignTemplateId == id && x.InActiveVersion == null).ToList();
                    variables.ForEach(x =>
                    {
                        x.InActiveVersion = checkVersion.VersionNumber;
                        _projectDesignVariableRepository.Update(x);
                    });
                }
                else
                {
                    _projectDesignTemplateRepository.Delete(record);
                }
                _uow.Save();

                int i = 0;
                var lists = _projectDesignTemplateRepository.All.Where(x => x.ProjectDesignVisitId == record.ProjectDesignVisitId && x.DeletedDate == null).OrderBy(r => r.DesignOrder).ToList();
                lists.ForEach(r =>
                {
                    if (r.InActiveVersion != null)
                        r.DesignOrder = i;
                    else
                        r.DesignOrder = ++i;
                    _projectDesignTemplateRepository.Update(r);
                });
                _uow.Save();
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

            if (index != 0)
                index--;

            orderedList.Insert(index, template);

            var i = 0;
            foreach (var item in orderedList)
            {
                if (item.InActiveVersion != null)
                    item.DesignOrder = i;
                else
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


        [HttpGet]
        [Route("IsTemplateExits/{projectDesignId}")]
        public async Task<IActionResult> IsTemplateExits(int projectDesignId)
        {
            var result = await _projectDesignTemplateRepository.IsTemplateExits(projectDesignId);
            return Ok(result);
        }


        [HttpPut]
        [Route("SetActiveFromInActive/{id}")]
        [TransactionRequired]
        public IActionResult SetActiveFromInActive(int id)
        {
            if (id <= 0) return BadRequest();

            var template = _projectDesignTemplateRepository.Find(id);

            if (template == null) return NotFound();

            var variables = _projectDesignVariableRepository.All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == id && x.InActiveVersion == template.InActiveVersion).
                ToList();
            variables.ForEach(x =>
            {
                x.InActiveVersion = null;
                _projectDesignVariableRepository.Update(x);
            });

            template.InActiveVersion = null;
            _projectDesignTemplateRepository.Update(template);

            _uow.Save();
            int i = 0;
            var lists = _projectDesignTemplateRepository.All.Where(x => x.ProjectDesignVisitId == template.ProjectDesignVisitId && x.DeletedDate == null).OrderBy(r => r.DesignOrder).ToList();
            lists.ForEach(r =>
            {
                if (r.InActiveVersion != null)
                    r.DesignOrder = i;
                else
                    r.DesignOrder = ++i;
                _projectDesignTemplateRepository.Update(r);
            });
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateSetting/{templateId}")]
        public IActionResult GetTemplateSetting(int templateId)
        {
            var projectDesignTemplateSetting = _projectDesignTemplateRepository.GetTemplateSetting(templateId);
            return Ok(projectDesignTemplateSetting);
        }

        [HttpPut("UpdateTemplateSetting")]
        public IActionResult UpdateTemplateSetting([FromBody] ProjectDesignTemplateDto projectDesignTemplateDto)
        {
            if (projectDesignTemplateDto.Id <= 0) return BadRequest();

            var projectDesignTemplate = _mapper.Map<ProjectDesignTemplate>(projectDesignTemplateDto);
            _projectDesignTemplateRepository.Update(projectDesignTemplate);

            if (_uow.Save() <= 0) throw new Exception("setting Failed.");
            return Ok();
        }
    }
}