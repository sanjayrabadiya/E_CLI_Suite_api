using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Volunteer;
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
using System.Collections.Generic;
using Futronic.SDKHelper;
using GSC.Data.Entities.Volunteer;
using System.Linq;
using GSC.Data.Entities.Audit;
using GSC.Respository.Screening;

namespace GSC.Api.Controllers.Volunteer
{
    [Route("api/[controller]")]
    public class VolunteerController : BaseController
    {
        private readonly IAttendanceRepository _attendanceRepository;
        private readonly IVolunteerAuditTrailRepository _volunteerAuditTrailRepository;
        private readonly ILocationRepository _locationRepository;
        private readonly IMapper _mapper;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IVolunteerRepository _volunteerRepository;
        private readonly IVolunteerSummaryReport _volunteerSummaryReport;
        private readonly IVolunteerFingerRepository _volunteerFingerRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;

        public VolunteerController(IVolunteerRepository volunteerRepository,
            IUnitOfWork uow, IMapper mapper,
            ILocationRepository locationRepository,
            IUploadSettingRepository uploadSettingRepository,
            IVolunteerAuditTrailRepository volunteerAuditTrailRepository,
            IRolePermissionRepository rolePermissionRepository,
            IAttendanceRepository attendanceRepository,
            IVolunteerSummaryReport volunteerSummaryReport,
            IVolunteerFingerRepository volunteerFingerRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository)
        {
            _volunteerRepository = volunteerRepository;
            _uow = uow;
            _locationRepository = locationRepository;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _volunteerAuditTrailRepository = volunteerAuditTrailRepository;
            _rolePermissionRepository = rolePermissionRepository;
            _attendanceRepository = attendanceRepository;
            _volunteerSummaryReport = volunteerSummaryReport;
            _volunteerFingerRepository = volunteerFingerRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var volunteers = "";
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

            return Ok(volunteerDto);
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
            if (_uow.Save() <= 0) return Ok(new Exception("Creating volunteer failed on save."));


            if (volunteerDto.Changes != null)
                _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Inserted, volunteer.Id,
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
            if (_uow.Save() <= 0) return Ok(new Exception("Updating volunteer failed on save."));

            if (volunteerDto.Changes != null)
                _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Updated, volunteer.Id,
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

            _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Deleted, record.Id,
                null, null);

            return Ok();
        }

        [HttpPost]
        [Route("DeleteVolunteer")]
        public IActionResult DeleteVolunteer([FromBody] List<int> Data)
        {
            foreach (var item in Data)
            {
                var record = _volunteerRepository.Find(item);

                if (record == null)
                    return NotFound();

                _volunteerRepository.Delete(record);

                _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Deleted, record.Id,
                null, null);
            }

