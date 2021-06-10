using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.ProjectRight;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using GSC.Respository.Project.Workflow;
using GSC.Shared.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using GSC.Data.Entities.UserMgt;
using GSC.Data.Dto.UserMgt;
using GSC.Shared.Security;
using GSC.Data.Dto.ProjectRight;

namespace GSC.Respository.Attendance
{
    public class RandomizationRepository : GenericRespository<Randomization>, IRandomizationRepository
    {

        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IProjectRepository _projectRepository;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly ISiteTeamRepository _siteTeamRepository;
        private readonly IRoleRepository _roleRepository;
        private readonly IManageSiteRepository _manageSiteRepository;
        private readonly ICentreUserService _centreUserService;
        private readonly IOptions<EnvironmentSetting> _environmentSetting;
        private readonly IScreeningNumberSettingsRepository _screeningNumberSettingsRepository;
        private readonly IRandomizationNumberSettingsRepository _randomizationNumberSettingsRepository;
        private readonly IUnitOfWork _uow;
        public RandomizationRepository(IGSCContext context,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignRepository projectDesignRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            ICityRepository cityRepository,
             IMapper mapper,
             IEmailSenderRespository emailSenderRespository,
            IProjectRepository projectRepository,
            IProjectRightRepository projectRightRepository,
            ISiteTeamRepository siteTeamRepository,
            IRoleRepository roleRepository,
            IManageSiteRepository manageSiteRepository, ICentreUserService centreUserService, IOptions<EnvironmentSetting> environmentSetting,
            IScreeningNumberSettingsRepository screeningNumberSettingsRepository,
            IRandomizationNumberSettingsRepository randomizationNumberSettingsRepository,
            IUnitOfWork uow)
            : base(context)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectDesignRepository = projectDesignRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _stateRepository = stateRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _mapper = mapper;
            _emailSenderRespository = emailSenderRespository;
            _projectRepository = projectRepository;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
            _siteTeamRepository = siteTeamRepository;
            _roleRepository = roleRepository;
            _manageSiteRepository = manageSiteRepository;
            _centreUserService = centreUserService;
            _environmentSetting = environmentSetting;
            _screeningNumberSettingsRepository = screeningNumberSettingsRepository;
            _randomizationNumberSettingsRepository = randomizationNumberSettingsRepository;
            _uow = uow;
        }

        public void SaveRandomizationNumber(Randomization randomization, RandomizationDto randomizationDto)
        {
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            randomizationNumberDto = GenerateRandomizationNumber(randomization.Id);
            if (randomizationNumberDto.IsManualRandomNo == true)
            {
                randomization.RandomizationNumber = randomizationDto.RandomizationNumber;
            }
            else
            {
                randomization.RandomizationNumber = randomizationNumberDto.RandomizationNumber;
            }
            randomization.DateOfRandomization = randomizationDto.DateOfRandomization;

            Update(randomization);
            if (!randomizationNumberDto.IsTestSite)
            {
                if (randomizationNumberDto.IsManualRandomNo == false)
                {
                    int projectidforRandomNo = 0;
                    if (randomizationNumberDto.IsSiteDependentRandomNo == true)
                        projectidforRandomNo = randomizationNumberDto.ProjectId;
                    else
                        projectidforRandomNo = randomizationNumberDto.ParentProjectId;

                    var projectRandom = _context.RandomizationNumberSettings.Where(x => x.ProjectId == projectidforRandomNo).FirstOrDefault();//_projectRepository.Find(projectidforRandomNo);
                    projectRandom.RandomizationNoseries = randomizationNumberDto.RandomizationNoseries + 1;
                    //_projectRepository.Update(projectRandom);
                    _randomizationNumberSettingsRepository.Update(projectRandom);
                }
            }
        }

        public void SaveScreeningNumber(Randomization randomization, RandomizationDto randomizationDto)
        {
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            randomizationNumberDto = GenerateScreeningNumber(randomization.Id);
            if (randomizationNumberDto.IsManualScreeningNo == true)
            {
                randomization.ScreeningNumber = randomizationDto.ScreeningNumber;
            }
            else
            {
                randomization.ScreeningNumber = randomizationNumberDto.ScreeningNumber;
            }
            randomization.DateOfScreening = randomizationDto.DateOfScreening;

            if (randomization.PatientStatusId == ScreeningPatientStatus.PreScreening)
                randomization.PatientStatusId = ScreeningPatientStatus.Screening;

            Update(randomization);
            if (!randomizationNumberDto.IsTestSite)
            {
                if (randomizationNumberDto.IsManualScreeningNo == false)
                {
                    int projectidforscreeningNo = 0;
                    if (randomizationNumberDto.IsSiteDependentScreeningNo == true)
                        projectidforscreeningNo = randomizationNumberDto.ProjectId;
                    else
                        projectidforscreeningNo = randomizationNumberDto.ParentProjectId;

                    var projectSeries = _context.ScreeningNumberSettings.Where(x => x.ProjectId == projectidforscreeningNo).FirstOrDefault();//_projectRepository.Find(projectidforscreeningNo);
                    projectSeries.ScreeningNoseries = randomizationNumberDto.ScreeningNoseries + 1;
                    //_projectRepository.Update(projectSeries);
                    _screeningNumberSettingsRepository.Update(projectSeries);
                }
            }
        }

