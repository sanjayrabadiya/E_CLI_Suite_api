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
using GSC.Shared.Generic;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerLanguageController : BaseController
    {
        private readonly IAuditTrailRepository _auditTrailRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVolunteerLanguageRepository _volunteerLanguageRepository;

        public VolunteerLanguageController(IVolunteerLanguageRepository volunteerLanguageRepository,
            IUnitOfWork uow, IMapper mapper,
            IAuditTrailRepository auditTrailRepository
        )
        {
            _volunteerLanguageRepository = volunteerLanguageRepository;
            _uow = uow;
            _mapper = mapper;
            _auditTrailRepository = auditTrailRepository;
        }

        [HttpGet("{id}/{isDeleted:bool?}")]
        public IActionResult Get(int id, bool isDeleted)
        {
            if (id <= 0) return BadRequest();
            return Ok(_volunteerLanguageRepository.GetLanguages(id, isDeleted));
        }


        [HttpPost]
        public IActionResult Post([FromBody] VolunteerLanguageDto volunteerLanguageDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _volunteerLanguageRepository.RemoveExisting(0, volunteerLanguageDto.VolunteerId,
                volunteerLanguageDto.LanguageId);

            volunteerLanguageDto.Id = 0;
            var volunteerLanguage = _mapper.Map<VolunteerLanguage>(volunteerLanguageDto);
            _volunteerLanguageRepository.Add(volunteerLanguage);
            if (_uow.Save() <= 0) throw new Exception("Creating volunteer language failed on save.");

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerLanguage, AuditAction.Inserted,
                volunteerLanguage.Id, volunteerLanguage.VolunteerId, volunteerLanguageDto.Changes);

            return Ok(volunteerLanguage.Id);
        }

        [HttpPut]
        public IActionResult Put(int id, [FromBody] VolunteerLanguageDto volunteerLanguageDto)
        {
            if (volunteerLanguageDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _volunteerLanguageRepository.RemoveExisting(volunteerLanguageDto.Id, volunteerLanguageDto.VolunteerId,
                volunteerLanguageDto.LanguageId);

            var volunteerLanguage = _mapper.Map<VolunteerLanguage>(volunteerLanguageDto);
            //volunteerLanguage.Id = id;
            _volunteerLanguageRepository.Update(volunteerLanguage);
            if (_uow.Save() <= 0) throw new Exception("Updating volunteer language failed on save.");

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerLanguage, AuditAction.Updated,
                volunteerLanguageDto.Id, volunteerLanguage.VolunteerId, volunteerLanguageDto.Changes);

            return Ok(volunteerLanguage.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _volunteerLanguageRepository.Find(id);

            if (record == null)
                return NotFound();

            _volunteerLanguageRepository.Delete(record);
            _uow.Save();

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerLanguage, AuditAction.Deleted,
                record.Id, record.VolunteerId, null);

            return Ok();
        }
    }
}