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
        private readonly IAPICall _centeralendpoint;
        private readonly IConfiguration _configuration;
        private readonly ICentreUserService _centreUserService;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IUserRepository _userRepository;
        private readonly IUserRoleRepository _userRoleRepository;

        public RandomizationController(IRandomizationRepository randomizationRepository,
            IUnitOfWork uow, IMapper mapper,
            IProjectDesignRepository projectDesignRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            ICityRepository cityRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            IAPICall centeralendpoint,
            IConfiguration configuration,
            ICentreUserService centreUserService,
            IOptions<EnvironmentSetting> environmentSetting,
            IUserRepository userRepository,
            IUserRoleRepository userRoleRepository
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
            _centeralendpoint = centeralendpoint;
            _configuration = configuration;
            _centreUserService = centreUserService;
            _environmentSetting = environmentSetting;
            _userRepository = userRepository;
            _userRoleRepository = userRoleRepository;
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
            return Ok(_randomizationRepository.GetRandomizationList(projectId, isDeleted));
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var randomization = _randomizationRepository.FindByInclude(x => x.Id == id, x => x.Country, x => x.State, x => x.City)
                .SingleOrDefault();
            if (randomization == null)
                return BadRequest();

            var randomizationDto = _mapper.Map<RandomizationDto>(randomization);

            return Ok(randomizationDto);
        }

        [HttpPost]
        public async Task<IActionResult> Post([FromBody] RandomizationDto randomizationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var randomization = _mapper.Map<Randomization>(randomizationDto);
            randomization.PatientStatusId = ScreeningPatientStatus.PreScreening;
            if (!_environmentSetting.Value.IsPremise)
            {
                var userDto = _mapper.Map<UserDto>(randomizationDto);
                userDto.UserType = UserMasterUserType.Patient;
                userDto.UserName = RandomPassword.CreateRandomNumericNumber(6);
                userDto.CompanyId = _jwtTokenAccesser.CompanyId;
                userDto.IsFirstTime = true;
                CommonResponceView userdetails = await _centreUserService.SaveUser(userDto, _environmentSetting.Value.CentralApi);
                if (!string.IsNullOrEmpty(userdetails.Message))
                {
                    ModelState.AddModelError("Message", userdetails.Message);
                    return BadRequest(ModelState);
                }
                randomization.UserId = userdetails.Id;

                var user = _mapper.Map<Data.Entities.UserMgt.User>(userDto);
                user.Id = userdetails.Id;
                _userRepository.Add(user);
                UserRole userRole = new UserRole();
                userRole.UserId= userdetails.Id;
                userRole.UserRoleId = 2;
               _userRoleRepository.Add(userRole);
            }

            _randomizationRepository.SendEmailOfStartEconsent(randomization);
            _randomizationRepository.Add(randomization);

            if (_uow.Save() <= 0) throw new Exception("Creating randomization failed on save.");
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
            if (!_environmentSetting.Value.IsPremise)
            {
                var userDetail = _userRepository.FindBy(x => x.Id == details.UserId).FirstOrDefault();
                userDetail.FirstName = RandomizationDto.FirstName;
                userDetail.MiddleName = RandomizationDto.LastName;
                userDetail.LastName = RandomizationDto.LastName;
                userDetail.DateOfBirth = RandomizationDto.DateOfBirth;
                userDetail.Email = RandomizationDto.Email;
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
                ModelState.AddModelError("Message", "Can not delete , because this record in under process.");
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


        [HttpPut]
        [Route("saveScreeningNumber")]
        public IActionResult SaveScreeningNumber([FromBody] RandomizationDto randomizationDto)
        {
            if (randomizationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (_projectDesignRepository.All.Any(x => x.DeletedDate == null && x.ProjectId == randomizationDto.ParentProjectId && !x.IsCompleteDesign))
            {
                ModelState.AddModelError("Message", "Design is not complete");
                return BadRequest(ModelState);
            }

            var randomization = _randomizationRepository.Find(randomizationDto.Id);

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

            //_randomizationRepository.Update(randomization);
            _randomizationRepository.SendEmailOfStartEconsent(randomization);
            _randomizationRepository.SendEmailOfScreenedtoPatient(randomization);

            if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");

            return Ok(randomization.Id);
        }


        [HttpPut]
        [Route("saveRandomizationNumber")]
        public IActionResult SaveRandomizationNumber([FromBody] RandomizationDto randomizationDto)
        {
            if (randomizationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (_projectDesignRepository.All.Any(x => x.DeletedDate == null && x.ProjectId == randomizationDto.ParentProjectId && !x.IsCompleteDesign))
            {
                ModelState.AddModelError("Message", "Design is not complete");
                return BadRequest(ModelState);
            }

            var randomization = _randomizationRepository.Find(randomizationDto.Id);

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

            //_randomizationRepository.Update(randomization);
            //_randomizationRepository.SendEmailOfStartEconsent(randomization);

            if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");

            return Ok(randomization.Id);
        }

        //[HttpPut]
        //[Route("SaveRandomization")]
        //public IActionResult SaveRandomization([FromBody] RandomizationDto randomizationDto)
        //{
        //    if (randomizationDto.Id <= 0) return BadRequest();

        //    if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);


        //    if (_projectDesignRepository.All.Any(x => x.DeletedDate == null && x.ProjectId == randomizationDto.ParentProjectId && !x.IsCompleteDesign))
        //    {
        //        ModelState.AddModelError("Message", "Design is not complete");
        //        return BadRequest(ModelState);
        //    }


        //    var randomization = _randomizationRepository.Find(randomizationDto.Id);


        //    var validate = _randomizationRepository.Duplicate(randomizationDto, randomizationDto.ProjectId);
        //    if (!string.IsNullOrEmpty(validate))
        //    {
        //        ModelState.AddModelError("Message", validate);
        //        return BadRequest(ModelState);
        //    }

        //    var validaterandomizationscreeningno = _randomizationRepository.ValidateRandomizationAndScreeningNumber(randomizationDto);
        //    if (!string.IsNullOrEmpty(validaterandomizationscreeningno))
        //    {
        //        ModelState.AddModelError("Message", validaterandomizationscreeningno);
        //        return BadRequest(ModelState);
        //    }

        //    _randomizationRepository.SaveRandomization(randomization, randomizationDto);

        //    //_randomizationRepository.Update(randomization);
        //    _randomizationRepository.SendEmailOfStartEconsent(randomization);

        //    if (_uow.Save() <= 0) throw new Exception("Updating None register failed on save.");

        //    return Ok(randomization.Id);
        //}

        [HttpPut]
        [Route("ChangeStatustoConsentInProgress")]
        public IActionResult ChangeStatustoConsentInProgress()
        {
            _randomizationRepository.ChangeStatustoConsentInProgress();
            _uow.Save();
            //if (_uow.Save() <= 0) throw new Exception("Updating status failed on save.");
            return Ok();
        }

        [HttpPut]
        [Route("ChangeStatustoWithdrawal")]
        public IActionResult ChangeStatustoWithdrawal([FromBody] FileModel fileModel)
        {
            _randomizationRepository.ChangeStatustoWithdrawal(fileModel);
            _uow.Save();
            return Ok();
        }

        [HttpGet("GetPatientVisits")]
        public IActionResult GetPatientVisits()
        {
            var data = _randomizationRepository.GetPatientVisits();
            return Ok(data);
        }

        [HttpGet("GetPatientTemplates/{screeningVisitId}")]
        public IActionResult GetPatientTemplates(int screeningVisitId)
        {
            var data = _randomizationRepository.GetPatientTemplates(screeningVisitId);
            return Ok(data);
        }

        //[HttpGet("GetRandomizationAndScreeningNumber/{id}")]
        //public IActionResult GetRandomizationAndScreeningNumber(int id)
        //{
        //    var data = _randomizationRepository.GetRandomizationAndScreeningNumber(id);
        //    return Ok(data);
        //}

        [HttpGet("GetRandomizationNumber/{id}")]
        public IActionResult GetRandomizationNumber(int id)
        {
            var data = _randomizationRepository.GetRandomizationNumber(id);
            return Ok(data);
        }

        [HttpGet("GetScreeningNumber/{id}")]
        public IActionResult GetScreeningNumber(int id)
        {
            var data = _randomizationRepository.GetScreeningNumber(id);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetRandomizationDropdown/{projectId}")]
        public IActionResult GetRandomizationDropdown(int projectId)
        {
            return Ok(_randomizationRepository.GetRandomizationDropdown(projectId));
        }
    }
}