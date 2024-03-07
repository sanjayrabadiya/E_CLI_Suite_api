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
    public class ClientTypeController : BaseController
    {
        private readonly IClientTypeRepository _clientTypeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ClientTypeController(IClientTypeRepository clientTypeRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _clientTypeRepository = clientTypeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var clientTypes = _clientTypeRepository.GetClientTypeList(isDeleted);
            return Ok(clientTypes);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var clientTypes = _clientTypeRepository.Find(id);
            var clientTypesDto = _mapper.Map<ClientTypeDto>(clientTypes);
            return Ok(clientTypesDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ClientTypeDto clientTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            clientTypeDto.Id = 0;
            var clientType = _mapper.Map<ClientType>(clientTypeDto);
            var validate = _clientTypeRepository.Duplicate(clientType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _clientTypeRepository.Add(clientType);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Client Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(clientType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ClientTypeDto clientTypeDto)
        {
            if (clientTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var clientType = _mapper.Map<ClientType>(clientTypeDto);
            var validate = _clientTypeRepository.Duplicate(clientType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _clientTypeRepository.AddOrUpdate(clientType);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Client Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(clientType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _clientTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _clientTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _clientTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _clientTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _clientTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetClientTypeDropDown")]
        public IActionResult GetClientTypeDropDown()
        {
            return Ok(_clientTypeRepository.GetClientTypeDropDown());
        }
    }
}