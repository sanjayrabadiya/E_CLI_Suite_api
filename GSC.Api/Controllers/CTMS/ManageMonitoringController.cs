using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ManageMonitoringController : BaseController
    {
        private readonly IManageMonitoringRepository _manageMonitoringRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ManageMonitoringController(IManageMonitoringRepository manageMonitoringRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _manageMonitoringRepository = manageMonitoringRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var manageMonitoringDto = _manageMonitoringRepository.GetMonitoringList(isDeleted);
            return Ok(manageMonitoringDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var manageMonitoring = _manageMonitoringRepository.Find(id);
            var manageMonitoringDto = _mapper.Map<ManageMonitoringDto>(manageMonitoring);
            return Ok(manageMonitoringDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ManageMonitoringDto manageMonitoringDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            manageMonitoringDto.Id = 0;
            var manageMonitoring = _mapper.Map<ManageMonitoring>(manageMonitoringDto);
            _manageMonitoringRepository.Add(manageMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Creating Manage Monitoring failed on save.");

            return Ok(manageMonitoring.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ManageMonitoringDto manageMonitoringDto)
        {
            if (manageMonitoringDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var manageMonitoring = _mapper.Map<ManageMonitoring>(manageMonitoringDto);

            _manageMonitoringRepository.Update(manageMonitoring);
            if (_uow.Save() <= 0) throw new Exception("Updating Manage Monitoring failed on save.");
            return Ok(manageMonitoring.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _manageMonitoringRepository.Find(id);

            if (record == null)
                return NotFound();

            _manageMonitoringRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _manageMonitoringRepository.Find(id);

            if (record == null)
                return NotFound();
            _manageMonitoringRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}