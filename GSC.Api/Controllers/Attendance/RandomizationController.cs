using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Entities.Attendance;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;
using GSC.Respository.EmailSender;
using Microsoft.Extensions.Configuration;
using GSC.Data.Dto.UserMgt;
using GSC.Shared.DocumentService;
using GSC.Shared;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using GSC.Shared.Generic;
using GSC.Respository.UserMgt;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using GSC.Shared.Configuration;
using GSC.Data.Entities.UserMgt;
using GSC.Data.Dto.Medra;
using GSC.Domain.Context;
using GSC.Respository.SupplyManagement;

namespace GSC.Api.Controllers.Attendance
{
    [Route("api/[controller]")]
    public class RandomizationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IUnitOfWork _uow;
        private readonly ICityRepository _cityRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly IConfiguration _configuration;
        private readonly ICentreUserService _centreUserService;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IStudyVersionRepository _studyVersionRepository;
        private readonly IGSCContext _context;
        private readonly ISupplyManagementFectorDetailRepository _supplyManagementFectorDetailRepository;
        public RandomizationController(IRandomizationRepository randomizationRepository,
            IUnitOfWork uow, IMapper mapper,
            IProjectDesignRepository projectDesignRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            ICityRepository cityRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            IConfiguration configuration,
            ICentreUserService centreUserService,
            IOptions<EnvironmentSetting> environmentSetting,
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository,
            IProjectRepository projectRepository,
            IStudyVersionRepository studyVersionRepository,
            IGSCContext context,
            ISupplyManagementFectorDetailRepository supplyManagementFectorDetailRepository
            )
        {
            _randomizationRepository = randomizationRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectDesignRepository = projectDesignRepository;
            _cityRepository = cityRepository;
            _stateRepository = stateRepository;
            _countryRepository = countryRepository;
            _configuration = configuration;
            _centreUserService = centreUserService;
            _environmentSetting = environmentSetting;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
            _projectRepository = projectRepository;
            _studyVersionRepository = studyVersionRepository;
            _context = context;
            _supplyManagementFectorDetailRepository = supplyManagementFectorDetailRepository;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var randomizations = _randomizationRepository.All.Where(x =>
             (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
             && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
         ).OrderByDescending(t => t.Id).ToList();

            var RandomizationDto = _mapper.Map<IEnumerable<RandomizationDto>>(randomizations);

            return Ok(RandomizationDto);
        }

        [HttpGet("GetRandomizationList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetRandomizationList(int projectId, bool isDeleted)
        {
            var randomizations = _randomizationRepository.GetRandomizationList(projectId, isDeleted);
            return Ok(randomizations);

            //return Ok();
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var randomization = _randomizationRepository.FindByInclude(x => x.Id == id, x => x.Country, x => x.State, x => x.City)
                .SingleOrDefault();
            if (randomization == null)
                return BadRequest();

            if (randomization.DateOfScreening != null && randomization.RandomizationNumber == null)
                _randomizationRepository.SetFactorMappingData(randomization);

            var randomizationDto = _mapper.Map<RandomizationDto>(randomization);
            return Ok(randomizationDto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RandomizationDto randomizationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var randomization = _mapper.Map<Randomization>(randomizationDto);

            var project = _projectRepository.All.Where(x => x.Id == randomization.ProjectId).Select(t => new { t.IsTestSite, t.ParentProjectId }).FirstOrDefault();
            randomizationDto.ParentProjectId = project.ParentProjectId ?? 0;

            if (!project.IsTestSite && !_studyVersionRepository.All.Any(x => x.ProjectId == randomizationDto.ParentProjectId && x.VersionStatus == VersionStatus.GoLive && x.DeletedDate == null))
            {
                ModelState.AddModelError("Message", "Design still not live for this site");
                return BadRequest(ModelState);
            }

            randomization.PatientStatusId = ScreeningPatientStatus.PreScreening;

            //for data save in central - prakash
            var userDto = _mapper.Map<UserDto>(randomizationDto);
            userDto.UserType = UserMasterUserType.Patient;
            userDto.UserName = RandomPassword.CreateRandomNumericNumber(6);
            userDto.CompanyId = _jwtTokenAccesser.CompanyId;
            userDto.IsFirstTime = true;
            userDto.Language = randomizationDto.LanguageId;
            CommonResponceView userdetails = await _centreUserService.SaveUser(userDto, _environmentSetting.Value.CentralApi);
            if (!string.IsNullOrEmpty(userdetails.Message))
            {
                ModelState.AddModelError("Message", userdetails.Message);
                return BadRequest(ModelState);
            }
            randomization.UserId = userdetails.Id;

            _randomizationRepository.AddRandomizationUser(userDto, userdetails);

            if (randomization.LegalStatus == true)
            {
                //for data save in central - prakash
                var userLarDto = new UserDto();
                userLarDto.FirstName = randomization.LegalFirstName;
                userLarDto.MiddleName = randomization.LegalMiddleName;
                userLarDto.LastName = randomization.LegalLastName;
                userLarDto.Email = randomization.LegalEmail;
                userLarDto.UserType = UserMasterUserType.LAR;
                userLarDto.UserName = RandomPassword.CreateRandomNumericNumber(6);
                userLarDto.CompanyId = _jwtTokenAccesser.CompanyId;
                userLarDto.IsFirstTime = true;
                userLarDto.Language = randomizationDto.LanguageId;
                CommonResponceView userLardetails = await _centreUserService.SaveUser(userLarDto, _environmentSetting.Value.CentralApi);
                if (!string.IsNullOrEmpty(userLardetails.Message))
                {
                    ModelState.AddModelError("Message", userLardetails.Message);
                    return BadRequest(ModelState);
                }
                randomization.LARUserId = userLardetails.Id;

                _randomizationRepository.AddRandomizationUserLAR(userLarDto, userLardetails);
            }

            _randomizationRepository.Add(randomization);
            var Project = _context.Project.Where(x => x.Id == randomization.ProjectId).FirstOrDefault();
            var projectSetting = _context.ProjectSettings.Where(x => x.ProjectId == Project.ParentProjectId && x.DeletedBy == null).FirstOrDefault();

            if (_uow.Save() <= 0) throw new Exception("Creating randomization failed on save.");

            if (projectSetting != null && projectSetting.IsEicf)
            {
                await _randomizationRepository.SendEmailOfScreenedtoPatient(randomization, 2);
                _randomizationRepository.SendEmailOfStartEconsent(randomization);

                if (randomization.LegalStatus == true)
                {
                    await _randomizationRepository.SendEmailOfScreenedtoPatientLAR(randomization, 2);
                    _randomizationRepository.SendEmailOfStartEconsentLAR(randomization);
                }

            }

            return Ok();
        }

        [HttpPut]
        public async Task<IActionResult> Put([FromBody] RandomizationDto RandomizationDto)
        {
            if (RandomizationDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            RandomizationDto.Initial = RandomizationDto.Initial.PadRight(3, '-');
            var details = _randomizationRepository.Find(RandomizationDto.Id);
            var randomization = _mapper.Map<Randomization>(RandomizationDto);
            randomization.PatientStatusId = details.PatientStatusId;

            var userDetail = _userRepository.FindBy(x => x.Id == details.UserId).FirstOrDefault();
            userDetail.FirstName = RandomizationDto.FirstName;
            userDetail.MiddleName = RandomizationDto.MiddleName;
            userDetail.LastName = RandomizationDto.LastName;
            userDetail.DateOfBirth = RandomizationDto.DateOfBirth;
            userDetail.Email = RandomizationDto.Email;
            userDetail.Language = RandomizationDto.LanguageId;
            userDetail.Phone = RandomizationDto.PrimaryContactNumber;
            var userDto = _mapper.Map<UserDto>(userDetail);
            CommonResponceView userdetails = await _centreUserService.UpdateUser(userDto, _environmentSetting.Value.CentralApi);
            if (!string.IsNullOrEmpty(userdetails.Message))
            {
                ModelState.AddModelError("Message", userdetails.Message);
                return BadRequest(ModelState);
            }
            randomization.UserId = userdetails.Id;
            var user = _mapper.Map<Data.Entities.UserMgt.User>(userDto);
            user.Id = userdetails.Id;
            _userRepository.Update(user);

            if (!String.IsNullOrEmpty(randomization.LegalFirstName))
            {
                var userLARDetail = _userRepository.FindBy(x => x.Id == details.LARUserId).FirstOrDefault();
                userLARDetail.FirstName = RandomizationDto.LegalFirstName;
                userLARDetail.MiddleName = RandomizationDto.LegalMiddleName;
                userLARDetail.LastName = RandomizationDto.LegalLastName;
                userLARDetail.Email = RandomizationDto.LegalEmail;
                userLARDetail.Phone = RandomizationDto.LegalEmergencyCoNumber;
                var userLARDto = _mapper.Map<UserDto>(userLARDetail);
                CommonResponceView userLARDetails = await _centreUserService.UpdateUser(userLARDto, _environmentSetting.Value.CentralApi);
                if (!string.IsNullOrEmpty(userLARDetails.Message))
                {
                    ModelState.AddModelError("Message", userLARDetails.Message);
                    return BadRequest(ModelState);
                }
                randomization.LARUserId = userLARDetails.Id;
                var userLAR = _mapper.Map<Data.Entities.UserMgt.User>(userLARDto);
                userLAR.Id = userLARDetails.Id;
                _userRepository.Update(userLAR);
            }

            _randomizationRepository.Update(randomization);
            if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");
            return Ok(randomization.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {

            if (!_randomizationRepository.All.Any(x => x.Id == id && (x.PatientStatusId == ScreeningPatientStatus.PreScreening || x.PatientStatusId == null)))
            {
                ModelState.AddModelError("Message", "Can not delete , because this record is under process.");
                return BadRequest(ModelState);
            }

            _randomizationRepository.Delete(id);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _randomizationRepository.Find(id);

            if (record == null)
                return NotFound();
            _randomizationRepository.Active(record);
            _uow.Save();
            return Ok();
        }

        [HttpPost]
        [Route("ResendSMSandEmail/{id}/{type}")]
        public async Task<IActionResult> ResendSMSandEmail(int id, int type)
        {
            if (id <= 0) return BadRequest();
            var randomization = _randomizationRepository.Find(id);

            if (type == 1 && (randomization.Email == null || randomization.Email == ""))
            {
                ModelState.AddModelError("Message", "Email is not set for this patient");
                return BadRequest(ModelState);
            }

            var Project = _context.Project.Where(x => x.Id == randomization.ProjectId).FirstOrDefault();
            var projectSetting = _context.ProjectSettings.Where(x => x.ProjectId == Project.ParentProjectId && x.DeletedBy == null).FirstOrDefault();


            if (randomization.UserId != null)
            {
                var userdata = _userRepository.Find((int)randomization.UserId);
                var user = new UserViewModel();

                user = await _centreUserService.GetUserDetails($"{_environmentSetting.Value.CentralApi}Login/GetUserDetails/{userdata.UserName}");

                if (user.IsFirstTime == true)
                {
                    await _randomizationRepository.SendEmailOfScreenedtoPatient(randomization, type);
                }
                else
                {
                    ModelState.AddModelError("Message", "Patient or Lar already logged in");
                    return BadRequest(ModelState);
                }
            }

            if (randomization.LARUserId != null)
            {
                var userLARdata = _userRepository.Find((int)randomization.LARUserId);
                var userLAR = new UserViewModel();

                userLAR = await _centreUserService.GetUserDetails($"{_environmentSetting.Value.CentralApi}Login/GetUserDetails/{userLARdata.UserName}");
                if (userLAR.IsFirstTime == true)
                {
                    await _randomizationRepository.SendEmailOfScreenedtoPatientLAR(randomization, type);
                }
                else
                {
                    ModelState.AddModelError("Message", "Patient or Lar already logged in");
                    return BadRequest(ModelState);
                }
            }

            //if (user.IsFirstTime == true || userLAR.IsFirstTime == true)
            //{
            //    if (user.IsFirstTime == true)
            //        await _randomizationRepository.SendEmailOfScreenedtoPatient(randomization, type);

            //    if (userLAR.IsFirstTime == true)
            //        await _randomizationRepository.SendEmailOfScreenedtoPatientLAR(randomization, type);
            //    //if (projectSetting.IsEicf)
            //    //    _randomizationRepository.SendEmailOfStartEconsent(randomization);
            //}
            //else
            //{
            //    ModelState.AddModelError("Message", "Patient or Lar already logged in");
            //    return BadRequest(ModelState);
            //}

            return Ok(id);
        }


        [HttpPut]
        [Route("saveScreeningNumber")]
        public async Task<IActionResult> SaveScreeningNumber([FromBody] RandomizationDto randomizationDto)
        {

            if (randomizationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var randomization = _randomizationRepository.Find(randomizationDto.Id);
            var Project = _context.Project.Where(x => x.Id == randomization.ProjectId).FirstOrDefault();
            var projectSetting = _context.ProjectSettings.Where(x => x.ProjectId == Project.ParentProjectId && x.DeletedBy == null).FirstOrDefault();

            var validate = _randomizationRepository.Duplicate(randomizationDto, randomizationDto.ProjectId);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            var validatescreeningno = _randomizationRepository.ValidateScreeningNumber(randomizationDto);
            if (!string.IsNullOrEmpty(validatescreeningno))
            {
                ModelState.AddModelError("Message", validatescreeningno);
                return BadRequest(ModelState);
            }

            _randomizationRepository.SaveScreeningNumber(randomization, randomizationDto);

            if (projectSetting == null || !projectSetting.IsEicf)
                _randomizationRepository.SendEmailOfStartEconsent(randomization);

            await _randomizationRepository.SendEmailOfScreenedtoPatient(randomization, 2);

            if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");

            return Ok(randomization.Id);
        }


        [HttpPut]
        [Route("saveRandomizationNumber")]
        public IActionResult SaveRandomizationNumber([FromBody] RandomizationDto randomizationDto)
        {
            var randno = string.Empty;
            if (randomizationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var numerformate = _context.RandomizationNumberSettings.Where(x => x.ProjectId == randomizationDto.ParentProjectId && x.DeletedDate == null).FirstOrDefault();

            var randomization = _randomizationRepository.Find(randomizationDto.Id);
            randno = randomization.RandomizationNumber;
            var validate = _randomizationRepository.Duplicate(randomizationDto, randomizationDto.ProjectId);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            var validaterandomizationno = _randomizationRepository.ValidateRandomizationNumber(randomizationDto);
            if (!string.IsNullOrEmpty(validaterandomizationno))
            {
                ModelState.AddModelError("Message", validaterandomizationno);
                return BadRequest(ModelState);
            }

            _randomizationRepository.SaveRandomizationNumber(randomization, randomizationDto);

            if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");



            if (string.IsNullOrEmpty(randno))
            {
                if (numerformate.IsIWRS || numerformate.IsIGT)
                {
                    randomizationDto = _randomizationRepository.CheckDuplicateRandomizationNumberIWRS(randomizationDto, numerformate);
                    if (!string.IsNullOrEmpty(randomizationDto.ErrorMessage))
                    {
                        ModelState.AddModelError("Message", randomizationDto.ErrorMessage);
                        return BadRequest(ModelState);
                    }
                    _randomizationRepository.SendRandomizationIWRSEMail(randomizationDto);
                    _randomizationRepository.SendRandomizationThresholdEMail(randomizationDto);
                }
            }
            return Ok(randomizationDto);
        }

        //[HttpPut]
        //[Route("ChangeStatustoConsentInProgress")]
        //public IActionResult ChangeStatustoConsentInProgress()
        //{
        //    _randomizationRepository.ChangeStatustoConsentInProgress();
        //    _uow.Save();
        //    return Ok();
        //}

        [HttpPut]
        [Route("ConsentStart")]
        public IActionResult ConsentStart()
        {
            var subjectDetail = _randomizationRepository.FindByInclude(x => x.UserId == _jwtTokenAccesser.UserId, x => x.EconsentReviewDetails).SingleOrDefault();
            if (subjectDetail.EconsentReviewDetails.Count() > 0)
            {
                subjectDetail.PatientStatusId = ScreeningPatientStatus.ConsentInProcess;
                _randomizationRepository.Update(subjectDetail);
                _uow.Save();
            }
            return Ok(subjectDetail.PatientStatusId);
        }

        //[HttpPut]
        //[Route("ChangeStatustoWithdrawal")]
        //public IActionResult ChangeStatustoWithdrawal([FromBody] FileModel fileModel)
        ////public IActionResult ChangeStatustoWithdrawal()
        //{
        //    //public IActionResult ChangeStatustoWithdrawal([FromBody] FileModel fileModel)
        //    _randomizationRepository.ChangeStatustoWithdrawal(fileModel);
        //    _uow.Save();
        //    return Ok();
        //}

        [HttpGet("GetPatientVisits")]
        public IActionResult GetPatientVisits()
        {
            var randomization = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            if (randomization != null && (randomization.PatientStatusId == ScreeningPatientStatus.ConsentInProcess || randomization.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess))
            {
                ModelState.AddModelError("Message", "Please complete your Consent first");
                return BadRequest(ModelState);
            }
            var data = _randomizationRepository.GetPatientVisits();
            if (data == null || data.ToList().Count <= 0)
            {
                ModelState.AddModelError("Message", "Your Visit is not started, Please contact your administrator");
                return BadRequest(ModelState);
            }
            return Ok(data);
        }

        [HttpGet("GetPatientTemplates/{screeningVisitId}")]
        public IActionResult GetPatientTemplates(int screeningVisitId)
        {
            var data = _randomizationRepository.GetPatientTemplates(screeningVisitId);
            return Ok(data);
        }



        [HttpGet("GetRandomizationNumber/{id}")]
        public IActionResult GetRandomizationNumber(int id)
        {
            var randdata = _randomizationRepository.All.Where(x => x.Id == id).FirstOrDefault();

            if (randdata.DateOfScreening != null && randdata.RandomizationNumber == null)
                _randomizationRepository.SetFactorMappingData(randdata);

            var isvalid = _randomizationRepository.IsRandomFormatSetInStudy(id);
            if (isvalid == true)
            {
                var data = _randomizationRepository.GetRandomizationNumber(id);
                if (data.IsIGT && randdata.PatientStatusId != ScreeningPatientStatus.Screening && randdata.PatientStatusId != ScreeningPatientStatus.OnTrial)
                {
                    ModelState.AddModelError("Message", "Patient status is not eligible for randomization");
                    return BadRequest(ModelState);
                }
                if (data.IsIGT && !string.IsNullOrEmpty(data.ErrorMessage))
                {
                    ModelState.AddModelError("Message", data.ErrorMessage);
                    return BadRequest(ModelState);
                }
                if (data.IsIWRS && !data.IsDoseWiseKit && string.IsNullOrEmpty(data.KitNo))
                {
                    ModelState.AddModelError("Message", "kit is not available");
                    return BadRequest(ModelState);
                }
                if (data.IsIWRS && data.IsDoseWiseKit && data.KitDoseList.Count == 0)
                {
                    ModelState.AddModelError("Message", "kit is not available");
                    return BadRequest(ModelState);
                }
                if (data.IsIGT && string.IsNullOrEmpty(data.RandomizationNumber))
                {
                    ModelState.AddModelError("Message", "Please upload randomization sheet");
                    return BadRequest(ModelState);
                }

                return Ok(data);
            }
            else
            {
                ModelState.AddModelError("Message", "Please set Randomization Number format in Study Setup");
                return BadRequest(ModelState);
            }

        }

        [HttpGet("GetScreeningNumber/{id}")]
        public IActionResult GetScreeningNumber(int id)
        {
            var isvalid = _randomizationRepository.IsScreeningFormatSetInStudy(id);
            if (isvalid == true)
            {
                var data = _randomizationRepository.GetScreeningNumber(id);
                return Ok(data);
            }
            else
            {
                ModelState.AddModelError("Message", "Please set Screening Number format in Study Setup");
                return BadRequest(ModelState);
            }

        }

        [HttpGet]
        [Route("GetRandomizationDropdown/{projectId}")]
        public IActionResult GetRandomizationDropdown(int projectId)
        {
            return Ok(_randomizationRepository.GetRandomizationDropdown(projectId));
        }

        [HttpGet]
        [Route("GetSubjectStatus/{projectId}")]
        public IActionResult GetSubjectStatus(int projectId)
        {
            return Ok(_randomizationRepository.GetSubjectStatus(projectId));
        }

        [HttpPost]
        [Route("GetSubjectForMeddraCodingDropDown")]
        public IActionResult GetSubjectForMeddraCodingDropDown([FromBody] MeddraCodingSearchDto filters)
        {
            return Ok(_randomizationRepository.GetAttendanceForMeddraCodingDropDown(filters));
        }

        [HttpGet]
        [Route("GetDashboardPatientStatus/{projectId}")]
        public IActionResult GetDashboardPatientStatus(int projectId)
        {
            return Ok(_randomizationRepository.GetDashboardPatientStatus(projectId));
        }

        [HttpGet]
        [Route("GetDashboardRecruitmentStatus/{projectId}")]
        public IActionResult GetDashboardRecruitmentStatus(int projectId)
        {
            return Ok(_randomizationRepository.GetDashboardRecruitmentStatus(projectId));
        }

        [HttpGet]
        [Route("GetDashboardRecruitmentRate/{projectId}")]
        public IActionResult GetDashboardRecruitmentRate(int projectId)
        {
            return Ok(_randomizationRepository.GetDashboardRecruitmentRate(projectId));
        }

        [HttpGet("GetFactorSetting/{id}")]
        public IActionResult GetFactorSetting(int id)
        {
            RandomizationFactor randomizationDto = new RandomizationFactor();
            var factorData = _supplyManagementFectorDetailRepository.All.Where(x => x.DeletedDate == null && x.SupplyManagementFector.ProjectId == id).ToList();
            var randomizationdata = _context.RandomizationNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == id).ToList();
            if (factorData != null && factorData.Count > 0)
            {
                randomizationDto.IsGenderFactor = factorData.Any(x => x.Fector == Fector.Gender);
                randomizationDto.IsDaitoryFactor = factorData.Any(x => x.Fector == Fector.Diatory);
                randomizationDto.IsAgeFactor = factorData.Any(x => x.Fector == Fector.Age);
                randomizationDto.IsBMIFactor = factorData.Any(x => x.Fector == Fector.BMI);
                randomizationDto.IsJointFactor = factorData.Any(x => x.Fector == Fector.Joint);
                randomizationDto.IsEligibilityFactor = factorData.Any(x => x.Fector == Fector.Eligibility);
                randomizationDto.isWeightFactor = factorData.Any(x => x.Fector == Fector.Weight);
                randomizationDto.isDoseFactor = factorData.Any(x => x.Fector == Fector.Dose);
                randomizationDto.IsIWRS = randomizationdata.Count > 0 ? randomizationdata.Any(x => x.IsIWRS == true || x.IsIGT == true) : false;
                randomizationDto.IsDisable = _context.SupplyManagementFactorMapping.Any(x => x.DeletedDate == null && x.ProjectId == id) ? true : false;
            }
            return Ok(randomizationDto);
        }
    }
}