        //public void SaveRandomization(Randomization randomization, RandomizationDto randomizationDto)
        //{
        //    RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
        //    randomizationNumberDto = GenerateRandomizationAndScreeningNumber(randomization.Id);
        //    if (randomizationNumberDto.IsManualScreeningNo == true)
        //    {
        //        randomization.ScreeningNumber = randomizationDto.ScreeningNumber;
        //    }
        //    else
        //    {
        //        randomization.ScreeningNumber = randomizationNumberDto.ScreeningNumber;
        //    }
        //    randomization.DateOfScreening = randomizationDto.DateOfScreening;
        //    if (randomizationNumberDto.IsManualRandomNo == true)
        //    {
        //        randomization.RandomizationNumber = randomizationDto.RandomizationNumber;
        //    }
        //    else
        //    {
        //        randomization.RandomizationNumber = randomizationNumberDto.RandomizationNumber;
        //    }
        //    randomization.DateOfRandomization = randomizationDto.DateOfRandomization;

        //    if (randomization.PatientStatusId == ScreeningPatientStatus.PreScreening)
        //        randomization.PatientStatusId = ScreeningPatientStatus.Screening;

        //    Update(randomization);
        //    int projectidforRandomNo = 0;
        //    if (randomizationNumberDto.IsSiteDependentRandomNo == true)
        //        projectidforRandomNo = randomizationNumberDto.ProjectId;
        //    else
        //        projectidforRandomNo = randomizationNumberDto.ParentProjectId;
        //    int projectidforscreeningNo = 0;
        //    if (randomizationNumberDto.IsSiteDependentScreeningNo == true)
        //        projectidforscreeningNo = randomizationNumberDto.ProjectId;
        //    else
        //        projectidforscreeningNo = randomizationNumberDto.ParentProjectId;
        //    if (projectidforRandomNo == projectidforscreeningNo)
        //    {
        //        var project = _projectRepository.Find(projectidforscreeningNo);
        //        project.RandomizationNoseries = randomizationNumberDto.RandomizationNoseries + 1;
        //        project.ScreeningNoseries = randomizationNumberDto.ScreeningNoseries + 1;
        //        _projectRepository.Update(project);
        //    }
        //    else
        //    {
        //        var projectRandom = _projectRepository.Find(projectidforRandomNo);
        //        projectRandom.RandomizationNoseries = randomizationNumberDto.RandomizationNoseries + 1;
        //        _projectRepository.Update(projectRandom);
        //        var projectSeries = _projectRepository.Find(projectidforscreeningNo);
        //        projectSeries.ScreeningNoseries = randomizationNumberDto.ScreeningNoseries + 1;
        //        _projectRepository.Update(projectSeries);
        //    }
        //}

        public string ValidateScreeningNumber(RandomizationDto randomization)
        {
            if (!_projectRepository.Find(randomization.ProjectId).IsTestSite)
            {
                RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
                randomizationNumberDto = GenerateScreeningNumber(randomization.Id);
                randomizationNumberDto.ScreeningNumber = randomization.ScreeningNumber;
                if (randomizationNumberDto.IsManualScreeningNo == true)
                {
                    if (randomizationNumberDto.ScreeningNumber.Length != randomizationNumberDto.ScreeningLength)
                    {
                        return "Please add " + randomizationNumberDto.ScreeningLength.ToString() + " characters in Screening Number";
                    }
                }
                return "";
            }
            return "";
        }

        public string ValidateRandomizationNumber(RandomizationDto randomization)
        {
            if (!_projectRepository.Find(randomization.ProjectId).IsTestSite)
            {
                RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
                randomizationNumberDto = GenerateRandomizationNumber(randomization.Id);
                randomizationNumberDto.RandomizationNumber = randomization.RandomizationNumber;
                if (randomizationNumberDto.IsManualRandomNo == true)
                {
                    if (randomizationNumberDto.RandomizationNumber.Length != randomizationNumberDto.RandomNoLength)
                    {
                        return "Please add " + randomizationNumberDto.RandomNoLength.ToString() + " characters in Randomization Number";
                    }
                    //if (randomizationNumberDto.IsAlphaNumRandomNo == true)
                    //{
                    //    if (randomizationNumberDto.RandomizationNumber.All(char.IsDigit))
                    //    {
                    //        return "Please add Prefix " + randomizationNumberDto.PrefixRandomNo + " in Randomization Number";
                    //    }
                    //}
                    //else
                    //{
                    //    if (randomizationNumberDto.RandomizationNumber.Contains(randomizationNumberDto.PrefixRandomNo) == false)
                    //    {
                    //        return "Please add Prefix " + randomizationNumberDto.PrefixRandomNo + " in Randomization Number";
                    //    }
                    //}
                }
                return "";
            }
            return "";
        }

