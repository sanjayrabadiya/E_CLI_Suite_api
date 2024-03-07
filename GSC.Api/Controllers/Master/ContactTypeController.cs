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
    public class ContactTypeController : BaseController
    {
        private readonly IContactTypeRepository _contactTypeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public ContactTypeController(IContactTypeRepository contactTypeRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _contactTypeRepository = contactTypeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var contacttypes = _contactTypeRepository.GetContactTypeList(isDeleted);
            return Ok(contacttypes);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var contactType = _contactTypeRepository.Find(id);
            var contactTypeDto = _mapper.Map<ContactTypeDto>(contactType);
            return Ok(contactTypeDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ContactTypeDto contactTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            contactTypeDto.Id = 0;
            var contactType = _mapper.Map<ContactType>(contactTypeDto);
            var validate = _contactTypeRepository.Duplicate(contactType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _contactTypeRepository.Add(contactType);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Contact Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(contactType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ContactTypeDto contactTypeDto)
        {
            if (contactTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var contactType = _mapper.Map<ContactType>(contactTypeDto);
            var validate = _contactTypeRepository.Duplicate(contactType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _contactTypeRepository.AddOrUpdate(contactType);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Contact Type failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(contactType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _contactTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _contactTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _contactTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _contactTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _contactTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetContactTypeDropDown")]
        public IActionResult GetContactTypeDropDown()
        {
            return Ok(_contactTypeRepository.GetContactTypeDropDown());
        }
    }
}