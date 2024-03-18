using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Client;
using GSC.Data.Entities.Client;
using GSC.Domain.Context;
using GSC.Respository.Client;
using GSC.Respository.Common;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Client
{
    [Route("api/[controller]")]
    public class ClientAddressController : BaseController
    {
        private readonly IClientAddressRepository _clientAddressRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ClientAddressController(IClientAddressRepository clientAddressRepository,
            IUnitOfWork uow, IMapper mapper,
            ILocationRepository locationRepository)
        {
            _clientAddressRepository = clientAddressRepository;
            _uow = uow;
            _mapper = mapper;
            _locationRepository = locationRepository;
        }


        [HttpGet("{id}/{isDeleted:bool?}")]
        public IActionResult Get(int id, bool isDeleted)
        {
            if (id <= 0) return BadRequest();

            var clientAddresses = _clientAddressRepository.GetAddresses(id, isDeleted);
            return Ok(clientAddresses);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ClientAddressDto clientAddressDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            clientAddressDto.Id = 0;
            var clientAddress = _mapper.Map<ClientAddress>(clientAddressDto);
            clientAddress.Location = _locationRepository.SaveLocation(clientAddress.Location);
            _locationRepository.Add(clientAddress.Location);
            _clientAddressRepository.Add(clientAddress);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating client address failed on save.");
                return BadRequest(ModelState);
            }
            var returnClientAddressDto = _mapper.Map<ClientAddressDto>(clientAddress);
            return CreatedAtAction("Get", new { id = clientAddress.Id }, returnClientAddressDto);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ClientAddressDto clientAddressDto)
        {
            if (clientAddressDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var clientAddress = _mapper.Map<ClientAddress>(clientAddressDto);

            clientAddress.Location = _locationRepository.SaveLocation(clientAddress.Location);

            if (clientAddress.Location.Id > 0)
                _locationRepository.Update(clientAddress.Location);
            else
                _locationRepository.Add(clientAddress.Location);

            /* Added by swati for effective Date on 02-06-2019 */
            _clientAddressRepository.Update(clientAddress);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Client address failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(clientAddress.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _clientAddressRepository.Find(id);

            if (record == null)
                return NotFound();

            _clientAddressRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _clientAddressRepository.Find(id);

            if (record == null)
                return NotFound();
            _clientAddressRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}