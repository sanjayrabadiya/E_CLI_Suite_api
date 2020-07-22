using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Respository.Volunteer;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerBiometricController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVolunteerBiometricRepository _volunteerBiometricRepository;

        public VolunteerBiometricController(IVolunteerBiometricRepository volunteerBiometricRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _volunteerBiometricRepository = volunteerBiometricRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var volunteerBiometrics = _volunteerBiometricRepository.All.ToList();
            var volunteerBiometricsDto = _mapper.Map<IEnumerable<VolunteerBiometricDto>>(volunteerBiometrics);
            return Ok(volunteerBiometricsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var volunteerBiometric = _volunteerBiometricRepository.Find(id);
            var volunteerBiometricDto = _mapper.Map<VolunteerBiometricDto>(volunteerBiometric);
            return Ok(volunteerBiometricDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] VolunteerBiometricDto volunteerBiometricDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            volunteerBiometricDto.Id = 0;
            var volunteerBiometric = _mapper.Map<VolunteerBiometric>(volunteerBiometricDto);

            _volunteerBiometricRepository.Add(volunteerBiometric);
            if (_uow.Save() <= 0) throw new Exception("Creating volunteer biometric failed on save.");
            return Ok(volunteerBiometric.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] VolunteerBiometricDto volunteerBiometricDto)
        {
            if (volunteerBiometricDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var volunteerBiometric = _mapper.Map<VolunteerBiometric>(volunteerBiometricDto);
            volunteerBiometric.Id = volunteerBiometricDto.Id;
            _volunteerBiometricRepository.Update(volunteerBiometric);
            if (_uow.Save() <= 0) throw new Exception("Updating volunteer biometric failed on save.");
            return Ok(volunteerBiometric.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _volunteerBiometricRepository.Find(id);

            if (record == null)
                return NotFound();

            _volunteerBiometricRepository.Delete(record);
            _uow.Save();

            return Ok();
        }
    }
}