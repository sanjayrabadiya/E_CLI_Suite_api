using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Client;
using GSC.Data.Entities.Client;
using GSC.Domain.Context;
using GSC.Respository.Client;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Client
{
    [Route("api/[controller]")]
    public class ClientContactController : BaseController
    {
        private readonly IClientContactRepository _clientContactRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public ClientContactController(IClientContactRepository clientContactRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper)
        {
            _clientContactRepository = clientContactRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var clientContacts = _clientContactRepository.GetContactList(id);
            return Ok(clientContacts);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ClientContactDto clientContactDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            clientContactDto.Id = 0;
            var clientContact = _mapper.Map<ClientContact>(clientContactDto);

            var validate = _clientContactRepository.DuplicateContact(clientContact);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }


            _clientContactRepository.Add(clientContact);
            if (_uow.Save() <= 0) throw new Exception("Creating client contact failed on save.");
            var returnClientContactDto = _mapper.Map<ClientContactDto>(clientContact);
            return CreatedAtAction("Get", new {id = clientContact.Id}, returnClientContactDto);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] ClientContactDto clientContactDto)
        {
            if (clientContactDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var clientContact = _mapper.Map<ClientContact>(clientContactDto);

            var validate = _clientContactRepository.DuplicateContact(clientContact);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            clientContact.Id = clientContactDto.Id;
            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(clientContact.Id);
            clientContact.Id = 0;
            _clientContactRepository.Add(clientContact);

            if (_uow.Save() <= 0) throw new Exception("Updating client contact failed on save.");
            return Ok(clientContact.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _clientContactRepository.Find(id);

            if (record == null)
                return NotFound();

            _clientContactRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _clientContactRepository.Find(id);

            if (record == null)
                return NotFound();
            _clientContactRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}