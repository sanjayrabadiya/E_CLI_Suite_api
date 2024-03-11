using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Medra;
using GSC.Data.Dto.Screening;
using GSC.Domain.Context;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Attendance
{
    [Route("api/[controller]")]
    public class AttendanceController : BaseController
    {
        private readonly IAttendanceHistoryRepository _attendanceHistoryRepository;
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IMapper _mapper;
        private readonly IStudyVersionRepository _studyVersionRepository;
        private readonly IProjectSubjectRepository _projectSubjectRepository;
        private readonly IUnitOfWork _uow;
        private readonly IScreeningHistoryRepository _screeningHistoryRepository;

        public AttendanceController(IAttendanceRepository attendanceRepository,
            IUnitOfWork uow, IMapper mapper,
            IProjectSubjectRepository projectSubjectRepository,
            IAttendanceHistoryRepository attendanceHistoryRepository,
            IStudyVersionRepository studyVersionRepository, IScreeningHistoryRepository screeningHistoryRepository)
        {
            _attendanceRepository = attendanceRepository;
            _uow = uow;
            _mapper = mapper;
            _projectSubjectRepository = projectSubjectRepository;
            _attendanceHistoryRepository = attendanceHistoryRepository;
            _studyVersionRepository = studyVersionRepository;
            _screeningHistoryRepository = screeningHistoryRepository;
        }

        [HttpPost]
        [Route("GetAttendaceList")]
        public IActionResult GetAttendaceList([FromBody] ScreeningSearhParamDto attendanceSearch)
        {
            var volunteers = _attendanceRepository.GetAttendaceList(attendanceSearch);
            return Ok(volunteers);
        }

        [HttpPost]
        [Route("GetAttendaceListByLock/{isLock}")]
        public IActionResult GetAttendaceListByLock([FromBody] ScreeningSearhParamDto attendanceSearch, bool isLock)
        {
            var volunteers = _attendanceRepository.GetAttendaceListByLock(attendanceSearch, isLock);
            return Ok(volunteers);
        }

        [HttpPost]
        [Route("GetAttendaceForProjectRightList")]
        public IActionResult GetAttendaceForProjectRightList([FromBody] ScreeningSearhParamDto attendanceSearch)
        {
            var volunteers = _attendanceRepository.GetAttendaceForProjectRightList(attendanceSearch);
            return Ok(volunteers);
        }

        [HttpPost]
        public IActionResult Post([FromBody] AttendanceDto attendanceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var validate = _attendanceRepository.CheckVolunteer(attendanceDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            var validatemsg = _screeningHistoryRepository.CheckVolunteerEligibaleDate(attendanceDto);
            if (!string.IsNullOrEmpty(validatemsg))
            {
                ModelState.AddModelError("Message", validatemsg);
                return BadRequest(ModelState);
            }

            attendanceDto.Id = 0;
            attendanceDto.StudyVersion = _studyVersionRepository.GetStudyVersionForLive(attendanceDto.ProjectId);
            var attendance = _mapper.Map<Data.Entities.Attendance.Attendance>(attendanceDto);
            _attendanceRepository.SaveAttendance(attendance);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating attendance failed on save.");
                return BadRequest(ModelState);
            }
            _mapper.Map<AttendanceDto>(attendance);
            return Ok(attendanceDto.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _attendanceRepository.Find(id);

            if (record == null)
                return NotFound();

            if (record.IsProcessed)
            {
                ModelState.AddModelError("Message", "Can not delete Attendance, because this record is under process.");
                return BadRequest(ModelState);
            }

            _attendanceHistoryRepository.SaveHistory("Delete attendance", record.Id, record.AuditReasonId);
            _attendanceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _attendanceRepository.Find(id);

            if (record == null)
                return NotFound();
            _attendanceRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetVolunteersByProjectId/{projectId}")]
        public IActionResult GetVolunteersByProjectId(int projectId)
        {
            return Ok(_attendanceRepository.GetVolunteersByProjectId(projectId));
        }

        [HttpGet]
        [Route("GetVolunteersForReplacement/{projectId}")]
        public IActionResult GetVolunteersForReplacement(int projectId)
        {
            return Ok(_attendanceRepository.GetVolunteersForReplacement(projectId));
        }

        [HttpPut]
        [Route("ReplaceVolunteer/{currentProjectSubjectId}/{attendanceId}")]
        public IActionResult ReplaceVolunteer(int currentProjectSubjectId, int attendanceId)
        {
            var projectSubject = _projectSubjectRepository.Find(currentProjectSubjectId);

            if (projectSubject.IsRepaced)
            {
                ModelState.AddModelError("Message", "Already replacement!");
                return BadRequest(ModelState);
            }

            _projectSubjectRepository.ReplaceSubjectNumber(currentProjectSubjectId, attendanceId);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Replace Volunteer failed.");
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [HttpPut]
        [Route("ProjectSuspended/{projectId}")]
        public IActionResult ProjectSuspended(int projectId)
        {
            var result = _attendanceRepository.ProjectSuspended(projectId);

            if (!string.IsNullOrEmpty(result))
            {
                ModelState.AddModelError("Message", result);
                return BadRequest(ModelState);
            }

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Replace Volunteer failed.");
                return BadRequest(ModelState);
            }

            return Ok();
        }


        [HttpGet]
        [Route("GetAttendanceHistory/{id}")]
        public IActionResult GetAttendanceHistory(int id)
        {
            return Ok(_attendanceHistoryRepository.GetAttendanceHistory(id));
        }

        [HttpPost]
        [Route("GetAttendanceForMeddraCodingDropDown")]
        public IActionResult GetAttendanceForMeddraCodingDropDown([FromBody] MeddraCodingSearchDto filters)
        {
            return Ok(_attendanceRepository.GetAttendanceForMeddraCodingDropDown(filters));
        }
    }
}