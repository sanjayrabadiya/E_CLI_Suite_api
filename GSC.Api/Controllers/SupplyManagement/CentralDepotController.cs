using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class CentralDepotController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ICentralDepotRepository _centralDepotRepository;
        private readonly IUnitOfWork _uow;

        public CentralDepotController(ICentralDepotRepository centralDepotRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _centralDepotRepository = centralDepotRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var productTypes = _centralDepotRepository.GetCentralDepotList(isDeleted);
            return Ok(productTypes);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _centralDepotRepository.Find(id);
            var centralDepoDto = _mapper.Map<CentralDepotDto>(centralDepo);
            return Ok(centralDepoDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] CentralDepotDto centralDepotDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            centralDepotDto.Id = 0;
            var centralDepot = _mapper.Map<CentralDepot>(centralDepotDto);
           
            _centralDepotRepository.Add(centralDepot);
            if (_uow.Save() <= 0) throw new Exception("Creating central depot failed on save.");
            return Ok(centralDepot.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] CentralDepotDto centralDepotDto)
        {
            if (centralDepotDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var centralDepot = _mapper.Map<CentralDepot>(centralDepotDto);

            _centralDepotRepository.AddOrUpdate(centralDepot);

            if (_uow.Save() <= 0) throw new Exception("Updating central depot failed on save.");
            return Ok(centralDepot.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _centralDepotRepository.Find(id);

            if (record == null)
                return NotFound();

            _centralDepotRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _centralDepotRepository.Find(id);

            if (record == null)
                return NotFound();

            _centralDepotRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetCentralDepotDropDown")]
        public IActionResult GetCentralDepotDropDown()
        {
            return Ok(_centralDepotRepository.GetCentralDepotDropDown());
        }
    }
}