        //public string ValidateRandomizationAndScreeningNumber(RandomizationDto randomization)
        //{
        //    RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
        //    randomizationNumberDto = GenerateRandomizationAndScreeningNumber(randomization.Id);
        //    randomizationNumberDto.ScreeningNumber = randomization.ScreeningNumber;
        //    randomizationNumberDto.RandomizationNumber = randomization.RandomizationNumber;
        //    if (randomizationNumberDto.IsManualRandomNo == true)
        //    {
        //        if (randomizationNumberDto.RandomizationNumber.Length != randomizationNumberDto.RandomNoLength)
        //        {
        //            return "Please add " + randomizationNumberDto.RandomNoLength.ToString() + " characters in Randomization Number";
        //        }
        //        if (randomizationNumberDto.IsAlphaNumRandomNo == true)
        //        {
        //            if (randomizationNumberDto.RandomizationNumber.All(char.IsDigit))
        //            {
        //                return "Please add Prefix " + randomizationNumberDto.PrefixRandomNo + " in Randomization Number";
        //            }
        //        }
        //        else
        //        {
        //            if (randomizationNumberDto.RandomizationNumber.Contains(randomizationNumberDto.PrefixRandomNo) == false)
        //            {
        //                return "Please add Prefix " + randomizationNumberDto.PrefixRandomNo + " in Randomization Number";
        //            }
        //        }
        //    }
        //    if (randomizationNumberDto.IsManualScreeningNo == true)
        //    {
        //        if (randomizationNumberDto.ScreeningNumber.Length != randomizationNumberDto.ScreeningLength)
        //        {
        //            return "Please add " + randomizationNumberDto.ScreeningLength.ToString() + " characters in Screening Number";
        //        }
        //        if (randomizationNumberDto.IsAlphaNumScreeningNo == true)
        //        {
        //            if (randomizationNumberDto.ScreeningNumber.All(char.IsDigit))
        //            {
        //                return "Please add Prefix " + randomizationNumberDto.PrefixScreeningNo + " in Screening Number";
        //            }
        //        }
        //        else
        //        {
        //            if (randomizationNumberDto.ScreeningNumber.Contains(randomizationNumberDto.PrefixScreeningNo) == false)
        //            {
        //                return "Please add Prefix " + randomizationNumberDto.PrefixScreeningNo + " in Screening Number";
        //            }
        //        }
        //    }
        //    return "";
        //}

        public RandomizationNumberDto GenerateRandomizationNumber(int id)
        {
            var randomization = Find(id);
            var site = _projectRepository.Find(randomization.ProjectId);
            var sitedata = _context.RandomizationNumberSettings.Where(x => x.ProjectId == randomization.ProjectId).FirstOrDefault();//_projectRepository.Find(randomization.ProjectId);
            var studydata = _context.RandomizationNumberSettings.Where(x => x.ProjectId == (int)site.ParentProjectId).FirstOrDefault();//_projectRepository.Find((int)sitedata.ParentProjectId);
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            randomizationNumberDto.ProjectId = randomization.ProjectId;
            randomizationNumberDto.ParentProjectId = (int)site.ParentProjectId;
            randomizationNumberDto.IsManualRandomNo = studydata.IsManualRandomNo;
            randomizationNumberDto.IsSiteDependentRandomNo = studydata.IsSiteDependentRandomNo;
            randomizationNumberDto.RandomNoLength = studydata.RandomNoLength;
            //randomizationNumberDto.PrefixRandomNo = studydata.PrefixRandomNo;

            if (studydata.IsManualRandomNo == true)
            {
                randomizationNumberDto.RandomizationNumber = "";
            }
            else
            {
                int latestno;
                if (studydata.IsSiteDependentRandomNo == true)
                {
                    latestno = sitedata.RandomizationNoseries;
                    randomizationNumberDto.RandomizationNoseries = sitedata.RandomizationNoseries;
                    if (string.IsNullOrEmpty(sitedata.PrefixRandomNo))
                        randomizationNumberDto.RandomizationNumber = latestno.ToString().PadLeft((int)studydata.RandomNoLength, '0');
                    else
                        randomizationNumberDto.RandomizationNumber = sitedata.PrefixRandomNo + latestno.ToString().PadLeft((int)studydata.RandomNoLength - 1, '0');
                }
                else
                {
                    latestno = studydata.RandomizationNoseries;
                    randomizationNumberDto.RandomizationNoseries = studydata.RandomizationNoseries;
                    randomizationNumberDto.RandomizationNumber = latestno.ToString().PadLeft((int)studydata.RandomNoLength, '0');
                }
            }
            randomizationNumberDto.IsTestSite = site.IsTestSite;
            if (site.IsTestSite)
            {
                var patientCount = All.Where(x => x.ProjectId == randomization.ProjectId && x.DeletedDate == null && x.RandomizationNumber != null).Count() + 1;
                randomizationNumberDto.RandomizationNumber = "TR -" + patientCount.ToString().PadLeft((int)studydata.RandomNoLength, '0');
                return randomizationNumberDto;
            }
            return randomizationNumberDto;
        }

        public bool IsScreeningFormatSetInStudy(int id)
        {
            var randomization = Find(id);
            var sitedata = _projectRepository.Find(randomization.ProjectId);
            var studydata = _context.ScreeningNumberSettings.Where(x => x.ProjectId == (int)sitedata.ParentProjectId).FirstOrDefault();
            //var studydata = _projectRepository.Find((int)sitedata.ParentProjectId);
            if (studydata.ScreeningLength <= 0)
                return false;
            return true;
        }

        public bool IsRandomFormatSetInStudy(int id)
        {
            var randomization = Find(id);
            var sitedata = _projectRepository.Find(randomization.ProjectId);
            //var studydata = _projectRepository.Find((int)sitedata.ParentProjectId);
            var studydata = _context.RandomizationNumberSettings.Where(x => x.ProjectId == (int)sitedata.ParentProjectId).FirstOrDefault();
            if (studydata.RandomNoLength <= 0)
                return false;
            return true;
        }

