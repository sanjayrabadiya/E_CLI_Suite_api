using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.LanguageSetup;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    public class ProjectDesignPeriodController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IProjectDesignVariableValueRepository _projectDesignVariableValueRepository;
        private readonly IUnitOfWork _uow;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        private readonly IProjectDesignVariableRemarksRepository _projectDesignVariableRemarksRepository;
        private readonly IVisitLanguageRepository _visitLanguageRepository;
        private readonly ITemplateLanguageRepository _templateLanguageRepository;
        private readonly IVariabeLanguageRepository _variabeLanguageRepository;
        private readonly IGSCContext _context;
        private readonly ITemplateNoteLanguageRepository _templateNoteLanguageRepository;
        private readonly IProjectDesignTemplateNoteRepository _projectDesignTemplateNoteRepository;
        public ProjectDesignPeriodController(IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IUnitOfWork uow, IMapper mapper, IGSCContext context,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository,
            IProjectDesignVariableRemarksRepository projectDesignVariableRemarksRepository,
            ITemplateLanguageRepository templateLanguageRepository,
            IVariabeLanguageRepository variabeLanguageRepository,
            ITemplateNoteLanguageRepository templateNoteLanguageRepository,
            IProjectDesignTemplateNoteRepository projectDesignTemplateNoteRepository,
            IVisitLanguageRepository visitLanguageRepository)
        {
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _projectDesignVariableRemarksRepository = projectDesignVariableRemarksRepository;
            _visitLanguageRepository = visitLanguageRepository;
            _templateLanguageRepository = templateLanguageRepository;
            _variabeLanguageRepository = variabeLanguageRepository;
            _templateNoteLanguageRepository = templateNoteLanguageRepository;
            _projectDesignTemplateNoteRepository = projectDesignTemplateNoteRepository;
        }

        [HttpGet("{id}/{projectDesignId}")]
        public IActionResult Get(int id, int projectDesignId)
        {
            if (id <= 0 || projectDesignId <= 0) return BadRequest();
            var projectDesignPeriod = _projectDesignPeriodRepository
                .FindBy(t => t.Id == id && t.ProjectDesignId == projectDesignId).FirstOrDefault();

            if (projectDesignPeriod == null) return NotFound();

            var projectDesignPeriodDto = _mapper.Map<ProjectDesignPeriodDto>(projectDesignPeriod);
            return Ok(projectDesignPeriodDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectDesignPeriodDto projectDesignPeriodDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var projectDesignPeriod = _mapper.Map<ProjectDesignPeriod>(projectDesignPeriodDto);
            _projectDesignPeriodRepository.Add(projectDesignPeriod);
            _uow.Save();
            return Ok(projectDesignPeriod.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignPeriodDto projectDesignPeriodDto)
        {
            if (projectDesignPeriodDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectDesignPeriod = _mapper.Map<ProjectDesignPeriod>(projectDesignPeriodDto);

            _projectDesignPeriodRepository.Update(projectDesignPeriod);

            _uow.Save();
            return Ok(projectDesignPeriod.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            if (id <= 0) return BadRequest();

            var period = _projectDesignPeriodRepository.Find(id);

            if (period == null) return NotFound();
            _projectDesignPeriodRepository.Delete(period);

            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetPeriodDropDown/{projectDesignId}")]
        public IActionResult GetPeriodDropDown(int projectDesignId)
        {
            return Ok(_projectDesignPeriodRepository.GetPeriodDropDown(projectDesignId));
        }

        [HttpGet]
        [Route("GetPeriodByProjectIdDropDown/{projectId}")]
        public IActionResult GetPeriodByProjectIdDropDown(int projectId)
        {
            return Ok(_projectDesignPeriodRepository.GetPeriodByProjectIdDropDown(projectId));
        }

        [HttpGet]
        [Route("ClonePeriod/{id}/{noOfPeriods}")]
        [TransactionRequired]
        public IActionResult ClonePeriod(int id, int noOfPeriods)
        {
            var projectDesignId = _projectDesignPeriodRepository.Find(id).ProjectDesignId;
            var saved = _projectDesignPeriodRepository
                .FindBy(t => t.ProjectDesignId == projectDesignId && t.DeletedDate == null).Count();

            ProjectDesignPeriod firstSaved = null;
            for (var i = 1; i <= noOfPeriods; i++)
            {
                var period = _projectDesignPeriodRepository.GetPeriod(id);

                period.Id = 0;
                period.VisitList.Where(x => x.DeletedDate == null).ToList().ForEach(visit =>
                {
                    var visitStatus = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == visit.Id && x.DeletedDate == null).ToList();
                    var visitLanguages = _visitLanguageRepository.All.Where(x => x.ProjectDesignVisitId == visit.Id && x.DeletedDate == null).ToList();
                    visit.Id = 0;
                    visit.Templates.Where(x => x.DeletedDate == null).ToList().ForEach(template =>
                    {
                        template.Variables.Where(x => x.DeletedDate == null).ToList().ForEach(variable =>
                        {
                            visitStatus.Where(e => e.ProjectDesignVariableId == variable.Id).ToList().ForEach(g =>
                            {
                                g.ProjectDesignVariable = variable;
                                g.ProjectDesignVariableId = 0;
                            });

                            var SeqNo = 0;
                            variable.Values.Where(x => x.DeletedDate == null).ToList().ForEach(value =>
                            {
                                value.SeqNo = ++SeqNo;
                                value.Id = 0;
                                if (value.VariableValueLanguage != null)
                                {
                                    value.VariableValueLanguage.Where(a => a.DeletedDate == null).ToList()
                                    .ForEach(c =>
                                    {
                                        c.Id = 0;
                                        c.ProjectDesignVariableValue = value;
                                        _context.VariableValueLanguage.Add(c);
                                    });
                                }
                                _projectDesignVariableValueRepository.Add(value);
                            });

                            var variableLanguage = _variabeLanguageRepository.All.Where(x => x.DeletedDate == null && x.ProjectDesignVariableId == variable.Id).ToList();
                            if (variableLanguage != null)
                            {
                                variableLanguage.ForEach(variableLang =>
                                {
                                    variableLang.Id = 0;
                                    variableLang.ProjectDesignVariable = variable;
                                    _variabeLanguageRepository.Add(variableLang);
                                });
                            }

                            variable.Id = 0;
                            _projectDesignVariableRepository.Add(variable);
                        });
                        var templateLanguage = _templateLanguageRepository.All.Where(x => x.ProjectDesignTemplateId == template.Id && x.DeletedDate == null).ToList();

                        if (templateLanguage != null)
                        {
                            templateLanguage.ForEach(lang =>
                            {
                                lang.Id = 0;
                                lang.ProjectDesignTemplate = template;
                                _templateLanguageRepository.Add(lang);
                            });
                        }

                        var notes = _projectDesignTemplateNoteRepository.All.Where(x => x.ProjectDesignTemplateId == template.Id && x.DeletedDate == null).Include(x => x.TemplateNoteLanguage).ToList();
                        if (notes != null)
                        {
                            notes.ForEach(note =>
                            {
                                note.ProjectDesignTemplate = template;
                                note.TemplateNoteLanguage.Where(q => q.DeletedDate == null).ToList().ForEach(v =>
                                {
                                    v.Id = 0;
                                    v.ProjectDesignTemplateNote = note;
                                    _templateNoteLanguageRepository.Add(v);
                                });
                                note.Id = 0;
                                _projectDesignTemplateNoteRepository.Add(note);
                            });
                        }

                        template.Id = 0;
                        _projectDesignTemplateRepository.Add(template);
                    });

                    _projectDesignVisitRepository.Add(visit);
                    if (visitLanguages != null)
                    {
                        visitLanguages.ForEach(l =>
                        {
                            l.Id = 0;
                            l.ProjectDesignVisitId = 0;
                            l.ProjectDesignVisit = visit;
                            _visitLanguageRepository.Add(l);
                        });
                    }
                    visitStatus.ForEach(e =>
                    {
                        e.Id = 0;
                        e.ProjectDesignVisitId = 0;
                        e.ProjectDesignVisit = visit;
                        _projectDesignVisitStatusRepository.Add(e);
                    });
                });

                period.DisplayName = "Period " + ++saved;
                _projectDesignPeriodRepository.Add(period);
                if (i == 1) firstSaved = period;
            }

            _uow.Save();

            return Ok(firstSaved.Id);
        }
    }
}