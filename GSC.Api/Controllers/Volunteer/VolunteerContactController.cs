using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Audit;
using GSC.Respository.Volunteer;
using GSC.Shared;
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

        public VolunteerContactController(IVolunteerContactRepository volunteerContactRepository,
            IUnitOfWork uow, IMapper mapper,
            IAuditTrailRepository auditTrailRepository)
        {
            _volunteerContactRepository = volunteerContactRepository;
            _uow = uow;
            _mapper = mapper;
            _auditTrailRepository = auditTrailRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            return Ok(_volunteerContactRepository.GetContactTypeList(id));
        }

        [HttpPost]
        public IActionResult Post([FromBody] VolunteerContactDto volunteerContactDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            volunteerContactDto.Id = 0;
            var volunteerContact = _mapper.Map<VolunteerContact>(volunteerContactDto);
            _volunteerContactRepository.Add(volunteerContact);
            if (_uow.Save() <= 0) throw new Exception("Creating volunteer contact failed on save.");
            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerContact, AuditAction.Inserted,
                volunteerContact.Id, volunteerContact.VolunteerId, volunteerContactDto.Changes);

            return Ok(volunteerContact.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VolunteerContactDto volunteerContactDto)
        {
            if (volunteerContactDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var volunteerContact = _mapper.Map<VolunteerContact>(volunteerContactDto);
            volunteerContact.Id = volunteerContactDto.Id;
            _volunteerContactRepository.Update(volunteerContact);
            if (_uow.Save() <= 0) throw new Exception("Updating volunteer contact failed on update.");

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerContact, AuditAction.Updated,
                volunteerContact.Id, volunteerContact.VolunteerId, volunteerContactDto.Changes);

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