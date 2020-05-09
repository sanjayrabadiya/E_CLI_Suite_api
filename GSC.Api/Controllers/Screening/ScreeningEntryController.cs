using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Common;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.Volunteer;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningEntryController : BaseController
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;
        private readonly IProjectDesignPeriodRepository _projectDesignPeriodRepository;
        private readonly IScreeningEntryRepository _screeningEntryRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IUserRecentItemRepository _userRecentItemRepository;
        private readonly IScreeningProgress _screeningProgress;
        public ScreeningEntryController(IScreeningEntryRepository screeningEntryRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IUserRecentItemRepository userRecentItemRepository,
            IVolunteerRepository volunteerRepository,
            IAttendanceRepository attendanceRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IScreeningProgress screeningProgress)
        {
            _screeningEntryRepository = screeningEntryRepository;
            _uow = uow;
            _mapper = mapper;
            _userRecentItemRepository = userRecentItemRepository;
            _attendanceRepository = attendanceRepository;
            _screeningProgress = screeningProgress;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var screeningEntryDto = _screeningEntryRepository.GetDetails(id);
            _userRecentItemRepository.SaveUserRecentItem(new UserRecentItem
            {
                KeyId = screeningEntryDto.Id,
                SubjectName = screeningEntryDto.ScreeningNo,
                SubjectName1 = screeningEntryDto.VolunteerName,
                ScreenType = UserRecent.Project
            });

            return Ok(screeningEntryDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ScreeningEntryDto screeningEntryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var screeningEntry = _mapper.Map<ScreeningEntry>(screeningEntryDto);

            _screeningEntryRepository.SaveScreening(screeningEntry, screeningEntryDto.ProjectAttendanceTemplateIds);

            if (_uow.Save() <= 0) throw new Exception("Creating Screening Entry failed on save.");

            return Ok(screeningEntry.Id);
        }

        [HttpPost("SaveByAttendanceId/{id}")]
        public IActionResult SaveByAttendanceId(int id)
        {
            var attendance = _attendanceRepository.Find(id);
            var screeningEntry = new ScreeningEntry();
            screeningEntry.AttendanceId = attendance.Id;
            screeningEntry.ProjectDesignId =
                _projectDesignPeriodRepository.Find(attendance.ProjectDesignPeriodId).ProjectDesignId;
            screeningEntry.ProjectDesignPeriodId = attendance.ProjectDesignPeriodId;
            screeningEntry.ProjectId = attendance.ProjectId;
            screeningEntry.ScreeningDate = DateTime.Now;

            _screeningEntryRepository.SaveScreening(screeningEntry, null);

            if (_uow.Save() <= 0) throw new Exception("Creating Screening Entry failed on save.");

            return Ok(screeningEntry.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ScreeningEntryDto screeningEntryDto)
        {
            if (screeningEntryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var temp = _screeningEntryRepository.FindBy(t => t.Id == screeningEntryDto.Id).FirstOrDefault();

            var screeningEntry = _mapper.Map<ScreeningEntry>(screeningEntryDto);
            screeningEntry.ProjectId = temp.ProjectId;

            _screeningEntryRepository.Update(screeningEntry);

            if (_uow.Save() <= 0) throw new Exception("Creating Screening Entry failed on save.");

            return Ok(screeningEntry.Id);
        }

        [HttpGet]
        [Route("Progress/{screeningEntryId}/{screeningTemplateId}")]
        public IActionResult Progress(int screeningEntryId, int screeningTemplateId)
        {
            var progress = _screeningProgress.GetScreeningProgress(screeningEntryId, screeningTemplateId);

            return Ok(progress);
        }

        [HttpGet("AutoCompleteSearch")]
        public IActionResult AutoCompleteSearch(string searchText)
        {
            var result = _screeningEntryRepository.AutoCompleteSearch(searchText);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetScreeningList")]
        public IActionResult GetScreeningList([FromBody] ScreeningSearhParamDto searchParam)
        {
            var result = _screeningEntryRepository.GetScreeningList(searchParam);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetAuditHistory/{id}")]
        public IActionResult GetAuditHistory(int id)
        {
            var auditHistory = _screeningEntryRepository.GetAuditHistory(id);

            return Ok(auditHistory);
        }

        [HttpGet("Summary/{id}")]
        public IActionResult Summary(int id)
        {
            if (id <= 0) return BadRequest();

            var screeningSummaryDto = _screeningEntryRepository.GetSummary(id);

            return Ok(screeningSummaryDto);
        }

        [HttpGet("GetProjectStatusAndLevelDropDown/{parentProjectId}")]
        public IActionResult GetProjectStatusAndLevelDropDown(int parentProjectId)
        {
            var screeningSummaryDto = _screeningEntryRepository.GetProjectStatusAndLevelDropDown(parentProjectId);

            return Ok(screeningSummaryDto);
        }
    }
}