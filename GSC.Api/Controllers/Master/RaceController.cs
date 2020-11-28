using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class RaceController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IRaceRepository _raceRepository;
        private readonly IUnitOfWork _uow;

        public RaceController(IRaceRepository raceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _raceRepository = raceRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var races = _raceRepository.GetRaceList(isDeleted);
            return Ok(races);
            //var races = _raceRepository.All.Where(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            //).OrderByDescending(x => x.Id).ToList();
            //return Ok(racesDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var race = _raceRepository.Find(id);
            var raceDto = _mapper.Map<RaceDto>(race);
            return Ok(raceDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] RaceDto raceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            raceDto.Id = 0;
            var race = _mapper.Map<Race>(raceDto);
            var validate = _raceRepository.Duplicate(race);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _raceRepository.Add(race);
            if (_uow.Save() <= 0) throw new Exception("Creating Race failed on save.");
            return Ok(race.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] RaceDto raceDto)
        {
            if (raceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var race = _mapper.Map<Race>(raceDto);
            var validate = _raceRepository.Duplicate(race);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _raceRepository.AddOrUpdate(race);

            if (_uow.Save() <= 0) throw new Exception("Updating Race failed on save.");
            return Ok(race.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _raceRepository.Find(id);

            if (record == null)
                return NotFound();

            _raceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _raceRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _raceRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _raceRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetRaceDropDown")]
        public IActionResult GetRaceDropDown()
        {
            return Ok(_raceRepository.GetRaceDropDown());
        }
    }
}