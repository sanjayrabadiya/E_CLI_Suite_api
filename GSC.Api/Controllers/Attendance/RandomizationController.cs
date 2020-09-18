using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Attendance
{
    [Route("api/[controller]")]
    public class RandomizationController : BaseController
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;

        public RandomizationController(IRandomizationRepository randomizationRepository,
            IUnitOfWork uow, IMapper mapper,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IAttendanceRepository attendanceRepository,
            ICityRepository cityRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository)
        {
            _randomizationRepository = randomizationRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _attendanceRepository = attendanceRepository;
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _countryRepository = countryRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var randomizations = _randomizationRepository.All.Where(x =>
             (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
             && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
         ).OrderByDescending(t => t.Id).ToList();

            var RandomizationDto = _mapper.Map<IEnumerable<RandomizationDto>>(randomizations);

            return Ok(RandomizationDto);
        }

        [HttpGet("GetRandomizationList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetRandomizationList(int projectId, bool isDeleted)
        {
            return Ok(_randomizationRepository.GetRandomizationList(projectId, isDeleted));
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var randomization = _randomizationRepository.FindByInclude(x => x.Id == id, x => x.City, x => x.City.State, x => x.City.State.Country)
                .SingleOrDefault();
            if (randomization == null)
                return BadRequest();

            var randomizationDto = _mapper.Map<RandomizationDto>(randomization);

            return Ok(randomizationDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] RandomizationDto randomizationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var randomization = _mapper.Map<Randomization>(randomizationDto);
            //Create By Vipul Attendance not save while subject profile save modify on 15092020
            _randomizationRepository.Add(randomization);
            if (_uow.Save() <= 0) throw new Exception("Creating randomization failed on save.");
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] RandomizationDto RandomizationDto)
        {
            if (RandomizationDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            RandomizationDto.Initial = RandomizationDto.Initial.PadRight(3, '-');
            var randomization = _mapper.Map<Randomization>(RandomizationDto);
            _randomizationRepository.Update(randomization);
            if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");
            return Ok(randomization.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _randomizationRepository
                .FindByInclude(x => x.Id == id && x.DeletedDate == null, x => x.Attendance).FirstOrDefault();

            if (record == null)
                return NotFound();

            if (record.Attendance.IsProcessed)
            {
                ModelState.AddModelError("Message", "Can not delete , because this record in under process.");
                return BadRequest(ModelState);
            }

            _attendanceRepository.Delete(record.Attendance);
            _randomizationRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _randomizationRepository
                .FindByInclude(x => x.Id == id, x => x.Attendance).FirstOrDefault();

            if (record == null)
                return NotFound();
            _randomizationRepository.Active(record);
            _attendanceRepository.Active(record.Attendance);
            _uow.Save();
            return Ok();
        }

        [HttpPut]
        [Route("SaveRandomization")]
        public IActionResult SaveRandomization([FromBody] RandomizationDto randomizationDto)
        {
            if (randomizationDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            //Change by vipul on 15092020 for Attendance add only when screening number and randomization number add
            var projectDesignPeriod = _projectDesignPeriodRepository.FindBy(x => x.DeletedDate == null && x.ProjectDesign.DeletedDate == null &&
                                    x.ProjectDesign.ProjectId == randomizationDto.ParentProjectId).FirstOrDefault();

            if (projectDesignPeriod == null)
            {
                ModelState.AddModelError("Message", "Design is not complete");
                return BadRequest(ModelState);
            }

            randomizationDto.ProjectDesignPeriodId = projectDesignPeriod.Id;
            var randomization = _randomizationRepository.Find(randomizationDto.Id);

            var validate = _randomizationRepository.Duplicate(randomization, randomizationDto.ProjectId);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _randomizationRepository.SaveRandomization(randomization, randomizationDto);

            _randomizationRepository.Update(randomization);
            if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");
            return Ok(randomization.Id);
        }
    }
}