        public RandomizationNumberDto GenerateScreeningNumber(int id)
        {
            var randomization = Find(id);
            var site = _projectRepository.Find(randomization.ProjectId);
            var sitedata = _context.ScreeningNumberSettings.Where(x => x.ProjectId == randomization.ProjectId).FirstOrDefault();//_projectRepository.Find(randomization.ProjectId);
            var studydata = _context.ScreeningNumberSettings.Where(x => x.ProjectId == (int)site.ParentProjectId).FirstOrDefault();//_projectRepository.Find((int)sitedata.ParentProjectId);
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            randomizationNumberDto.ProjectId = randomization.ProjectId;
            randomizationNumberDto.ParentProjectId = (int)site.ParentProjectId;
            randomizationNumberDto.IsManualScreeningNo = studydata.IsManualScreeningNo;
            randomizationNumberDto.IsSiteDependentScreeningNo = studydata.IsSiteDependentScreeningNo;
            randomizationNumberDto.ScreeningLength = studydata.ScreeningLength;

            if (studydata.IsManualScreeningNo == true)
            {
                randomizationNumberDto.ScreeningNumber = "";
            }
            else
            {
                int latestno;
                if (studydata.IsSiteDependentScreeningNo == true)
                {
                    latestno = sitedata.ScreeningNoseries;
                    randomizationNumberDto.ScreeningNoseries = sitedata.ScreeningNoseries;
                    if (string.IsNullOrEmpty(sitedata.PrefixScreeningNo))
                        randomizationNumberDto.ScreeningNumber = latestno.ToString().PadLeft((int)studydata.ScreeningLength, '0');
                    else
                        randomizationNumberDto.ScreeningNumber = sitedata.PrefixScreeningNo + latestno.ToString().PadLeft((int)studydata.ScreeningLength - 1, '0');
                }
                else
                {
                    latestno = studydata.ScreeningNoseries;
                    randomizationNumberDto.ScreeningNoseries = studydata.ScreeningNoseries;
                    randomizationNumberDto.ScreeningNumber = latestno.ToString().PadLeft((int)studydata.ScreeningLength, '0');
                }
            }
            randomizationNumberDto.IsTestSite = site.IsTestSite;
            if (site.IsTestSite)
            {
                var patientCount = All.Where(x => x.ProjectId == randomization.ProjectId && x.DeletedDate == null && x.ScreeningNumber != null).Count() + 1;
                randomizationNumberDto.ScreeningNumber = "TS -" + patientCount.ToString().PadLeft((int)studydata.ScreeningLength, '0');
                //  randomizationNumberDto.RandomizationNumber = "TestRnd -" + patientCount.ToString().PadLeft((int)studydata.ScreeningLength, '0');
                return randomizationNumberDto;
            }
            return randomizationNumberDto;
        }


        //public RandomizationNumberDto GenerateRandomizationAndScreeningNumber(int id)
        //{
        //    var randomization = Find(id);
        //    var sitedata = _projectRepository.Find(randomization.ProjectId);
        //    var studydata = _projectRepository.Find((int)sitedata.ParentProjectId);
        //    RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
        //    randomizationNumberDto.ProjectId = randomization.ProjectId;
        //    randomizationNumberDto.ParentProjectId = studydata.Id;
        //    randomizationNumberDto.IsManualRandomNo = studydata.IsManualRandomNo;
        //    randomizationNumberDto.IsManualScreeningNo = studydata.IsManualScreeningNo;
        //    randomizationNumberDto.IsSiteDependentRandomNo = studydata.IsSiteDependentRandomNo;
        //    randomizationNumberDto.IsSiteDependentScreeningNo = studydata.IsSiteDependentScreeningNo;
        //    randomizationNumberDto.RandomNoLength = studydata.RandomNoLength;
        //    randomizationNumberDto.ScreeningLength = studydata.ScreeningLength;
        //    randomizationNumberDto.PrefixRandomNo = studydata.PrefixRandomNo;
        //    randomizationNumberDto.PrefixScreeningNo = studydata.PrefixScreeningNo;
        //    if (studydata.IsManualRandomNo == true)
        //    {
        //        randomizationNumberDto.RandomizationNumber = "";
        //    }
        //    else
        //    {
        //        int latestno;
        //        if (studydata.IsSiteDependentRandomNo == true)
        //        {
        //            latestno = sitedata.RandomizationNoseries;
        //            randomizationNumberDto.RandomizationNoseries = sitedata.RandomizationNoseries;
        //        }
        //        else
        //        {
        //            latestno = studydata.RandomizationNoseries;
        //            randomizationNumberDto.RandomizationNoseries = studydata.RandomizationNoseries;
        //        }
        //        if (studydata.IsAlphaNumRandomNo == true)
        //        {
        //            randomizationNumberDto.RandomizationNumber = studydata.PrefixRandomNo + latestno.ToString().PadLeft((int)studydata.RandomNoLength - 1, '0');
        //        }
        //        else
        //        {
        //            randomizationNumberDto.RandomizationNumber = latestno.ToString().PadLeft((int)studydata.RandomNoLength, '0');
        //        }
        //    }
        //    if (studydata.IsManualScreeningNo == true)
        //    {
        //        randomizationNumberDto.ScreeningNumber = "";
        //    }
        //    else
        //    {
        //        int latestno;
        //        if (studydata.IsSiteDependentScreeningNo == true)
        //        {
        //            latestno = sitedata.ScreeningNoseries;
        //            randomizationNumberDto.ScreeningNoseries = sitedata.ScreeningNoseries;
        //        }
        //        else
        //        {
        //            latestno = studydata.ScreeningNoseries;
        //            randomizationNumberDto.ScreeningNoseries = studydata.ScreeningNoseries;
        //        }
        //        if (studydata.IsAlphaNumScreeningNo == true)
        //        {
        //            randomizationNumberDto.ScreeningNumber = studydata.PrefixScreeningNo + latestno.ToString().PadLeft((int)studydata.ScreeningLength - 1, '0');
        //        }
        //        else
        //        {
        //            randomizationNumberDto.ScreeningNumber = latestno.ToString().PadLeft((int)studydata.ScreeningLength, '0');
        //        }
        //    }
        //    return randomizationNumberDto;
        //}

