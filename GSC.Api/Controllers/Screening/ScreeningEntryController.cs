using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Respository.Attendance;
using GSC.Respository.Common;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.Volunteer;
using GSC.Shared.JWTAuth;
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
        private readonly IUnitOfWork _uow;
        private readonly IScreeningProgress _screeningProgress;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IScreeningHistoryRepository _screeningHistoryRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ScreeningEntryController(IScreeningEntryRepository screeningEntryRepository,
            IUnitOfWork uow, IMapper mapper,
            IVolunteerRepository volunteerRepository,
            IAttendanceRepository attendanceRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IScreeningProgress screeningProgress,
            IScreeningVisitRepository screeningVisitRepository,
            IScreeningHistoryRepository screeningHistoryRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _screeningEntryRepository = screeningEntryRepository;
            _uow = uow;
            _mapper = mapper;
            _attendanceRepository = attendanceRepository;
            _screeningProgress = screeningProgress;
            _projectDesignPeriodRepository = projectDesignPeriodRepository;
            _screeningVisitRepository = screeningVisitRepository;
            _screeningHistoryRepository = screeningHistoryRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var screeningEntryDto = _screeningEntryRepository.GetDetails(id);
            //_userRecentItemRepository.SaveUserRecentItem(new UserRecentItem
            //{
            //    KeyId = screeningEntryDto.Id,
            //    SubjectName = screeningEntryDto.ScreeningNo,
            //    SubjectName1 = screeningEntryDto.VolunteerName,
            //    ScreenType = UserRecent.Project
            //});

            return Ok(screeningEntryDto);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ScreeningEntryDto screeningEntryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (screeningEntryDto.AttendanceId == 0 || screeningEntryDto.AttendanceId == null)
                throw new Exception("Not found Attendance");

            screeningEntryDto.ScreeningDate=screeningEntryDto.ScreeningDate==null ? DateTime.Now : screeningEntryDto.ScreeningDate;

            var screeningEntry = _mapper.Map<ScreeningEntry>(screeningEntryDto);
            _screeningEntryRepository.SaveScreeningAttendance(screeningEntry, screeningEntryDto.ProjectAttendanceTemplateIds);

            _uow.Save();

            screeningEntry.ScreeningHistory.ScreeningEntryId = screeningEntry.Id;
            _screeningHistoryRepository.Add(screeningEntry.ScreeningHistory);
            _uow.Save();

            return Ok(screeningEntry.Id);
        }

        [HttpPost]
        [TransactionRequired]
        [Route("SaveScreeningRandomization")]
        public IActionResult SaveScreeningRandomization([FromBody] SaveRandomizationDto saveRandomizationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var result = _screeningEntryRepository.SaveScreeningRandomization(saveRandomizationDto);
            _uow.Save();
            if (result.Id <= 0) throw new Exception("Creating Screening Entry failed on save.");

            return Ok(result.Id);
        }

        [HttpPost("SaveByAttendanceId/{id}")]
        [TransactionRequired]
        public IActionResult SaveByAttendanceId(int id)
        {
            var attendance = _attendanceRepository.Find(id);
            var screeningEntry = new ScreeningEntry();
            screeningEntry.AttendanceId = attendance.Id;
            screeningEntry.ProjectDesignId =
                _projectDesignPeriodRepository.Find(attendance.ProjectDesignPeriodId).ProjectDesignId;
            screeningEntry.ProjectDesignPeriodId = attendance.ProjectDesignPeriodId;
            screeningEntry.ProjectId = attendance.ProjectId;
            screeningEntry.ScreeningDate = _jwtTokenAccesser.GetClientDate();

            _screeningEntryRepository.SaveScreeningAttendance(screeningEntry, null);

            _uow.Save();

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

            _uow.Save();

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


        [HttpGet("BarcodeSearch")]
        public IActionResult BarcodeSearch(string searchText)
        {
            var result = _screeningEntryRepository.BarcodeSearch(searchText);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetScreeningList")]
        public IActionResult GetScreeningList([FromBody] ScreeningSearhParamDto searchParam)
        {
            var result = _screeningEntryRepository.GetScreeningList(searchParam);
            return Ok(result);
        }
        [HttpGet("GetProjectStatusAndLevelDropDown/{parentProjectId}")]
        public IActionResult GetProjectStatusAndLevelDropDown(int parentProjectId)
        {
            var screeningSummaryDto = _screeningEntryRepository.GetProjectStatusAndLevelDropDown(parentProjectId);

            return Ok(screeningSummaryDto);
        }

        [HttpGet]
        [Route("GetSubjectByProjecId/{projectId}")]
        public IActionResult GetSubjectByProjecId(int projectId)
        {
            return Ok(_screeningEntryRepository.GetSubjectByProjecId(projectId));
        }

        [HttpGet]
        [Route("GetSubjectByProjecIdLocked/{projectId}/{isLock}/{isParent}")]
        public IActionResult GetSubjectByProjecIdLocked(int projectId, bool isLock, bool isParent)  // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
        {
            return Ok(_screeningEntryRepository.GetSubjectByProjecIdLocked(projectId, isLock, isParent));
        }

        [HttpGet]
        [Route("GetPeriodByProjectIdIsLocked")]
        public IActionResult GetPeriodByProjectIdIsLocked([FromQuery] LockUnlockDDDto lockUnlockDDDto)
        {
            return Ok(_screeningEntryRepository.GetPeriodByProjectIdIsLockedDropDown(lockUnlockDDDto));
        }

        [HttpGet]
        [Route("GetVisitByLockedDropDown")]
        public IActionResult GetVisitByLockedDropDown([FromQuery] LockUnlockDDDto lockUnlockDDDto)
        {
            return Ok(_screeningVisitRepository.GetVisitByLockedDropDown(lockUnlockDDDto));
        }


        //Add by Tinku Mahato for Screening Edit visit list on 21-06-2022
        [HttpGet]
        [Route("GetVisitListByEntryId/{screeningEntryId}")]
        public IActionResult GetVisitListByEntryId(int screeningEntryId)
        {
            var visitList = _screeningVisitRepository.GetScreeningVisitList(screeningEntryId);
            return Ok(visitList);
        }

        // Change by Tinku for add separate dropdown for parent project (24/06/2022)
        [HttpGet]
        [Route("GetSitesByLockUnlock/{parentProjectId}/{isLock}")]
        public IActionResult GetSitesByLockUnlock(int parentProjectId, bool isLock)
        {
            var sites = _screeningEntryRepository.GetSiteByLockUnlock(parentProjectId, isLock);
            return Ok(sites);
        }
    }
}