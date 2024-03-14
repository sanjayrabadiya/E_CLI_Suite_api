using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Helper;
using GSC.Respository.LanguageSetup;
using GSC.Respository.Project.Design;
using GSC.Respository.SupplyManagement;
using GSC.Shared.Extension;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignVisitController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IUnitOfWork _uow;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectDesignTemplateNoteRepository _projectDesignTemplateNoteRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        private readonly IVisitLanguageRepository _visitLanguageRepository;
        private readonly ITemplateLanguageRepository _templateLanguageRepository;
        private readonly ITemplateNoteLanguageRepository _templateNoteLanguageRepository;
        private readonly IVariabeLanguageRepository _variableLanguageRepository;
        private readonly IVariabeNoteLanguageRepository _variableNoteLanguageRepository;
        private readonly IVariabeValueLanguageRepository _variableValueLanguageRepository;
        private readonly IProjectDesignVariableEncryptRoleRepository _projectDesignVariableEncryptRoleRepository;
        private readonly IProjectDesingTemplateRestrictionRepository _templatePermissioRepository;
        private readonly IWorkflowTemplateRepository _workflowTemplateRepository;
        private readonly IVisitEmailConfigurationRepository _visitEmailConfigurationRepository;
        private readonly IVisitEmailConfigurationRolesRepository _visitEmailConfigurationRolesRepository;

        public ProjectDesignVisitController(IProjectDesignVisitRepository projectDesignVisitRepository,
            IUnitOfWork uow, IMapper mapper,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignTemplateNoteRepository projectDesignTemplateNoteRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository,
            IVisitLanguageRepository visitLanguageRepository,
            ITemplateLanguageRepository templateLanguageRepository,
            ITemplateNoteLanguageRepository templateNoteLanguageRepository,
            IVariabeLanguageRepository variableLanguageRepository,
            IVariabeNoteLanguageRepository variableNoteLanguageRepository,
            IVariabeValueLanguageRepository variableValueLanguageRepository,
            IProjectDesignVariableEncryptRoleRepository projectDesignVariableEncryptRoleRepository,
            IProjectDesingTemplateRestrictionRepository templatePermissioRepository,
            IWorkflowTemplateRepository workflowTemplateRepository,
            IVisitEmailConfigurationRepository visitEmailConfigurationRepository,
            IVisitEmailConfigurationRolesRepository visitEmailConfigurationRolesRepository)
        {
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _uow = uow;
            _mapper = mapper;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignTemplateNoteRepository = projectDesignTemplateNoteRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _visitLanguageRepository = visitLanguageRepository;
            _templateLanguageRepository = templateLanguageRepository;
            _templateNoteLanguageRepository = templateNoteLanguageRepository;
            _variableLanguageRepository = variableLanguageRepository;
            _variableNoteLanguageRepository = variableNoteLanguageRepository;
            _variableValueLanguageRepository = variableValueLanguageRepository;
            _projectDesignVariableEncryptRoleRepository = projectDesignVariableEncryptRoleRepository;
            _templatePermissioRepository = templatePermissioRepository;
            _workflowTemplateRepository = workflowTemplateRepository;
            _visitEmailConfigurationRepository = visitEmailConfigurationRepository;
            _visitEmailConfigurationRolesRepository = visitEmailConfigurationRolesRepository;
        }

        [HttpGet("{id}/{projectDesignPeriodId}")]
        public IActionResult Get(int id, int projectDesignPeriodId)
        {
            if (id <= 0 || projectDesignPeriodId <= 0) return BadRequest();
            var projectDesignVisit = _projectDesignVisitRepository
                .FindBy(t => t.Id == id && t.ProjectDesignPeriodId == projectDesignPeriodId).FirstOrDefault();

            if (projectDesignVisit == null) return NotFound();

            var projectDesignVisitDto = _mapper.Map<ProjectDesignVisitDto>(projectDesignVisit);

            return Ok(projectDesignVisitDto);
        }


        [HttpGet]
        [Route("GetVisitList/{projectDesignPeriodId}")]
        public IActionResult GetVisitList(int projectDesignPeriodId)
        {
            return Ok(_projectDesignVisitRepository.GetVisitList(projectDesignPeriodId));
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectDesignVisitDto projectDesignVisitDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var projectDesignVisit = _mapper.Map<ProjectDesignVisit>(projectDesignVisitDto);

            var validateMessage = _projectDesignVisitRepository.Duplicate(projectDesignVisit);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }
            var checkVersion = _projectDesignVisitRepository.CheckStudyVersion(projectDesignVisit.ProjectDesignPeriodId);

            var designOrder = 0;
            if (_projectDesignVisitRepository.All.Any(t => t.ProjectDesignPeriodId == projectDesignVisit.ProjectDesignPeriodId && t.DeletedDate == null))
                designOrder = (int)_projectDesignVisitRepository.All.Where(t => t.ProjectDesignPeriodId == projectDesignVisit.ProjectDesignPeriodId
                && t.DeletedDate == null).Max(t => t.DesignOrder);

            projectDesignVisit.DesignOrder = ++designOrder;
            projectDesignVisit.StudyVersion = checkVersion.VersionNumber;
            _projectDesignVisitRepository.Add(projectDesignVisit);
            _uow.Save();

            return Ok(projectDesignVisit.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignVisitDto projectDesignVisitDto)
        {
            if (projectDesignVisitDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var lastForm = _projectDesignVisitRepository.Find(projectDesignVisitDto.Id);
            if (lastForm.Description == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't edit visit!");
                return BadRequest(ModelState);
            }


            var projectDesignVisit = _mapper.Map<ProjectDesignVisit>(projectDesignVisitDto);

            var validateMessage = _projectDesignVisitRepository.Duplicate(projectDesignVisit);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }
            var validateIWRS = _projectDesignVisitRepository.ValidationVisitIWRS(projectDesignVisit);
            if (!string.IsNullOrEmpty(validateIWRS))
            {
                ModelState.AddModelError("Message", validateIWRS);
                return BadRequest(ModelState);
            }
            _projectDesignVisitRepository.Update(projectDesignVisit);

            _uow.Save();
            return Ok(projectDesignVisit.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest();

            var visit = _projectDesignVisitRepository.Find(id);

            if (visit == null) return NotFound();

            if (visit.DisplayName == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't delete visit!");
                return BadRequest(ModelState);
            }
            var validateIWRS = _projectDesignVisitRepository.ValidationVisitIWRS(visit);
            if (!string.IsNullOrEmpty(validateIWRS))
            {
                ModelState.AddModelError("Message", validateIWRS);
                return BadRequest(ModelState);
            }
            var checkVersion = _projectDesignVisitRepository.CheckStudyVersion(visit.ProjectDesignPeriodId);


            if (checkVersion.AnyLive)
            {
                visit.InActiveVersion = checkVersion.VersionNumber;
                _projectDesignVisitRepository.Update(visit);

                var templates = _projectDesignTemplateRepository.All.Where(x => x.DeletedDate == null
                  && x.ProjectDesignVisitId == id && x.InActiveVersion == null).ToList();
                templates.ForEach(x =>
                {
                    x.InActiveVersion = checkVersion.VersionNumber;
                    _projectDesignTemplateRepository.Update(x);
                });

                var variables = _projectDesignVariableRepository.All.Where(x => x.DeletedDate == null
                   && x.ProjectDesignTemplate.ProjectDesignVisitId == id && x.InActiveVersion == null).ToList();
                variables.ForEach(x =>
                {
                    x.InActiveVersion = checkVersion.VersionNumber;
                    _projectDesignVariableRepository.Update(x);
                });
            }
            else
                _projectDesignVisitRepository.Delete(visit);

            _uow.Save();

            return Ok();
        }


        [HttpPut]
        [Route("SetActiveFromInActive/{id}")]
        public IActionResult SetActiveFromInActive(int id)
        {
            if (id <= 0) return BadRequest();

            var visit = _projectDesignVisitRepository.Find(id);

            if (visit == null) return NotFound();

            var templates = _projectDesignTemplateRepository.All.Where(x => x.DeletedDate == null
                 && x.ProjectDesignVisitId == id && x.InActiveVersion == visit.InActiveVersion).ToList();
            templates.ForEach(x =>
            {
                x.InActiveVersion = null;
                _projectDesignTemplateRepository.Update(x);
            });

            var variables = _projectDesignVariableRepository.All.Where(x => x.DeletedDate == null
               && x.ProjectDesignTemplate.ProjectDesignVisitId == id && x.InActiveVersion == visit.InActiveVersion).ToList();
            variables.ForEach(x =>
            {
                x.InActiveVersion = null;
                _projectDesignVariableRepository.Update(x);
            });

            visit.InActiveVersion = null;
            _projectDesignVisitRepository.Update(visit);

            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVisitDropDown/{projectDesignPeriodId}")]
        public IActionResult GetVisitDropDown(int projectDesignPeriodId)
        {
            return Ok(_projectDesignVisitRepository.GetVisitDropDown(projectDesignPeriodId));
        }

        [HttpGet]
        [Route("GetVisitDropDownByProjectId/{ProjectId}")]
        public IActionResult GetVisitDropDownByProjectId(int ProjectId)
        {
            return Ok(_projectDesignVisitRepository.GetVisitDropDownByProjectId(ProjectId));
        }

        [HttpGet]
        [Route("GetVisitsByProjectDesignId/{projectDesignId}")]
        public IActionResult GetVisitsByProjectDesignId(int projectDesignId)
        {
            return Ok(_projectDesignVisitRepository.GetVisitsByProjectDesignId(projectDesignId));
        }

        [HttpGet]
        [Route("CloneVisit")]
        [TransactionRequired]
        public IActionResult CloneVisit([FromQuery] ProjectDesignVisitClone data)
        {
            var saved = _projectDesignVisitRepository.FindBy(t => t.ProjectDesignPeriodId == data.projectDesignPeriodId && t.DeletedDate == null).Count();
            ProjectDesignVisit firstSaved = null;
            var checkVersion = _projectDesignVisitRepository.CheckStudyVersion(data.projectDesignPeriodId);

            var designOrder = 0;
            if (_projectDesignVisitRepository.All.Any(t => t.ProjectDesignPeriodId == data.projectDesignPeriodId && t.DeletedDate == null))
                designOrder = (int)_projectDesignVisitRepository.All.Where(t => t.ProjectDesignPeriodId == data.projectDesignPeriodId
                && t.DeletedDate == null).Max(t => t.DesignOrder);

            for (var i = 1; i <= data.noOfVisits; i++)
            {
                var visitStatus = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == data.Id).ToList();
                var visitEmails = _visitEmailConfigurationRepository.All.Where(x => x.ProjectDesignVisitId == data.Id).Include(i => i.VisitEmailConfigurationRoles).ToList();

                var visit = _projectDesignVisitRepository.GetVisit(data.Id);
                visit.Id = 0;
                visit.StudyVersion = checkVersion.VersionNumber;
                visit.InActiveVersion = null;
                visit.ProjectDesignPeriodId = data.projectDesignPeriodId;
                visit.DesignOrder = ++designOrder;

                visit.Templates.Where(z => (!data.noOfTemplate.Any() || data.noOfTemplate.Contains(z.Id))).ToList().ForEach(template =>
                {
                    CloneTemplate(visit, template, checkVersion, designOrder, visitStatus);
                });

                visit.DisplayName = "Visit " + ++saved;
                visit.IsSchedule = false;
                _projectDesignVisitRepository.Add(visit);

                //For visit clone language
                visit.VisitLanguage.ToList().ForEach(x =>
                {
                    x.Id = 0;
                    _visitLanguageRepository.Add(x);
                });

                visitStatus.ForEach(e =>
                {
                    e.Id = 0;
                    e.ProjectDesignVisitId = 0;
                    e.ProjectDesignVisit = visit;
                    _projectDesignVisitStatusRepository.Add(e);
                });

                visitEmails.ForEach(e =>
                {
                    e.Id = 0;
                    e.ProjectDesignVisitId = 0;
                    e.ProjectDesignVisit = visit;
                    _visitEmailConfigurationRepository.Add(e);
                    e.VisitEmailConfigurationRoles.ForEach(m =>
                    {
                        m.Id = 0;
                        m.VisitEmailConfigurationId = 0;
                        m.VisitEmailConfiguration = e;
                        _visitEmailConfigurationRolesRepository.Add(m);
                    });
                });

                if (data.visitIds != null)
                {
                    foreach (var id in data.visitIds)
                    {
                        var OtherVisit = _projectDesignVisitRepository.GetVisit(id);
                        OtherVisit.Templates.AsEnumerable().Where(z => (!data.noOfTemplate.Any() || data.noOfTemplate.Contains(z.Id))).ToList().ForEach(template =>
                        {
                            CloneTemplate(visit, template, checkVersion, designOrder, visitStatus);
                        });
                    }
                }

                _uow.Save();

                if (i == 1) firstSaved = visit;
            }
            return Ok(firstSaved != null ? firstSaved.Id : data.Id);
        }


        private void CloneTemplate(ProjectDesignVisit visit, ProjectDesignTemplate template, CheckVersionDto checkVersion, int designOrder, List<ProjectDesignVisitStatus> visitStatus)
        {
            var temp = _projectDesignTemplateRepository.GetTemplateClone(template.Id);
            var projectDesignTemplate = _mapper.Map<ProjectDesignTemplate>(temp);
            projectDesignTemplate.Id = 0;
            projectDesignTemplate.DesignOrder = ++designOrder;
            projectDesignTemplate.VariableTemplate = null;
            projectDesignTemplate.Domain = null;
            projectDesignTemplate.StudyVersion = checkVersion.VersionNumber;
            projectDesignTemplate.InActiveVersion = null;

            foreach (var variable in projectDesignTemplate.Variables)
            {

                visitStatus.Where(e => e.ProjectDesignVariableId == variable.Id).ToList().ForEach(g =>
                {
                    g.ProjectDesignVariable = variable;
                    g.ProjectDesignVariableId = 0;
                });

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

            foreach (var workflow in projectDesignTemplate.WorkflowTemplate)
            {
                workflow.Id = 0;
                _workflowTemplateRepository.Add(workflow);
            }

            projectDesignTemplate.ProjectDesignVisit = visit;

            _projectDesignTemplateRepository.Add(projectDesignTemplate);

            //For template clone language
            temp.TemplateLanguage.ToList().ForEach(x =>
            {
                x.Id = 0;
                _templateLanguageRepository.Add(x);
            });
        }

        [HttpGet]
        [Route("ChangeVisitDesignOrder/{id}/{index}")]
        public IActionResult ChangeVisitDesignOrder(int id, int index)
        {
            var template = _projectDesignVisitRepository.Find(id);
            if (template != null)
            {

                var PeriodId = template.ProjectDesignPeriodId;

                var orderedList = _projectDesignVisitRepository
                    .FindBy(t => t.ProjectDesignPeriodId == PeriodId && t.DeletedDate == null).OrderBy(t => t.DesignOrder)
                    .ToList();
                orderedList.Remove(orderedList.First(t => t.Id == id));

                if (index != 0)
                    index--;

                orderedList.Insert(index, template);

                var i = 0;
                foreach (var item in orderedList)
                {
                    item.DesignOrder = ++i;
                    _projectDesignVisitRepository.Update(item);
                }

                _uow.Save();
            }

            return Ok();
        }

        [HttpGet]
        [Route("GetVisitsforWorkflowVisit/{projectDesignId}")]
        public IActionResult GetVisitsforWorkflowVisit(int projectDesignId)
        {
            return Ok(_projectDesignVisitRepository.GetVisitsforWorkflowVisit(projectDesignId));
        }

    }
}