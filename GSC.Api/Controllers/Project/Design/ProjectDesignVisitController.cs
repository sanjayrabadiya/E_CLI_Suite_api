using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.LanguageSetup;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

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
            IProjectDesignVariableEncryptRoleRepository projectDesignVariableEncryptRoleRepository)
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
        }

        [HttpGet("{id}/{projectDesignPeriodId}")]
        public IActionResult Get(int id, int projectDesignPeriodId)
        {
            if (id <= 0 || projectDesignPeriodId <= 0) return BadRequest();
            var projectDesignVisit = _projectDesignVisitRepository
                .FindBy(t => t.Id == id && t.ProjectDesignPeriodId == projectDesignPeriodId).FirstOrDefault();

            if (projectDesignVisit == null) return NotFound();

            var projectDesignVisitDto = _mapper.Map<ProjectDesignVisitDto>(projectDesignVisit);
            //var templates = _projectDesignTemplateRepository.FindBy(t => t.ProjectDesignVisitId == projectDesignVisit.Id && t.DeletedDate == null).ToList();
            //projectDesignVisitDto.Templates = _mapper.Map<List<ProjectDesignTemplateDto>>(templates);

            return Ok(projectDesignVisitDto);
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

            var designOrder = 0;
            if (_projectDesignVisitRepository.All.Any(t => t.ProjectDesignPeriodId == projectDesignVisit.ProjectDesignPeriodId && t.DeletedDate == null))
                designOrder = (int)_projectDesignVisitRepository.All.Where(t => t.ProjectDesignPeriodId == projectDesignVisit.ProjectDesignPeriodId
                && t.DeletedDate == null).Max(t => t.DesignOrder);
            projectDesignVisit.DesignOrder = ++designOrder;
            _projectDesignVisitRepository.Add(projectDesignVisit);
            _uow.Save();

            return Ok(projectDesignVisit.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignVisitDto projectDesignVisitDto)
        {
            if (projectDesignVisitDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectDesignVisit = _mapper.Map<ProjectDesignVisit>(projectDesignVisitDto);

            var validateMessage = _projectDesignVisitRepository.Duplicate(projectDesignVisit);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
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

            var period = _projectDesignVisitRepository.Find(id);

            if (period == null) return NotFound();
            _projectDesignVisitRepository.Delete(period);

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
        [Route("GetVisitsByProjectDesignId/{projectDesignId}")]
        public IActionResult GetVisitsByProjectDesignId(int projectDesignId)
        {
            return Ok(_projectDesignVisitRepository.GetVisitsByProjectDesignId(projectDesignId));
        }

        [HttpGet]
        [Route("CloneVisit/{id}/{projectDesignPeriodId}/{noOfVisits}")]
        [TransactionRequired]
        public IActionResult CloneVisit(int id, int projectDesignPeriodId, int noOfVisits)
        {
            var saved = _projectDesignVisitRepository
                .FindBy(t => t.ProjectDesignPeriodId == projectDesignPeriodId && t.DeletedDate == null).Count();

            ProjectDesignVisit firstSaved = null;

            var designOrder = 0;
            if (_projectDesignVisitRepository.All.Any(t => t.ProjectDesignPeriodId == projectDesignPeriodId && t.DeletedDate == null))
                designOrder = (int)_projectDesignVisitRepository.All.Where(t => t.ProjectDesignPeriodId == projectDesignPeriodId
                && t.DeletedDate == null).Max(t => t.DesignOrder);

            for (var i = 1; i <= noOfVisits; i++)
            {
                var visitStatus = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == id).ToList();

                var visit = _projectDesignVisitRepository.GetVisit(id);
                visit.Id = 0;
                visit.ProjectDesignPeriodId = projectDesignPeriodId;
                visit.DesignOrder = ++designOrder;
                visit.Templates.ToList().ForEach(template =>
                {
                    template.Id = 0;
                    template.Variables.ToList().ForEach(variable =>
                    {
                        visitStatus.Where(e => e.ProjectDesignVariableId == variable.Id).ToList().ForEach(g =>
                        {
                            g.ProjectDesignVariable = variable;
                            g.ProjectDesignVariableId = 0;
                        });

                        variable.Id = 0;
                        variable.Values.ToList().ForEach(value =>
                        {
                            value.Id = 0;
                            _projectDesignVariableValueRepository.Add(value);

                            //For variable value clone language
                            value.VariableValueLanguage.ToList().ForEach(x =>
                            {
                                x.Id = 0;
                                _variableValueLanguageRepository.Add(x);
                            });

                        });
                        _projectDesignVariableRepository.Add(variable);

                        //For variable clone language
                        variable.VariableLanguage.ToList().ForEach(r =>
                        {
                            r.Id = 0;
                            _variableLanguageRepository.Add(r);
                        });

                        // For encrypt clone
                        variable.Roles.ToList().ForEach(r =>
                        {
                            r.Id = 0;
                            _projectDesignVariableEncryptRoleRepository.Add(r);
                        });

                        //For variable note clone language
                        variable.VariableNoteLanguage.ToList().ForEach(r =>
                        {
                            r.Id = 0;
                            _variableNoteLanguageRepository.Add(r);
                        });

                    });


                    template.ProjectDesignTemplateNote.ToList().ForEach(templateNote =>
                    {
                        templateNote.Id = 0;
                        _projectDesignTemplateNoteRepository.Add(templateNote);

                        //For template note clone language
                        templateNote.TemplateNoteLanguage.ToList().ForEach(x =>
                        {
                            x.Id = 0;
                            _templateNoteLanguageRepository.Add(x);
                        });
                    });

                    _projectDesignTemplateRepository.Add(template);

                    //For template clone language
                    template.TemplateLanguage.ToList().ForEach(x =>
                    {
                        x.Id = 0;
                        _templateLanguageRepository.Add(x);
                    });

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

                if (i == 1) firstSaved = visit;
            }

            _uow.Save();

            return Ok(firstSaved != null ? firstSaved.Id : id);
        }

        [HttpGet]
        [Route("ChangeVisitDesignOrder/{id}/{index}")]
        public IActionResult ChangeVisitDesignOrder(int id, int index)
        {
            var template = _projectDesignVisitRepository.Find(id);
            var PeriodId = template.ProjectDesignPeriodId;

            var orderedList = _projectDesignVisitRepository
                .FindBy(t => t.ProjectDesignPeriodId == PeriodId && t.DeletedDate == null).OrderBy(t => t.DesignOrder)
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
                _projectDesignVisitRepository.Update(item);
            }

            _uow.Save();

            return Ok();
        }

    }
}