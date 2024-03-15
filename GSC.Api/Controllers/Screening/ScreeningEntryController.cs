using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
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
        private readonly IGSCContext _context;
        public ScreeningEntryController(IScreeningEntryRepository screeningEntryRepository,
            IUnitOfWork uow, IMapper mapper,
            IAttendanceRepository attendanceRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IScreeningProgress screeningProgress,
            IScreeningVisitRepository screeningVisitRepository,
            IScreeningHistoryRepository screeningHistoryRepository,
            IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
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
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var screeningEntryDto = _screeningEntryRepository.GetDetails(id,null);
     
            return Ok(screeningEntryDto);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ScreeningEntryDto screeningEntryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (screeningEntryDto.AttendanceId == 0 || screeningEntryDto.AttendanceId == null)
                return Ok(new Exception("Not found Attendance"));

            var attendance = _attendanceRepository.Find((int)screeningEntryDto.AttendanceId);

            screeningEntryDto.StudyVersion = attendance.StudyVersion;
            screeningEntryDto.ScreeningDate = screeningEntryDto.ScreeningDate == null ? attendance.AttendanceDate : screeningEntryDto.ScreeningDate;

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
            if (result.Id <= 0) return Ok(new Exception("Creating Screening Entry failed on save."));

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
            if(temp != null) { 
            screeningEntry.ProjectId = temp.ProjectId;
            }

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

        [HttpGet("VolunteerSearch")]
        public IActionResult VolunteerSearch(string searchText)
        {
            var result = _screeningEntryRepository.VolunteerSearch(searchText);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetScreeningList")]
        public IActionResult GetScreeningList([FromBody] ScreeningSearhParamDto searchParam)
        {
            var result = _screeningEntryRepository.GetScreeningDataList(searchParam);
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
        [Route("GetSubjectByProjecIdLocked/{projectId}/{isLock}/{isHardLock}/{isParent}")]
        public IActionResult GetSubjectByProjecIdLocked(int projectId, bool isLock, bool isHardLock, bool isParent)  // Change by Tinku for add separate dropdown for parent project (24/06/2022) 
        {
            return Ok(_screeningEntryRepository.GetSubjectByProjecIdLocked(projectId, isLock, isHardLock, isParent));
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
        [Route("GetSitesByLockUnlock/{parentProjectId}/{isLock}/{isHardLock}")]
        public IActionResult GetSitesByLockUnlock(int parentProjectId, bool isLock, bool isHardLock)
        {
            var sites = _screeningEntryRepository.GetSiteByLockUnlock(parentProjectId, isLock, isHardLock);
            return Ok(sites);
        }
        [HttpGet]
        [Route("GetProjectByLockUnlock/{isLock}/{isHardLock}")]
        public IActionResult GetProjectByLockUnlock(bool isLock, bool isHardLock)
        {
            var projects = _screeningEntryRepository.GetParentProjectDropdown(isLock, isHardLock);
            return Ok(projects);
        }

        [HttpPost]
        [Route("SaveVolunteerProject")]
        public IActionResult SaveVolunteerProject([FromBody] VolunteerProject volunteerProject)
        {
            var data = _screeningEntryRepository.All.Where(x => x.Id == volunteerProject.ScreeningEntryId).FirstOrDefault();
            if (data == null)
            {
                ModelState.AddModelError("Message", "Screening not started!");
                return BadRequest(ModelState);
            }

            if (data.IsFitnessFit == false || data.IsEnrolled == false)
            {
                ModelState.AddModelError("Message", "Subject is not fit, You cannot assign project number.");
                return BadRequest(ModelState);
            }

            data.StudyId = volunteerProject.ProjectId;
            data.Notes = volunteerProject.Notes;
            _screeningEntryRepository.Update(data);

            var screeninghistory = new ScreeningEntryStudyHistory
            {
                StudyId = volunteerProject.ProjectId,
                ScreeningEntryId = volunteerProject.ScreeningEntryId,
                Notes = volunteerProject.Notes,
                RoleId = _jwtTokenAccesser.RoleId
            };
            _context.ScreeningEntryStudyHistory.Add(screeninghistory);
            _uow.Save();
            return Ok(screeninghistory.Id);
        }
        [HttpGet]
        [Route("GetVolunteerProjectHistory/{ScreeningEntryId}")]
        public IActionResult GetVolunteerProjectHistory(int ScreeningEntryId)
        {
            var sites = _screeningEntryRepository.GetVolunteerProjectHistory(ScreeningEntryId);
            return Ok(sites);
        }

        [HttpGet]
        [Route("GetVolunteerByProjectId/{projectId}")]
        public IActionResult GetVolunteerByProjectId(int projectId)
        {
            var volunteerList = _screeningEntryRepository.GetVolunteerByProjectId(projectId);
            return Ok(volunteerList);
        }

        [HttpGet]
        [Route("GetVolunteerScreeningList/{projectId}/{volunteerId}")]
        public IActionResult GetVolunteerScreeningList(int projectId, int volunteerId)
        {
            var volunteerList = _screeningEntryRepository.GetVolunteerScreeningList(projectId, volunteerId);
            return Ok(volunteerList);
        }

        [HttpGet]
        [Route("GetScreeningVisitsDataReview/{id}/{siteId}")]
        public IActionResult GetScreeningVisitsDataReview(int id, int siteId)
        {
            var screeningEntryDto = _screeningEntryRepository.GetDetails(id, siteId);

            return Ok(screeningEntryDto);
        }

        // NA Report for visit
        [HttpGet]
        [Route("GetNAReportData")]
        public IActionResult GetNAReportData([FromQuery] NAReportSearchDto filters)
        {
            if (filters.SiteId <= 0) return BadRequest();

            var reportDto = _screeningVisitRepository.NAReport(filters);

            return Ok(reportDto);
        }

        [HttpPost("SetStatusNA")]
        public ActionResult SetStatusNA([FromBody] List<int> screeningTemplateId)
        {
            foreach (var item in screeningTemplateId)
            {
                var record = _screeningVisitRepository.Find(item);
                if (record == null)
                    return NotFound();
                record.IsNA = true;
                _screeningVisitRepository.Update(record);
                _uow.Save();
            }
            return Ok(true);
        }
        // NA Report
    }
}