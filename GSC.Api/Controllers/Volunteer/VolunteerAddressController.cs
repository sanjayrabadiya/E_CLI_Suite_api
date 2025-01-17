using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Helper;
using GSC.Respository.Audit;
using GSC.Respository.Common;
using GSC.Respository.Volunteer;
using GSC.Shared.Generic;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerAddressController : BaseController
    {
        private readonly IVolunteerAuditTrailRepository _volunteerAuditTrailRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVolunteerAddressRepository _volunteerAddressRepository;

        public VolunteerAddressController(IVolunteerAddressRepository volunteerAddressRepository,
            IUnitOfWork uow, IMapper mapper,
            ILocationRepository locationRepository,
            IVolunteerAuditTrailRepository volunteerAuditTrailRepository)
        {
            _volunteerAddressRepository = volunteerAddressRepository;
            _uow = uow;
            _mapper = mapper;
            _locationRepository = locationRepository;
            _volunteerAuditTrailRepository = volunteerAuditTrailRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var volunteerAddresses = _volunteerAddressRepository.GetAddresses(id);
            return Ok(volunteerAddresses);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VolunteerAddressDto volunteerAddressDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            volunteerAddressDto.Id = 0;
            var volunteerAddress = _mapper.Map<VolunteerAddress>(volunteerAddressDto);
            volunteerAddress.Location = _locationRepository.SaveLocation(volunteerAddress.Location);

            if (volunteerAddress.Location.Id > 0)
                _locationRepository.Update(volunteerAddress.Location);
            else
                _locationRepository.Add(volunteerAddress.Location);

            _volunteerAddressRepository.Add(volunteerAddress);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating volunteer address failed on save."));

            if (volunteerAddressDto.Changes != null)
                _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerAddress, AuditAction.Inserted,
                    volunteerAddress.Id, volunteerAddress.VolunteerId, volunteerAddressDto.Changes);

            return Ok(volunteerAddress.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VolunteerAddressDto volunteerAddressDto)
        {
            if (volunteerAddressDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var volunteerAddress = _mapper.Map<VolunteerAddress>(volunteerAddressDto);
            volunteerAddress.Location = _locationRepository.SaveLocation(volunteerAddress.Location);

            if (volunteerAddress.Location.Id > 0)
                _locationRepository.Update(volunteerAddress.Location);
            else
                _locationRepository.Add(volunteerAddress.Location);

            _volunteerAddressRepository.Update(volunteerAddress);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating volunteer address failed on save."));

            if (volunteerAddressDto.Changes != null)
                _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerAddress, AuditAction.Updated,
                volunteerAddress.Id, volunteerAddress.VolunteerId, volunteerAddressDto.Changes);

            return Ok(volunteerAddress.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _volunteerAddressRepository.Find(id);

            if (record == null)
                return NotFound();

            _volunteerAddressRepository.Delete(record);
            _uow.Save();

            _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerAddress, AuditAction.Deleted,
                record.Id, record.VolunteerId, null);


            return Ok();
        }
    }
}