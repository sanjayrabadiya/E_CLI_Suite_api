using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Audit;
using GSC.Respository.Common;
using GSC.Respository.Volunteer;
using GSC.Shared.Generic;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerContactController : BaseController
    {
        private readonly IAuditTrailRepository _auditTrailRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVolunteerContactRepository _volunteerContactRepository;
        private readonly ILocationRepository _locationRepository;

        public VolunteerContactController(IVolunteerContactRepository volunteerContactRepository,
            IUnitOfWork uow, IMapper mapper,
            IAuditTrailRepository auditTrailRepository,
            ILocationRepository locationRepository)
        {
            _volunteerContactRepository = volunteerContactRepository;
            _uow = uow;
            _mapper = mapper;
            _auditTrailRepository = auditTrailRepository;
            _locationRepository = locationRepository;
        }

        [HttpGet("{id}/{isDeleted:bool?}")]
        public IActionResult Get(int id, bool isDeleted)
        {
            //if (id <= 0) return BadRequest();

            //return Ok(_volunteerContactRepository.GetContacts(id));
            if (id <= 0) return BadRequest();

            return Ok(_volunteerContactRepository.GetContactTypeList(id));
        }

        [HttpPost]
        public IActionResult Post([FromBody] VolunteerContactDto volunteerContactDto)
        {
            //if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            //volunteerContactDto.Id = 0;
            //var volunteerContact = _mapper.Map<VolunteerContact>(volunteerContactDto);
            //volunteerContact.Location = _locationRepository.SaveLocation(volunteerContact.Location);
            //_locationRepository.Add(volunteerContact.Location);
            //_volunteerContactRepository.Add(volunteerContact);
            //if (_uow.Save() <= 0) throw new Exception("Creating Volunteer Contact failed on save.");
            //var returnClientAddressDto = _mapper.Map<VolunteerContactDto>(volunteerContact);
            //return CreatedAtAction("Get", new { id = volunteerContact.Id }, returnClientAddressDto);


            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            volunteerContactDto.Id = 0;
            var volunteerContact = _mapper.Map<VolunteerContact>(volunteerContactDto);
            _volunteerContactRepository.Add(volunteerContact);
            if (_uow.Save() <= 0) throw new Exception("Creating volunteer contact failed on save.");
            //_auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerContact, AuditAction.Inserted,
            //    volunteerContact.Id, volunteerContact.VolunteerId, volunteerContactDto.Changes);

            return Ok(volunteerContact.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VolunteerContactDto volunteerContactDto)
        {

            //if (volunteerContactDto.Id <= 0) return BadRequest();

            //if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //var voluteerContact = _mapper.Map<VolunteerContact>(volunteerContactDto);

            //voluteerContact.Location = _locationRepository.SaveLocation(voluteerContact.Location);

            //if (voluteerContact.Location.Id > 0)
            //    _locationRepository.Update(voluteerContact.Location);
            //else
            //    _locationRepository.Add(voluteerContact.Location);

            //_volunteerContactRepository.Update(voluteerContact);

            //if (_uow.Save() <= 0) throw new Exception("Updating Client address failed on save.");
            //return Ok(voluteerContact.Id);


            //if (volunteerContactDto.Id <= 0) return BadRequest();

            //if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //var voluteerContact = _mapper.Map<VolunteerContact>(volunteerContactDto);

            ////var validate = _volunteerContactRepository.DuplicateContact(voluteerContact);
            ////if (!string.IsNullOrEmpty(validate))
            ////{
            ////    ModelState.AddModelError("Message", validate);
            ////    return BadRequest(ModelState);
            ////}

            //voluteerContact.Id = volunteerContactDto.Id;

            ///* Added by swati for effective Date on 02-06-2019 */
            //_volunteerContactRepository.AddOrUpdate(voluteerContact);

            //if (_uow.Save() <= 0) throw new Exception("Updating client contact failed on save.");
            //return Ok(voluteerContact.Id);



            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var volunteerContact = _mapper.Map<VolunteerContact>(volunteerContactDto);
            volunteerContact.Id = volunteerContactDto.Id;
            _volunteerContactRepository.Update(volunteerContact);
            if (_uow.Save() <= 0) throw new Exception("Updating volunteer contact failed on update.");

            //_auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerContact, AuditAction.Updated,
            //    volunteerContact.Id, volunteerContact.VolunteerId, volunteerContactDto.Changes);

            return Ok(volunteerContact.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _volunteerContactRepository.Find(id);

            if (record == null)
                return NotFound();

            _volunteerContactRepository.Delete(record);
            _uow.Save();

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerContact, AuditAction.Deleted,
                record.Id, record.VolunteerId, null);

            return Ok();
        }
    }
}