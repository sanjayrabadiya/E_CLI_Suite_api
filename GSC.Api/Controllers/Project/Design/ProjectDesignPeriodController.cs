using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Mvc;

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

        public ProjectDesignPeriodController(IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IUnitOfWork uow, IMapper mapper,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignVariableValueRepository projectDesignVariableValueRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository)
        {
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectDesignVariableValueRepository = projectDesignVariableValueRepository;
            _uow = uow;
            _mapper = mapper;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
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
            if (_uow.Save() <= 0) throw new Exception("Creating Project Design Period failed on save.");
            return Ok(projectDesignPeriod.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignPeriodDto projectDesignPeriodDto)
        {
            if (projectDesignPeriodDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectDesignPeriod = _mapper.Map<ProjectDesignPeriod>(projectDesignPeriodDto);

            _projectDesignPeriodRepository.Update(projectDesignPeriod);

            if (_uow.Save() <= 0) throw new Exception("Updating Project Design Period failed on save.");
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
        [Route("getPeriodByProjectIdIsLockedDropDown")]
        public IActionResult getPeriodByProjectIdIsLockedDropDown([FromQuery] LockUnlockDDDto lockUnlockDDDto)
        {
            return Ok(_projectDesignPeriodRepository.getPeriodByProjectIdIsLockedDropDown(lockUnlockDDDto));
        }

        [HttpGet]
        [Route("ClonePeriod/{id}/{noOfPeriods}")]
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

                period.VisitList.ToList().ForEach(visit =>
                {
                    var visitStatus = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == visit.Id).ToList();
                    visit.Id = 0;
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
                            });
                            _projectDesignVariableRepository.Add(variable);
                        });
                        _projectDesignTemplateRepository.Add(template);
                    });
                    _projectDesignVisitRepository.Add(visit);

                    visitStatus.ForEach(e =>
                    {
                        e.Id=0;
                        e.ProjectDesignVisitId = 0;
                        e.ProjectDesignVisit = visit;
                        _projectDesignVisitStatusRepository.Add(e);
                    });
                    
                });

                period.DisplayName = "Period " + ++saved;
                _projectDesignPeriodRepository.Add(period);
                if (i == 1) firstSaved = period;
            }

            if (_uow.Save() <= 0) throw new Exception("Creating Project Design Period failed on clone period.");

            return Ok(firstSaved.Id);
        }
    }
}