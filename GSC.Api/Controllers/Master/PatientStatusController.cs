using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Helper;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class PatientStatusController : BaseController
    {
        private readonly IPatientStatusRepository _patientStatusRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;


        public PatientStatusController(IPatientStatusRepository patientStatusRepository,
    IUnitOfWork uow,
    IMapper mapper,
    IJwtTokenAccesser jwtTokenAccesser)
        {
            _patientStatusRepository = patientStatusRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {

            var patientStatus = _patientStatusRepository.GetPatientStatusList(isDeleted);
            return Ok(patientStatus);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var patientStatus = _patientStatusRepository.Find(id);
            var patientStatusDto = _mapper.Map<PatientStatusDto>(patientStatus);
            return Ok(patientStatusDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PatientStatusDto patientStatusDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            patientStatusDto.Id = 0;
            var patientStatus = _mapper.Map<PatientStatus>(patientStatusDto);
            var validate = _patientStatusRepository.Duplicate(patientStatus);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _patientStatusRepository.Add(patientStatus);
            if (_uow.Save() <= 0) throw new Exception("Creating Patient Status failed on save.");
            return Ok(patientStatusDto.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PatientStatusDto patientStatusDto)
        {
            if (patientStatusDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var patientStatus = _mapper.Map<PatientStatus>(patientStatusDto);
            var validate = _patientStatusRepository.Duplicate(patientStatus);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by darshil for effective Date on 17-08-2020 */
            _patientStatusRepository.AddOrUpdate(patientStatus);
            
            if (_uow.Save() <= 0) throw new Exception("Updating Patient Status failed on save.");
            return Ok(patientStatus.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _patientStatusRepository.Find(id);

            if (record == null)
                return NotFound();

            _patientStatusRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _patientStatusRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _patientStatusRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _patientStatusRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetPatientStatusDropDown")]
        public IActionResult GetPatientStatusDropDown()
        {
            return Ok(_patientStatusRepository.GetPatientStatusDropDown());
        }
    }
}
