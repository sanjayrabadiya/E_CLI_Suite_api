using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.Volunteer;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Audit;
using GSC.Respository.Common;
using GSC.Respository.Screening;
using GSC.Shared.Generic;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningHistoryController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IScreeningHistoryRepository _screeningHistoryRepository;
        private readonly IVolunteerAuditTrailRepository _volunteerAuditTrailRepository;
        private readonly IUnitOfWork _uow;

        public ScreeningHistoryController(IScreeningHistoryRepository screeningHistoryRepository,
            IUnitOfWork uow, IMapper mapper,
            IVolunteerAuditTrailRepository volunteerAuditTrailRepository,
            IUserRecentItemRepository userRecentItemRepository)
        {
            _screeningHistoryRepository = screeningHistoryRepository;
            _volunteerAuditTrailRepository = volunteerAuditTrailRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{volunteerId}/{lastDay}")]
        public IActionResult Get(int volunteerId, int lastDay)
        {
            if (volunteerId <= 0) return BadRequest();

            return Ok(_screeningHistoryRepository.GetScreeningHistoryByVolunteerId(volunteerId, lastDay));
        }

        [HttpPut]
        public IActionResult Put([FromBody] ScreeningHistoryDto screeningHistoryDto)
        {
            if (screeningHistoryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var screeningHistory = _mapper.Map<ScreeningHistory>(screeningHistoryDto);
            screeningHistory.Id = screeningHistoryDto.Id;
            _screeningHistoryRepository.Update(screeningHistory);
            if (_uow.Save() <= 0) throw new Exception("Updating screening history failed on update.");

            _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.VolunteerScreening, AuditAction.Inserted,
               screeningHistory.Id, screeningHistoryDto.VolunteerId, screeningHistoryDto.Changes);

            return Ok(screeningHistory.Id);
        }
    }
}