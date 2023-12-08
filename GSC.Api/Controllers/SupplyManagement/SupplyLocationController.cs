using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.Configuration;
using GSC.Respository.SupplyManagement;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    public class SupplyLocationController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly ISupplyLocationRepository _supplyLocationRepository;
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public SupplyLocationController(ISupplyLocationRepository supplyLocationRepository,
            IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser)
        {
            _supplyLocationRepository = supplyLocationRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var locations = _supplyLocationRepository.GetSupplyLocationList(isDeleted);
            return Ok(locations);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var location = _supplyLocationRepository.Find(id);
            var locationDto = _mapper.Map<SupplyLocationDto>(location);
            return Ok(locationDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SupplyLocationDto locationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            locationDto.Id = 0;
            var location = _mapper.Map<SupplyLocation>(locationDto);
            var validate = _supplyLocationRepository.Duplicate(location);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            location.IpAddress = _jwtTokenAccesser.IpAddress;
            location.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _supplyLocationRepository.Add(location);
            if (_uow.Save() <= 0) throw new Exception("Creating Location failed on save.");
            return Ok(location.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SupplyLocationDto locationDto)
        {
            if (locationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var location = _mapper.Map<SupplyLocation>(locationDto);
            var validate = _supplyLocationRepository.Duplicate(location);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            location.IpAddress = _jwtTokenAccesser.IpAddress;
            location.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _supplyLocationRepository.AddOrUpdate(location);

            if (_uow.Save() <= 0) throw new Exception("Updating Location failed on save.");
            return Ok(location.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyLocationRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyLocationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyLocationRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _supplyLocationRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _supplyLocationRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetSupplyLocationDropDown")]
        public IActionResult GetSupplyLocationDropDown()
        {
            return Ok(_supplyLocationRepository.GetSupplyLocationDropDown());
        }
    }
}