            _uow.Save();
            return Ok();
        }

        [HttpPost]
        [Route("ActiveVolunteer")]
        public IActionResult ActiveVolunteer([FromBody] List<int> Data)
        {
            foreach (var item in Data)
            {
                var record = _volunteerRepository.Find(item);

                if (record == null)
                    return NotFound();

                _volunteerRepository.Active(record);

                _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Activated, record.Id,
               null, null);
            }

            _uow.Save();
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

            _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Activated, record.Id,
                null, null);

            return Ok();
        }


        [HttpPut]
        [Route("assignRandomizationNumberToVoluteer/{volunteerId}/{randomizationNumber}/{reasonId}/{reasonAuth}")]
        public ActionResult AssignRandomizationNumberToVoluteer(int volunteerId, string randomizationNumber, int reasonId, string reasonAuth)
        {
            var record = _volunteerRepository.Find(volunteerId);

            if (record == null)
                return NotFound();

            var isEligible = _screeningTemplateValueRepository.IsEligible(volunteerId);

            if (!isEligible)
            {
                ModelState.AddModelError("Message", "Volunteer not Eligible for Randomization number");
                return BadRequest(ModelState);
            }

            record.RandomizationNumber = randomizationNumber;
            var validate = _volunteerRepository.DuplicateRandomizationNumber(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _volunteerRepository.Update(record);

            _uow.Save();

            VolunteerAuditTrail Changes = new VolunteerAuditTrail();

            Changes.ColumnName = "RandomizationNumber";
            Changes.LabelName = "Randomization Number";
            Changes.NewValue = randomizationNumber;
            Changes.ReasonOth = reasonAuth;
            Changes.ReasonId = reasonId;

            List<VolunteerAuditTrail> change = new List<VolunteerAuditTrail>();
            change.Add(Changes);

            _volunteerAuditTrailRepository.Save(AuditModule.Volunteer, AuditTable.Volunteer, AuditAction.Updated, record.Id,
                null, change);

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
        public IActionResult GetVolunteerSummary(int volunteerId)
        {
            var response = _volunteerSummaryReport.GetVolunteerSummaryDesign(volunteerId);
            return response;
        }

        [HttpPost]
        [Route("GetVolunteersSearchData")]
        [AllowAnonymous]
        public IActionResult GetVolunteersSearchData([FromBody] VolunteerSearchDto search)
        {
            var volunteers = _volunteerRepository.Search(search);
            var response = _volunteerSummaryReport.GetVolunteerSearchDesign(volunteers);
            return response;
        }

        //Add action to get used population type dropdown by Tinku Mahato (07/07/2022)
        [HttpGet]
        [Route("GetPopulationTypeDropDown")]
        public IActionResult GetPopulationTypeDropDown()
        {
            return Ok(_volunteerRepository.GetPopulationTypeDropDownList());
        }

        [HttpPost]
        [Route("VolunteerIdentification")]
        public IActionResult VolunteerIdentification([FromBody] dynamic obj)
        {
            return GetIdentification(obj, false);
        }

        [HttpPost]
        [Route("AddFingerImage")]
        public IActionResult AddFingerImage([FromBody] VolunteerFingerAddDto objVolunteerFingerDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            dynamic obj = GetIdentification(objVolunteerFingerDto.Template, true);

            if (obj.GetType().Name != "Boolean")
            {
                if (obj.UserBlock)
                {
                    ModelState.AddModelError("Message", obj.m_UserName + " is block.");
                    return BadRequest(ModelState);
                }
                else if (obj.UserInActive)
                {
                    ModelState.AddModelError("Message", obj.m_UserName + " is inactive. If required, active the volunteer.");
                    return BadRequest(ModelState);
                }

                ModelState.AddModelError("Message", "Same finger enroll with another volunteer. UserName: " + obj.m_UserName);
                return BadRequest(ModelState);
            }

            VolunteerFingerDto volunteerFingerDto = new VolunteerFingerDto();
            volunteerFingerDto.Id = 0;
            volunteerFingerDto.VolunteerId = objVolunteerFingerDto.VolunteerId;
            volunteerFingerDto.FingerImage = objVolunteerFingerDto.Template;
            var volunteerFinger = _mapper.Map<VolunteerFinger>(volunteerFingerDto);

            _volunteerFingerRepository.Add(volunteerFinger);

            if (_uow.Save() <= 0) return Ok(new Exception("Creating volunteer Finger failed on save."));
            return Ok();
        }

        [HttpGet]
        [Route("GetVolunteerDropDown")]
        public IActionResult GetVolunteerDropDown()
        {
            return Ok(_volunteerRepository.GetVolunteerDropDown());
        }

        public dynamic GetIdentification(dynamic obj, bool isFromAdd)
        {
            int iIndex = 0;
            int nResult;

            byte[] decode_tmplate = Convert.FromBase64String(obj.Split(',')[1]);

            List<DbRecords> Users = _volunteerFingerRepository.GetFingers();

            var rgRecords = Users.Select(item => new FtrIdentifyRecord { KeyValue = item.m_Key.ToByteArray(), Template = Convert.FromBase64String(item.m_Template.Split(',')[1]) }).ToArray();

            FutronicSdkBase m_Operation = new FutronicIdentification();
            ((FutronicIdentification)m_Operation).FARN = 245;
            ((FutronicIdentification)m_Operation).BaseTemplate = decode_tmplate; // the Sample sent from client
            nResult = ((FutronicIdentification)m_Operation).Identification(rgRecords, ref iIndex); // rgRecords are the saved templates.

            if (nResult == FutronicSdkBase.RETCODE_OK)
            {
                if (iIndex != -1)
                {
                    if (isFromAdd)
                    {
                        return Users[iIndex];
                    }
                    return Ok(Users[iIndex]);
                }
                else
                {
                    if (isFromAdd)
                    {
                        return false;
                    }
                    ModelState.AddModelError("Message", "Identification process complete. User not found.");
                    return BadRequest(ModelState);
                }
            }
            else
            {
                if (isFromAdd)
                {
                    return false;
                }
                ModelState.AddModelError("Message", "Identification failed." + FutronicSdkBase.SdkRetCode2Message(nResult));
                return BadRequest(ModelState);
            }
        }

        [HttpGet]
        [Route("GetVolunteerForPKBarcode")]
        public ActionResult GetVolunteerForPKBarcode()
        {
            return Ok(_volunteerRepository.GetVolunteerDropDownForPKBarcode());
        }
    }
}