﻿using System;
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
        private readonly IUnitOfWork<GscContext> _uow;

        public ProjectDesignPeriodController(IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper)
        {
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _uow = uow;
            _mapper = mapper;
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
        public IActionResult getPeriodByProjectIdIsLockedDropDown([FromQuery]LockUnlockDDDto lockUnlockDDDto)
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

                period.VisitList.ForEach(visit =>
                {
                    visit.Id = 0;
                    visit.Templates.ForEach(template =>
                    {
                        template.Id = 0;
                        template.Variables.ForEach(variable =>
                        {
                            variable.Id = 0;
                            variable.Values.ForEach(value => { value.Id = 0; });
                        });
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