using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Respository.Volunteer;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerBlockHistoryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVolunteerBlockHistoryRepository _volunteerBlockHistoryRepository;
        private readonly IVolunteerRepository _volunteerRepository;

        public VolunteerBlockHistoryController(IVolunteerRepository volunteerRepository,
            IVolunteerBlockHistoryRepository volunteerBlockHistoryRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _volunteerBlockHistoryRepository = volunteerBlockHistoryRepository;
            _uow = uow;
            _mapper = mapper;
            _volunteerRepository = volunteerRepository;
        }


        [HttpGet("{volunteerId}")]
        public IActionResult Get(int volunteerId)
        {
            return Ok(_volunteerBlockHistoryRepository.GetVolunteerBlockHistoryById(volunteerId));
        }


        [HttpPost]
        public IActionResult Post([FromBody] VolunteerBlockHistoryDto volunteerBlockHistoryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            volunteerBlockHistoryDto.Id = 0;
            var volunteerBlockHistory = _mapper.Map<VolunteerBlockHistory>(volunteerBlockHistoryDto);

            _volunteerBlockHistoryRepository.Add(volunteerBlockHistory);
            if (_uow.Save() <= 0) throw new Exception("Creating Volunteer Block History failed on save.");
            var voluntterRecord = _volunteerRepository.Find(volunteerBlockHistoryDto.VolunteerId);
            voluntterRecord.IsBlocked = volunteerBlockHistory.IsBlock;
            _volunteerRepository.Update(voluntterRecord);
            if (_uow.Save() <= 0) throw new Exception("Volunteer Block failed on update.");

            return Ok(volunteerBlockHistory.Id);
        }

        [HttpGet("GetVolunteerBlockedList")]
        public IActionResult GetVolunteerBlockedList()
        {
            var volunteers = _volunteerRepository.Search(new VolunteerSearchDto {IsBlocked = true});
            return Ok(volunteers);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _volunteerBlockHistoryRepository.Find(id);

            if (record == null)
                return NotFound();

            _volunteerBlockHistoryRepository.Delete(record);
            _uow.Save();

            return Ok();
        }
    }
}