﻿using System;
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
        private readonly IUnitOfWork<GscContext> _uow;

        public ClientAddressController(IClientAddressRepository clientAddressRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            ILocationRepository locationRepository)
        {
            _clientAddressRepository = clientAddressRepository;
            _uow = uow;
            _mapper = mapper;
            _locationRepository = locationRepository;
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var clientAddresses = _clientAddressRepository.GetAddresses(id);
            return Ok(clientAddresses);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ClientAddressDto clientAddressDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            clientAddressDto.Id = 0;
            var clientAddress = _mapper.Map<ClientAddress>(clientAddressDto);
            clientAddress.Location = _locationRepository.SaveLocation(clientAddress.Location);
            _clientAddressRepository.Add(clientAddress);
            if (_uow.Save() <= 0) throw new Exception("Creating client address failed on save.");
            var returnClientAddressDto = _mapper.Map<ClientAddressDto>(clientAddress);
            return CreatedAtAction("Get", new {id = clientAddress.Id}, returnClientAddressDto);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ClientAddressDto clientAddressDto)
        {
            if (clientAddressDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var clientAddress = _mapper.Map<ClientAddress>(clientAddressDto);
            clientAddress.Location = _locationRepository.SaveLocation(clientAddress.Location);

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(clientAddress.Id);
            clientAddress.Id = 0;
            _clientAddressRepository.Add(clientAddress);

            if (_uow.Save() <= 0) throw new Exception("Updating Client address failed on save.");
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