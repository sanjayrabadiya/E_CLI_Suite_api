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
    public class ProjectDesignVisitController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IUnitOfWork _uow;

        public ProjectDesignVisitController(IProjectDesignVisitRepository projectDesignVisitRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _uow = uow;
            _mapper = mapper;
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

            _projectDesignVisitRepository.Add(projectDesignVisit);
            if (_uow.Save() <= 0) throw new Exception("Creating Project Design Visit failed on save.");

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

            if (_uow.Save() <= 0) throw new Exception("Updating Project Design Visit failed on save.");
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
        [Route("GetVisitByLockedDropDown")]
        public IActionResult GetVisitByLockedDropDown([FromQuery]LockUnlockDDDto lockUnlockDDDto)
        {
            return Ok(_projectDesignVisitRepository.GetVisitByLockedDropDown(lockUnlockDDDto));
        }

        [HttpGet]
        [Route("GetVisitsByProjectDesignId/{projectDesignId}")]
        public IActionResult GetVisitsByProjectDesignId(int projectDesignId)
        {
            return Ok(_projectDesignVisitRepository.GetVisitsByProjectDesignId(projectDesignId));
        }

        [HttpGet]
        [Route("CloneVisit/{id}/{projectDesignPeriodId}/{noOfVisits}")]
        public IActionResult CloneVisit(int id, int projectDesignPeriodId, int noOfVisits)
        {
            var saved = _projectDesignVisitRepository
                .FindBy(t => t.ProjectDesignPeriodId == projectDesignPeriodId && t.DeletedDate == null).Count();

            ProjectDesignVisit firstSaved = null;
            for (var i = 1; i <= noOfVisits; i++)
            {
                var visit = _projectDesignVisitRepository.GetVisit(id);
                visit.Id = 0;
                visit.ProjectDesignPeriodId = projectDesignPeriodId;
                visit.Templates.ForEach(template =>
                {
                    template.Id = 0;
                    template.Variables.ForEach(variable =>
                    {
                        variable.Id = 0;
                        variable.Values.ForEach(value => { value.Id = 0; });
                    });
                });

                visit.DisplayName = "Visit " + ++saved;
                _projectDesignVisitRepository.Add(visit);
                if (i == 1) firstSaved = visit;
            }

            if (_uow.Save() <= 0) throw new Exception("Creating Project Design Visit failed on clone period.");

            return Ok(firstSaved != null ? firstSaved.Id : id);
        }
    }
}