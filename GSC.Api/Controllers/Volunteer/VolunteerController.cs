using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
using GSC.Data.Entities.Common;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Attendance;
using GSC.Respository.Audit;
using GSC.Respository.Common;
using GSC.Respository.Configuration;
using GSC.Respository.UserMgt;
using GSC.Respository.Volunteer;
using Microsoft.AspNetCore.Mvc;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Report;
using Microsoft.AspNetCore.Authorization;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerController : BaseController
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IAuditTrailRepository _auditTrailRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUserRecentItemRepository _userRecentItemRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IVolunteerSummaryReport _volunteerSummaryReport;

        public VolunteerController(IVolunteerRepository volunteerRepository,
            IUnitOfWork uow, IMapper mapper,
            ILocationRepository locationRepository,
            IUploadSettingRepository uploadSettingRepository,
            IAuditTrailRepository auditTrailRepository,
            IUserRecentItemRepository userRecentItemRepository,
            IRolePermissionRepository rolePermissionRepository,
            IAttendanceRepository attendanceRepository,
            IVolunteerSummaryReport volunteerSummaryReport)
        {
            _volunteerRepository = volunteerRepository;
            _uow = uow;
            _locationRepository = locationRepository;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _auditTrailRepository = auditTrailRepository;
            _userRecentItemRepository = userRecentItemRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _attendanceRepository = attendanceRepository;
            _volunteerSummaryReport = volunteerSummaryReport;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var volunteers = "";//_volunteerRepository.GetVolunteerDetail(isDeleted);
            return Ok(volunteers);
        }

        [HttpPost]
        [Route("Search")]
        public IActionResult Search([FromBody] VolunteerSearchDto search)
        {
            var volunteers = _volunteerRepository.Search(search);
            return Ok(volunteers);
        }

        [HttpGet("GetVolunteerForAttendance/{text}")]
        public IActionResult GetVolunteerForAttendance(string text)
        {
            var search = new VolunteerSearchDto
            {
                TextSearch = text
            };

            var volunteers = _volunteerRepository.GetVolunteerForAttendance(search);
            return Ok(volunteers);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var volunteer = _volunteerRepository.Find(id);
            var volunteerDto = _mapper.Map<VolunteerDto>(volunteer);
            volunteerDto.ProfilePicPath = _uploadSettingRepository.GetWebImageUrl() +
                                          (volunteerDto.ProfilePic ?? DocumentService.DefulatProfilePic);
            volunteerDto.StatusName = volunteerDto.Status.GetDescription();
            volunteerDto.IsBlockDisplay =
                _rolePermissionRepository.GetRolePermissionByScreenCode("mnu_volunteerblock").IsView;

            //_userRecentItemRepository.SaveUserRecentItem(new UserRecentItem
            //{
            //    KeyId = volunteerDto.Id,
            //    SubjectName = volunteerDto.VolunteerNo,
            //    SubjectName1 = volunteer.FullName,
            //    ScreenType = UserRecent.Volunteer
            //});

            return Ok(volunteerDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VolunteerDto volunteerDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (!string.IsNullOrEmpty(volunteerDto.RefNo))
            {
                var validate = _volunteerRepository.DuplicateOldReference(volunteerDto);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }
            }

            volunteerDto.Id = 0;
            if (volunteerDto.FileModel?.Base64?.Length > 0)
                volunteerDto.ProfilePic = new ImageService().ImageSave(volunteerDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Volunteer);

            var volunteer = _mapper.Map<Data.Entities.Volunteer.Volunteer>(volunteerDto);

            if (volunteer.Addresses != null)
                foreach (var address in volunteer.Addresses)
                    address.Location = _locationRepository.SaveLocation(address.Location);

            _volunteerRepository.Add(volunteer);
            if (_uow.Save() <= 0) throw new Exception("Creating volunteer failed on save.");


            if (volunteerDto.Changes != null)
                _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Inserted, volunteer.Id,
                null, volunteerDto.Changes);

            return Ok(volunteer.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VolunteerDto volunteerDto)
        {
            if (volunteerDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (!string.IsNullOrEmpty(volunteerDto.RefNo))
            {
                var validate = _volunteerRepository.DuplicateOldReference(volunteerDto);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }
            }

            if (volunteerDto.FileModel?.Base64?.Length > 0)
                volunteerDto.ProfilePic = new ImageService().ImageSave(volunteerDto.FileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.Volunteer);

            var volunteer = _mapper.Map<Data.Entities.Volunteer.Volunteer>(volunteerDto);

            if (volunteer.Addresses != null)
                foreach (var address in volunteer.Addresses)
                    address.Location = _locationRepository.SaveLocation(address.Location);

            _volunteerRepository.Update(volunteer);
            if (_uow.Save() <= 0) throw new Exception("Updating volunteer failed on save.");

            if (volunteerDto.Changes != null)
                _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Updated, volunteer.Id,
                null, volunteerDto.Changes);

            return Ok(volunteer.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _volunteerRepository.Find(id);

            if (record == null)
                return NotFound();

            _volunteerRepository.Delete(record);
            _uow.Save();

            _auditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Deleted, record.Id,
                null, null);

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _volunteerRepository.Find(id);

            if (record == null)
                return NotFound();
            _volunteerRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("CheckStatus/{id}")]
        public IActionResult CheckStatus(int id)
        {
            if (id <= 0) return BadRequest();

            return Ok(_volunteerRepository.CheckStatus(id));
        }

        [HttpGet("AutoCompleteSearch")]
        public IActionResult AutoCompleteSearch(string searchText)
        {
            var result = _volunteerRepository.AutoCompleteSearch(searchText, true);
            return Ok(result);
        }

        [HttpPost("GetVolunteersForProjectAttendance")]
        public IActionResult GetVolunteersForProjectAttendance([FromBody] VolunteerSearchDto search)
        {
            if (search.PeriodNo > 1)
                return Ok(_attendanceRepository.GetAttendanceAnotherPeriod(search));
            return Ok(_volunteerRepository.GetVolunteerForAttendance(search));
        }

        [HttpGet]
        [Route("GetVolunteerSummary/{volunteerId}")]
        [AllowAnonymous]
        public IActionResult GetVolunteerSummary(int volunteerId)
        {
            var response = _volunteerSummaryReport.GetVolunteerSummaryDesign(volunteerId);
            return response;
        }

    }
}