        public List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted)
        {
            var result = All.Where(x => x.ProjectId == projectId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                  ProjectTo<RandomizationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            var projectright = _projectRightRepository.FindBy(x => x.ProjectId == projectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId).ToList();
            result.ForEach(x =>
            {
                x.PatientStatusName = x.PatientStatusId == null ? "" : x.PatientStatusId.GetDescription();//_patientStatusRepository.Find((int)x.PatientStatusId).StatusName;
                x.IsShowEconsentIcon = x.EconsentReviewDetails.Any(x => x.IsReviewedByPatient == true);
                //if (projectright.Count > 0)
                //{
                //    var EconsentReviewDetails = (from econsentreviewdetails in _context.EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true).ToList()
                //                                 join econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == projectId && x.DeletedDate == null) on econsentreviewdetails.EconsentDocumentId equals econsentsetups.Id
                //                                 join roles in _context.EconsentSetupRoles.Where(a => a.RoleId == _jwtTokenAccesser.RoleId && a.DeletedDate == null) on econsentsetups.Id equals roles.EconsentDocumentId
                //                                 select new EconsentReviewDetailsDto
                //                                 {
                //                                     Id = econsentreviewdetails.Id,
                //                                     EconsentDocumentName = econsentsetups.DocumentName,
                //                                     IsApprovedByInvestigator = econsentreviewdetails.IsApprovedByInvestigator,
                //                                     IsReviewedByPatient = econsentreviewdetails.IsReviewedByPatient,
                //                                     AttendanceId = x.Id,
                //                                     EconsentDocumentId = econsentsetups.Id,
                //                                     patientdigitalSignImagepath = econsentreviewdetails.patientdigitalSignImagepath,
                //                                     pdfpath = econsentreviewdetails.pdfpath,
                //                                     ApprovedByRoleId = econsentreviewdetails.ApprovedByRoleId
                //                                 }
                //                            ).ToList();
                //    if (EconsentReviewDetails.Count > 0)
                //    {
                //        x.IsShowEconsentIcon = true;
                //        x.EconsentReviewDetails = EconsentReviewDetails;
                //        x.IsEconsentReviewPending = EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true && t.IsApprovedByInvestigator == false).ToList().Count > 0 ? true : false;
                //        x.IsmultipleEconsentReviewDetails = EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true).ToList().Count > 1 ? true : false;
                //    }
                //    else
                //    {
                //        x.IsShowEconsentIcon = false;
                //    }
                //}
                //else
                //{
                //    x.IsShowEconsentIcon = false;
                //}
            });

            var projectCode = _context.Project.Find(_context.Project.Find(projectId).ParentProjectId).ProjectCode;
            result.ForEach(x =>
            {
                x.ParentProjectCode = projectCode;
                var screeningTemplate = _screeningTemplateRepository.FindByInclude(y => y.ScreeningVisit.ScreeningEntry.RandomizationId == x.Id && y.DeletedDate == null).ToList();
                x.IsLocked = screeningTemplate.Count() <= 0 || screeningTemplate.Any(y => y.IsLocked == false) ? false : true;
            });

            return result;
        }

        public string Duplicate(RandomizationDto objSave, int projectId)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.ScreeningNumber == objSave.ScreeningNumber &&
                x.ProjectId == projectId &&
                x.DeletedDate == null)) return "Duplicate Screening Number : " + objSave.ScreeningNumber;

            if (All.Any(x =>
                x.Id != objSave.Id && x.RandomizationNumber == objSave.RandomizationNumber &&
                x.ProjectId == projectId && !string.IsNullOrEmpty(x.RandomizationNumber) &&
                x.DeletedDate == null)) return "Duplicate Randomization Number : " + objSave.RandomizationNumber;

            return "";
        }

        public async Task SendEmailOfScreenedtoPatient(Randomization randomization, int sendtype)
        {
            var studyId = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var studydata = _projectRepository.Find((int)studyId);
            if (studydata.IsSendSMS == true || studydata.IsSendEmail == true)
            {
                var userdata = _userRepository.Find((int)randomization.UserId);
                //var userotp = _userOtpRepository.All.Where(x => x.UserId == userdata.Id).ToList().FirstOrDefault();
                var userotp = await _centreUserService.GetUserOtpDetails($"{_environmentSetting.Value.CentralApi}UserOtp/GetuserOtpDetails/{userdata.Id}");
                await _emailSenderRespository.SendEmailOfScreenedPatient(randomization.Email, randomization.ScreeningNumber + " " + randomization.Initial, userdata.UserName, userotp.Otp, studydata.ProjectCode, randomization.PrimaryContactNumber, sendtype, studydata.IsSendEmail, studydata.IsSendSMS);
            }
        }

        public void SendEmailOfStartEconsent(Randomization randomization)
        {
            //var projectname = _projectRepository.Find(randomization.ProjectId).ProjectCode;
            var studyid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var study = _projectRepository.Find((int)studyid);
            var Econsentdocuments = (from econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == study.Id && x.LanguageId == randomization.LanguageId && x.DeletedDate == null)
                                     join status in _context.EconsentSetupPatientStatus.Where(a => a.PatientStatusId == (int)randomization.PatientStatusId && a.DeletedDate == null) on econsentsetups.Id equals status.EconsentDocumentId
                                     select new EconsentSetup
                                     {
                                         Id = econsentsetups.Id,
                                         DocumentName = econsentsetups.DocumentName
                                     }).ToList();
            string documentname = "";
            for (var i = 0; i < Econsentdocuments.Count; i++)
            {
                documentname = documentname + ((i == 0) ? "" : " , ") + Econsentdocuments[i].DocumentName;
            }
            if (Econsentdocuments.Count > 0)
            {
                _emailSenderRespository.SendEmailOfStartEconsent(randomization.Email, randomization.ScreeningNumber + " " + randomization.Initial, documentname, study.ProjectCode);
            }
        }


        public void ChangeStatustoConsentInProgress()
        {

            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            var studyid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            if (randomization.PatientStatusId != ScreeningPatientStatus.ConsentInProcess && randomization.PatientStatusId != ScreeningPatientStatus.ReConsentInProcess)
            {
                var Econsentdocuments = (from econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == (int)studyid && x.LanguageId == randomization.LanguageId && x.DeletedDate == null)
                                         join status in _context.EconsentSetupPatientStatus.Where(a => a.PatientStatusId == (int)randomization.PatientStatusId && a.DeletedDate == null) on econsentsetups.Id equals status.EconsentDocumentId
                                         select new EconsentSetup
                                         {
                                             Id = econsentsetups.Id,
                                             DocumentName = econsentsetups.DocumentName
                                         }).ToList();
                if (Econsentdocuments.Count > 0)
                {
                    for (var i = 0; i < Econsentdocuments.Count; i++)
                    {
                        if (_context.EconsentReviewDetails.Where(x => x.RandomizationId == randomization.Id && x.EconsentSetupId == Econsentdocuments[i].Id).ToList().Count <= 0)
                        {
                            EconsentReviewDetails econsentReviewDetails = new EconsentReviewDetails();
                            econsentReviewDetails.RandomizationId = randomization.Id;
                            econsentReviewDetails.EconsentSetupId = Econsentdocuments[i].Id;
                            econsentReviewDetails.IsReviewedByPatient = false;
                            _context.EconsentReviewDetails.Add(econsentReviewDetails);
                            _uow.Save();
                        }
                    }
                    randomization.PatientStatusId = ScreeningPatientStatus.ConsentInProcess;
                    Update(randomization);
                }
                
            }
        }

        public void ChangeStatustoConsentCompleted(int id)
        {
            var randomization = Find(id);
            var studyid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            if (randomization.PatientStatusId == ScreeningPatientStatus.ConsentInProcess || randomization.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess)
            {
                //if (_context.EconsentReviewDetails.Where(x => x.RandomizationId == id && x.IsReviewDoneByInvestigator == false).ToList().Count > 0)
                //{
                //}
                //else
                //{
                //    var Econsentdocuments = (from econsentsetups in _context.EconsentSetup.Where(x => x.DeletedDate == null && x.LanguageId == randomization.LanguageId && x.ProjectId == (int)studyid).ToList()
                //                             join doc in _context.EconsentReviewDetails.Where(x => x.RandomizationId == id && x.IsReviewedByPatient == true && x.IsReviewDoneByInvestigator == true).ToList() on econsentsetups.Id equals doc.EconsentSetupId into ps
                //                             from p in ps.DefaultIfEmpty()
                //                             select new EconsentSetup
                //                             {
                //                                 Id = (p == null) ? 0 : econsentsetups.Id
                //                             }).ToList();


                //    if (Econsentdocuments.Where(x => x.Id == 0).ToList().Count > 0)
                //    {
                //    }
                //    else
                //    {
                //        randomization.PatientStatusId = ScreeningPatientStatus.ConsentCompleted;
                //        Update(randomization);
                //        _context.Save();
                //    }
                //}

                if(_context.EconsentReviewDetails.Where(x => x.RandomizationId == id && x.IsReviewDoneByInvestigator == false).Count() == 0)
                {
                    randomization.PatientStatusId = ScreeningPatientStatus.ConsentCompleted;
                     Update(randomization);
                    _context.Save();
                }
            }
        }

        public void ChangeStatustoReConsentInProgress(int id)
        {
            var randomization = Find(id);
            if (randomization.PatientStatusId == ScreeningPatientStatus.ConsentCompleted || randomization.PatientStatusId == ScreeningPatientStatus.OnTrial)
            {
                randomization.PatientStatusId = ScreeningPatientStatus.ReConsentInProcess;
                Update(randomization);
                _context.Save();
            }
        }


        public async Task PatientStatus(ScreeningPatientStatus patientStatus, int screeningEntryId)
        {
            var randomization = All.AsNoTracking().Where(x => x.ScreeningEntry.Id == screeningEntryId).FirstOrDefault();
            if (randomization.PatientStatusId != patientStatus)
            {
                randomization.PatientStatusId = patientStatus;
                Update(randomization);
                if (patientStatus == ScreeningPatientStatus.ScreeningFailure || patientStatus == ScreeningPatientStatus.Withdrawal)
                {
                    if (!_environmentSetting.Value.IsPremise)
                    {
                        int userId = (int)randomization.UserId;
                        User user = new User();
                        user = _userRepository.Find(userId);
                        user.ValidTo = DateTime.Today.AddDays(-1);
                        _userRepository.Update(user);

                        user = await _centreUserService.GetUserData($"{_environmentSetting.Value.CentralApi}Login/GetUserData/{user.UserName}");
                        user.ValidTo = DateTime.Today.AddDays(-1);
                        var userDto = _mapper.Map<UserDto>(user);
                        CommonResponceView userdetails = await _centreUserService.UpdateUser(userDto, _environmentSetting.Value.CentralApi);
                    }
                }
            }

        }        
        public void ChangeStatustoWithdrawal()
        {
            //public void ChangeStatustoWithdrawal(FileModel fileModel)
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            //if (randomization.PatientStatusId == ScreeningPatientStatus.ConsentInProcess || randomization.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess)
            //{
                //if (fileModel.Base64?.Length > 0)
                //{
                //    randomization.WithdrawSignaturePath = new ImageService().ImageSave(fileModel,
                //        _context.UploadSetting.FirstOrDefault().ImagePath, FolderType.InformConcent);
                //}
                randomization.PatientStatusId = ScreeningPatientStatus.Withdrawal;
                Update(randomization);
            //}
        }

        public DashboardPatientDto GetDashboardPatientDetail()
        {
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization != null)
            {
                var project = _context.Project.Where(x => x.Id == randomization.ProjectId).ToList().FirstOrDefault();
                var parentproject = _context.Project.Where(x => x.Id == project.ParentProjectId).ToList().FirstOrDefault();
                var investigator = _context.InvestigatorContact.Where(x => x.Id == project.InvestigatorContactId).ToList().FirstOrDefault();
                DashboardPatientDto dashboardPatientDto = new DashboardPatientDto();
                dashboardPatientDto.projectId = project.Id;
                dashboardPatientDto.studycode = parentproject.ProjectCode;
                dashboardPatientDto.studyname = parentproject.ProjectName;
                dashboardPatientDto.sitecode = project.ProjectCode;
                dashboardPatientDto.sitename = project.ProjectName;
                dashboardPatientDto.patientStatusId = (int)randomization.PatientStatusId;
                dashboardPatientDto.patientStatus = randomization.PatientStatusId.GetDescription();
                var siteteams = _siteTeamRepository.FindBy(x => x.ProjectId == randomization.ProjectId && x.DeletedDate == null).ToList();
                var siteteamdtos = _mapper.Map<List<SiteTeamDto>>(siteteams);
                siteteamdtos.ForEach(x =>
                {
                    x.ContactEmail = _userRepository.Find(x.UserId).Email;
                    x.ContactMobile = _userRepository.Find(x.UserId).Phone;
                    x.UserName = _userRepository.Find(x.UserId).FirstName + " " + _userRepository.Find(x.UserId).LastName;
                    x.Role = _roleRepository.Find(x.RoleId).RoleName;
                    x.UserPicUrl = _context.UploadSetting.FirstOrDefault().ImageUrl + (_userRepository.Find(x.UserId).ProfilePic ?? DocumentService.DefulatProfilePic);
                }
                    );
                dashboardPatientDto.siteTeams = siteteamdtos;
                if (project.ManageSiteId != null)
                {
                    dashboardPatientDto.hospitalName = _manageSiteRepository.Find((int)project.ManageSiteId).SiteName;

                    dashboardPatientDto.siteAddress = _manageSiteRepository.Find((int)project.ManageSiteId).SiteAddress;
                }
                dashboardPatientDto.patientdetail = "Screening Number: " + randomization.ScreeningNumber + " Initial: " + randomization.Initial;
                //dashboardPatientDto.investigatorName = investigator.NameOfInvestigator;
                //dashboardPatientDto.investigatorcontact = investigator.ContactNumber;
                //dashboardPatientDto.investigatorEmail = investigator.EmailOfInvestigator;
                return dashboardPatientDto;
            }
            else
            {
                return new DashboardPatientDto();
            }

        }

        public List<ProjectDesignVisitMobileDto> GetPatientVisits()
        {
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization == null) return new List<ProjectDesignVisitMobileDto>();
            //string sqlquery = @"select d.Id,d.DisplayName
            //                    from
            //                    Randomization a,
            //                    screeningentry b,
            //                    ScreeningVisit c,
            //                    ProjectDesignVisit d
            //                    where
            //                    a.Id = " + randomization.Id + @" and
            //                    a.Id = b.RandomizationId and
            //                    b.Id = c.ScreeningEntryId and
            //                    c.ProjectDesignVisitId = d.Id";

            //var data = (from screeningentry in _context.ScreeningEntry.Where(x => x.RandomizationId == randomization.Id)
            //            join ScreeningVisit in _context.ScreeningVisit.Where(x => x.DeletedDate == null) on screeningentry.Id equals ScreeningVisit.ScreeningEntryId
            //            join ProjectDesignVisit in _context.ProjectDesignVisit.Where(x => x.DeletedDate == null) on ScreeningVisit.ProjectDesignVisitId equals ProjectDesignVisit.Id

            //            select new ProjectDesignVisitMobileDto
            //            {
            //                Id = ScreeningVisit.Id,
            //                DisplayName = ProjectDesignVisit.DisplayName,
            //            }).ToList();

            var data = _context.ScreeningVisit.Include(x => x.ScreeningEntry).Include(x => x.ProjectDesignVisit).Include(x => x.ScreeningTemplates).
                        Where(x => x.ScreeningEntry.RandomizationId == randomization.Id && (int)x.Status >= 4 && x.DeletedDate == null && x.ProjectDesignVisit.DeletedDate == null && x.ScreeningTemplates.Any(x => x.ProjectDesignTemplate.IsParticipantView == true)).
                        Select(r => new ProjectDesignVisitMobileDto
                        {
                            Id = r.Id,
                            DisplayName = ((_jwtTokenAccesser.Language != null && _jwtTokenAccesser.Language != 1) ?
                r.ProjectDesignVisit.VisitLanguage.Where(x => x.LanguageId == (int)_jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : r.ProjectDesignVisit.DisplayName) //r.ProjectDesignVisit.DisplayName,
                        }).ToList();

            return data;
        }

        public List<ProjectDesignTemplateMobileDto> GetPatientTemplates(int screeningVisitId)
        {
            var data = _context.ScreeningTemplate.Include(x => x.ProjectDesignTemplate).Include(x => x.ScreeningVisit).Where(x => x.ScreeningVisitId == screeningVisitId && x.ProjectDesignTemplate.IsParticipantView == true).
                        Select(r => new ProjectDesignTemplateMobileDto
                        {
                            ScreeningTemplateId = r.Id,
                            ProjectDesignTemplateId = r.ProjectDesignTemplateId,
                            ProjectDesignVisitId = r.ScreeningVisit.ProjectDesignVisitId,
                            TemplateName = ((_jwtTokenAccesser.Language != null && _jwtTokenAccesser.Language != 1) ?
                r.ProjectDesignTemplate.TemplateLanguage.Where(x => x.DeletedDate == null && x.LanguageId == (int)_jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : r.ProjectDesignTemplate.TemplateName),// r.ProjectDesignTemplate.TemplateName,
                            Status = r.Status,
                            DesignOrder = r.ProjectDesignTemplate.DesignOrder,
                            ScheduleDate = r.ScheduleDate,
                            IsTemplateRestricted = false,
                            IsPastTemplate = false
                        }).OrderBy(r => r.DesignOrder).ToList();
            data.ForEach(x =>
            {
                if (x.Status == ScreeningTemplateStatus.Submitted)
                {
                    x.SubmittedDate = _context.ScreeningTemplateReview.Where(t => t.ScreeningTemplateId == x.ScreeningTemplateId && t.Status == ScreeningTemplateStatus.Submitted).ToList().FirstOrDefault().CreatedDate;
                }
                if (x.ScheduleDate != null)
                {
                    var ProjectScheduleTemplates = _context.ProjectScheduleTemplate.Where(t => t.ProjectDesignTemplateId == x.ProjectDesignTemplateId && t.ProjectDesignVisitId == x.ProjectDesignVisitId && t.DeletedDate == null);
                    var noofday = ProjectScheduleTemplates.Min(t => t.NoOfDay);
                    var ProjectScheduleTemplate = ProjectScheduleTemplates.Where(x => x.NoOfDay == noofday).FirstOrDefault();
                    var mindate = ((DateTime)x.ScheduleDate).AddDays(ProjectScheduleTemplate.NegativeDeviation * -1);
                    var maxdate = ((DateTime)x.ScheduleDate).AddDays(ProjectScheduleTemplate.PositiveDeviation);
                    if (DateTime.Today >= mindate && DateTime.Today <= maxdate)
                        x.IsTemplateRestricted = false;
                    else
                    {
                        x.IsTemplateRestricted = true;
                        if (DateTime.Today > maxdate)
                            x.IsPastTemplate = true;
                    }
                }
            });
            return data;
        }

        //public RandomizationNumberDto GetRandomizationAndScreeningNumber(int id)
        //{
        //    return GenerateRandomizationAndScreeningNumber(id);
        //}
        //for (int i = 0; i < data.Count; i++)
        //{
        //    if (data[i].ScheduleDate != null)
        //    {
        //        var ProjectScheduleTemplates = _context.ProjectScheduleTemplate.Where(t => t.ProjectDesignTemplateId == data[i].ProjectDesignTemplateId && t.ProjectDesignVisitId == data[i].ProjectDesignVisitId);
        //        var noofday = ProjectScheduleTemplates.Min(t => t.NoOfDay);
        //        var ProjectScheduleTemplate = ProjectScheduleTemplates.Where(x => x.NoOfDay == noofday).FirstOrDefault();
        //        var mindate = ((DateTime)data[i].ScheduleDate).AddDays(ProjectScheduleTemplate.NegativeDeviation * -1);
        //        var maxdate = ((DateTime)data[i].ScheduleDate).AddDays(ProjectScheduleTemplate.PositiveDeviation);
        //        if (DateTime.Today >= mindate && DateTime.Today <= maxdate)
        //            data[i].IsTemplateRestricted = false;
        //        else
        //            data[i].IsTemplateRestricted = true;
        //    }
        //}
        public RandomizationNumberDto GetRandomizationNumber(int id)
        {
            return GenerateRandomizationNumber(id);
        }
        public RandomizationNumberDto GetScreeningNumber(int id)
        {
            return GenerateScreeningNumber(id);
        }

        public List<DropDownDto> GetRandomizationDropdown(int projectid)
        {
            var ParentProject = _context.Project.FirstOrDefault(x => x.Id == projectid).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == projectid).ToList().Select(x => x.Id).ToList();

            return All.Where(a => a.DeletedDate == null && ParentProject != null ? a.ProjectId == projectid : sites.Contains(a.ProjectId))
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = Convert.ToString(x.ScreeningNumber + " - " +
                                           x.Initial +
                                           (x.RandomizationNumber == null
                                               ? ""
                                               : " - " + x.RandomizationNumber))
                }).Distinct().ToList();
        }

        // Dashboard chart for Subject Status
        public List<DashboardQueryStatusDto> GetSubjectStatus(int projectId)
        {
            var result = All.Where(x => (x.ProjectId == projectId ||
           x.Project.ParentProjectId == projectId) && x.DeletedDate == null).GroupBy(
               t => new { t.PatientStatusId }).Select(g => new DashboardQueryStatusDto
               {
                   DisplayName = g.Key.PatientStatusId.GetDescription(),
                   Total = g.Count()
               }).ToList();
            return result;

        }
    }
}