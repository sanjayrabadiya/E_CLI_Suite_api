﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
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
using GSC.Shared.Configuration;
using Microsoft.Extensions.Options;
using System.Threading.Tasks;
using GSC.Data.Entities.UserMgt;
using GSC.Data.Dto.UserMgt;
using GSC.Shared.Security;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.Medra;
using System.Globalization;
using GSC.Respository.InformConcent;
using GSC.Respository.SupplyManagement;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Data.Entities.Master;
using System.Security.Policy;


namespace GSC.Respository.Attendance
{
    public class RandomizationRepository : GenericRespository<Randomization>, IRandomizationRepository
    {

        private readonly IUserRepository _userRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
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
        private readonly IEconsentReviewDetailsAuditRepository _econsentReviewDetailsAuditRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ISupplyManagementFectorRepository _supplyManagementFectorRepository;
        private readonly ISupplyManagementKitRepository _supplyManagementKITRepository;
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
            IUnitOfWork uow, IUserRoleRepository userRoleRepository,
            IAppSettingRepository appSettingRepository, IUploadSettingRepository uploadSettingRepository, IEconsentReviewDetailsAuditRepository econsentReviewDetailsAuditRepository,
            ISupplyManagementFectorRepository supplyManagementFectorRepository,
            ISupplyManagementKitRepository supplyManagementKITRepository
            )
            : base(context)
        {
            _userRepository = userRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
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
            _econsentReviewDetailsAuditRepository = econsentReviewDetailsAuditRepository;
            _userRoleRepository = userRoleRepository;
            _supplyManagementFectorRepository = supplyManagementFectorRepository;
            _supplyManagementKITRepository = supplyManagementKITRepository;
        }

        public void SaveRandomizationNumber(Randomization randomization, RandomizationDto randomizationDto)
        {
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            var numerformate = _context.RandomizationNumberSettings.Where(x => x.ProjectId == randomizationDto.ParentProjectId).FirstOrDefault();
            if (numerformate != null && numerformate.IsIGT)
            {
                randomization.RandomizationNumber = randomizationDto.RandomizationNumber;
                if (!string.IsNullOrEmpty(randomizationDto.ProductCode))
                    randomization.ProductCode = randomizationDto.ProductCode;
            }
            else
            {
                randomizationNumberDto = GenerateRandomizationNumber(randomization.Id);
                if (randomizationNumberDto.IsManualRandomNo == true)
                    randomization.RandomizationNumber = randomizationDto.RandomizationNumber;
                else
                    randomization.RandomizationNumber = randomizationNumberDto.RandomizationNumber;
            }

            randomization.DateOfRandomization = randomizationDto.DateOfRandomization;
            randomization.StudyVersion = randomizationDto.StudyVersion;

            Update(randomization);
            if (!randomizationNumberDto.IsTestSite && !randomizationNumberDto.IsIGT && randomizationNumberDto.IsManualRandomNo == false)
            {
                int projectidforRandomNo = 0;
                if (randomizationNumberDto.IsSiteDependentRandomNo == true)
                    projectidforRandomNo = randomizationNumberDto.ProjectId;
                else
                    projectidforRandomNo = randomizationNumberDto.ParentProjectId;

                var projectRandom = _context.RandomizationNumberSettings.Where(x => x.ProjectId == projectidforRandomNo).First();
                projectRandom.RandomizationNoseries = randomizationNumberDto.RandomizationNoseries + 1;
                _randomizationNumberSettingsRepository.Update(projectRandom);
            }
        }

        public void SaveScreeningNumber(Randomization randomization, RandomizationDto randomizationDto)
        {
            var randomizationNumberDto = GenerateScreeningNumber(randomization.Id);
            if (randomizationNumberDto.IsManualScreeningNo == true)
                randomization.ScreeningNumber = randomizationDto.ScreeningNumber;
            else
                randomization.ScreeningNumber = randomizationNumberDto.ScreeningNumber;

            randomization.DateOfScreening = randomizationDto.DateOfScreening;
            if (randomization.PatientStatusId == ScreeningPatientStatus.PreScreening || randomization.PatientStatusId == ScreeningPatientStatus.ConsentCompleted)
                randomization.PatientStatusId = ScreeningPatientStatus.Screening;

            randomization.StudyVersion = randomizationDto.StudyVersion;
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

                    var projectSeries = _context.ScreeningNumberSettings.First(x => x.ProjectId == projectidforscreeningNo);
                    projectSeries.ScreeningNoseries = randomizationNumberDto.ScreeningNoseries + 1;
                    _screeningNumberSettingsRepository.Update(projectSeries);
                }
            }
        }

        public string ValidateScreeningNumber(RandomizationDto randomization)
        {
            if (!_projectRepository.Find(randomization.ProjectId).IsTestSite)
            {
                var randomizationNumberDto = GenerateScreeningNumber(randomization.Id);
                randomizationNumberDto.ScreeningNumber = randomization.ScreeningNumber;
                if (randomizationNumberDto.IsManualScreeningNo == true && randomizationNumberDto.ScreeningNumber.Length != randomizationNumberDto.ScreeningLength)
                {
                    return "Please enter the number as length of " + randomizationNumberDto.ScreeningLength.ToString();
                }
                return "";
            }
            return "";
        }

        public string ValidateRandomizationNumber(RandomizationDto randomization)
        {
            var numerformate = _context.RandomizationNumberSettings.Where(x => x.ProjectId == randomization.ParentProjectId).FirstOrDefault();
            if (numerformate != null && numerformate.IsIGT)
            {
                return "";
            }

            if (!_projectRepository.Find(randomization.ProjectId).IsTestSite)
            {
                var randomizationNumberDto = GenerateRandomizationNumber(randomization.Id);
                randomizationNumberDto.RandomizationNumber = randomization.RandomizationNumber;
                if (randomizationNumberDto.IsManualRandomNo == true && randomizationNumberDto.RandomizationNumber.Length != randomizationNumberDto.RandomNoLength)
                {
                    return "Please add " + randomizationNumberDto.RandomNoLength.ToString() + " characters in Randomization Number";
                }
                return "";
            }
            return "";
        }

        public RandomizationNumberDto GenerateRandomizationNumber(int id)
        {
            var randomization = Find(id);
            var site = _projectRepository.Find(randomization.ProjectId);
            var sitedata = _context.RandomizationNumberSettings.First(x => x.ProjectId == randomization.ProjectId);
            var studydata = _context.RandomizationNumberSettings.First(x => x.ProjectId == (int)site.ParentProjectId);
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            randomizationNumberDto.ProjectId = randomization.ProjectId;
            randomizationNumberDto.ParentProjectId = (int)site.ParentProjectId;
            randomizationNumberDto.IsManualRandomNo = studydata.IsManualRandomNo;
            randomizationNumberDto.IsSiteDependentRandomNo = studydata.IsSiteDependentRandomNo;
            randomizationNumberDto.RandomNoLength = studydata.RandomNoLength;
            randomizationNumberDto.IsIWRS = studydata.IsIWRS;
            randomizationNumberDto.IsIGT = studydata.IsIGT;
            randomizationNumberDto.PrefixRandomNo = studydata.PrefixRandomNo;

            if (!studydata.IsIGT)
            {
                if (studydata.IsManualRandomNo)
                {
                    randomizationNumberDto.RandomizationNumber = "";
                }
                else
                {
                    int latestno;
                    if (studydata.IsSiteDependentRandomNo)
                    {
                        latestno = sitedata.RandomizationNoseries;
                        randomizationNumberDto.RandomizationNoseries = sitedata.RandomizationNoseries;
                        if (string.IsNullOrEmpty(sitedata.PrefixRandomNo))
                            randomizationNumberDto.RandomizationNumber = latestno.ToString().PadLeft(studydata.RandomNoLength, '0');
                        else
                            randomizationNumberDto.RandomizationNumber = sitedata.PrefixRandomNo + latestno.ToString().PadLeft(studydata.RandomNoLength, '0');
                    }
                    else
                    {
                        latestno = studydata.RandomizationNoseries;
                        randomizationNumberDto.RandomizationNoseries = studydata.RandomizationNoseries;
                        randomizationNumberDto.RandomizationNumber = latestno.ToString().PadLeft(studydata.RandomNoLength, '0');
                    }
                }
                randomizationNumberDto.IsTestSite = site.IsTestSite;
                if (site.IsTestSite)
                {
                    var patientCount = All.Where(x => x.ProjectId == randomization.ProjectId && x.DeletedDate == null && x.RandomizationNumber != null).Count() + 1;
                    randomizationNumberDto.RandomizationNumber = "TR -" + patientCount.ToString().PadLeft(studydata.RandomNoLength, '0');
                    return randomizationNumberDto;
                }
            }
            else
            {
                var result = _supplyManagementFectorRepository.ValidateSubjecWithFactor(randomization);
                if (result != null)
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        randomizationNumberDto.ErrorMessage = result.ErrorMessage;
                        return randomizationNumberDto;
                    }
                    if (!string.IsNullOrEmpty(result.Result))
                    {
                        randomizationNumberDto.ErrorMessage = result.Result;
                        return randomizationNumberDto;
                    }
                    if (string.IsNullOrEmpty(result.ErrorMessage) || string.IsNullOrEmpty(result.Result))
                    {
                        randomizationNumberDto = GetRandNoIWRS(studydata.ProjectId, randomization.ProjectId, site.ManageSiteId, result.ProductType, randomizationNumberDto, randomization, studydata.IsIWRS);
                        if (!string.IsNullOrEmpty(randomizationNumberDto.RandomizationNumber))
                        {
                            if (!randomizationNumberDto.IsStaticRandomizationNo && !string.IsNullOrEmpty(randomizationNumberDto.PrefixRandomNo.Trim()))
                            {
                                randomizationNumberDto.RandomizationNumber = randomizationNumberDto.PrefixRandomNo.Trim() + randomizationNumberDto.RandomizationNumber;
                            }
                            if (randomizationNumberDto.IsStaticRandomizationNo)
                            {
                                randomizationNumberDto.RandomizationNumber = randomizationNumberDto.DisplayRandomizationNumber;
                            }
                        }
                    }
                }

            }
            return randomizationNumberDto;
        }
        public RandomizationNumberDto GetRandNoIWRS(int projectid, int siteId, int? countryId, string productType, RandomizationNumberDto randomizationNumberDto, Randomization randomization, bool isIwrs)
        {
            List<SupplyManagementKITDetail> kitdata = new List<SupplyManagementKITDetail>();
            List<SupplyManagementKITSeriesDetail> kitSequencedata = new List<SupplyManagementKITSeriesDetail>();
            SupplyManagementUploadFileDetail uploaddetail = new SupplyManagementUploadFileDetail();

            var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.ProjectId == projectid && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
            if (SupplyManagementUploadFile == null)
            {
                randomizationNumberDto.ErrorMessage = "Please upload or approve randomization sheet!";
                return randomizationNumberDto;

            }
            var supplyManagementKitNumberSettings = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == projectid && x.DeletedDate == null).FirstOrDefault();
            if (supplyManagementKitNumberSettings == null)
            {
                randomizationNumberDto.ErrorMessage = "Please set first kit number setting!";
                return randomizationNumberDto;
            }
            if (supplyManagementKitNumberSettings.IsStaticRandomizationNo == null)
                randomizationNumberDto.IsStaticRandomizationNo = false;
            else
                randomizationNumberDto.IsStaticRandomizationNo = (bool)supplyManagementKitNumberSettings.IsStaticRandomizationNo;
            var project = _context.Project.Where(s => s.Id == siteId).FirstOrDefault();
            if (project != null && (project.Status == Helper.MonitoringSiteStatus.CloseOut || project.Status == Helper.MonitoringSiteStatus.Terminated || project.Status == Helper.MonitoringSiteStatus.OnHold || project.Status == Helper.MonitoringSiteStatus.Rejected))
            {
                randomizationNumberDto.ErrorMessage = "Selected site is " + project.Status.GetDescription() + "!";
                return randomizationNumberDto;
            }

            if (isIwrs)
            {
                if (supplyManagementKitNumberSettings.IsUploadWithKit)
                {
                    var visits = _context.SupplyManagementUploadFileVisit
                                   .Include(x => x.SupplyManagementUploadFileDetail)
                                   .ThenInclude(x => x.SupplyManagementUploadFile)
                                   .Where(x => x.DeletedDate == null
                                   && x.SupplyManagementUploadFileDetail.DeletedDate == null
                                   && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.DeletedDate == null
                                   && x.SupplyManagementUploadFileDetail.RandomizationId == null
                                   && x.SupplyManagementUploadFileDetail.SupplyManagementKITSeriesId != null
                                   && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve
                                   && x.Isfirstvisit == true).OrderBy(x => x.Id).ToList();
                    if (visits == null || visits.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }


                    visits = visits.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).ToList();


                    if (visits == null || visits.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }

                    if (!string.IsNullOrEmpty(productType))
                    {
                        var productarray = productType.Split(',').ToArray();

                        kitSequencedata = _context.SupplyManagementKITSeriesDetail
                            .Include(x => x.SupplyManagementKITSeries)
                            .ThenInclude(x => x.SupplyManagementShipment)
                            .ThenInclude(x => x.SupplyManagementRequest)
                            .Include(x => x.PharmacyStudyProductType)
                            .ThenInclude(x => x.ProductType)
                            .Where(x =>
                                            x.DeletedDate == null
                                            && x.ProjectDesignVisitId == visits.FirstOrDefault().ProjectDesignVisitId
                                            && x.SupplyManagementKITSeries.ProjectId == projectid
                                            && productarray.Contains(x.PharmacyStudyProductType.ProductType.ProductTypeCode)
                                            && x.SupplyManagementKITSeries.SupplyManagementShipmentId != null
                                            && !x.SupplyManagementKITSeries.IsRetension
                                            && x.SupplyManagementKITSeries.DeletedDate == null
                                            && x.SupplyManagementKITSeries.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                            && (x.SupplyManagementKITSeries.Status == KitStatus.WithIssue || x.SupplyManagementKITSeries.Status == KitStatus.WithoutIssue)
                                            && x.RandomizationId == null).OrderBy(x => x.Id).ToList();

                        if (kitSequencedata == null || kitSequencedata.Count == 0)
                        {
                            randomizationNumberDto.ErrorMessage = "Kit is not available!";
                            return randomizationNumberDto;
                        }

                        randomizationNumberDto.KitCount = kitSequencedata.Select(x => x.SupplyManagementKITSeriesId).Distinct().Count();

                        foreach (var visititem in visits)
                        {
                            if (productarray.Contains(visititem.Value.Trim()) && string.IsNullOrEmpty(randomizationNumberDto.KitNo))
                            {
                                randomizationNumberDto.RandomizationNumber = Convert.ToString(visititem.SupplyManagementUploadFileDetail.RandomizationNo);
                                randomizationNumberDto.ProductCode = visititem.Value;
                                randomizationNumberDto.DisplayRandomizationNumber = visititem.SupplyManagementUploadFileDetail.DisplayRandomizationNumber;
                                var kit = kitSequencedata.Where(x => x.SupplyManagementKITSeries.KitNo.ToLower() == visititem.SupplyManagementUploadFileDetail.KitNo.ToLower()).OrderBy(x => x.Id).FirstOrDefault();

                                if (kit != null)
                                {
                                    var currentdate = DateTime.Now.Date;
                                    var kitExpiryDate = kit.SupplyManagementKITSeries.KitExpiryDate;
                                    if (Convert.ToDateTime(kitExpiryDate).Date >= currentdate.Date)
                                    {
                                        randomizationNumberDto.KitNo = kit.SupplyManagementKITSeries.KitNo;
                                        randomizationNumberDto.KitDetailId = kit.Id;
                                        randomizationNumberDto.VisitId = visits.FirstOrDefault().ProjectDesignVisitId;
                                    }
                                }
                            }
                        }
                        if (string.IsNullOrEmpty(randomizationNumberDto.RandomizationNumber) && kitSequencedata.Count > 0)
                        {
                            randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                            return randomizationNumberDto;

                        }

                        return randomizationNumberDto;
                    }
                    else
                    {

                        kitSequencedata = _context.SupplyManagementKITSeriesDetail
                            .Include(x => x.SupplyManagementKITSeries)
                            .ThenInclude(x => x.SupplyManagementShipment)
                            .ThenInclude(x => x.SupplyManagementRequest)
                            .Include(x => x.PharmacyStudyProductType)
                            .ThenInclude(x => x.ProductType)
                            .Where(x =>
                                            x.DeletedDate == null
                                            && x.ProjectDesignVisitId == visits.FirstOrDefault().ProjectDesignVisitId
                                            && x.SupplyManagementKITSeries.ProjectId == projectid
                                            && x.SupplyManagementKITSeries.DeletedDate == null
                                            && !x.SupplyManagementKITSeries.IsRetension
                                            && x.SupplyManagementKITSeries.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                            && (x.SupplyManagementKITSeries.Status == KitStatus.WithIssue || x.SupplyManagementKITSeries.Status == KitStatus.WithoutIssue)
                                            && x.RandomizationId == null).OrderBy(x => x.Id).ToList();

                        if (kitSequencedata == null || kitSequencedata.Count == 0)
                        {
                            randomizationNumberDto.ErrorMessage = "Kit is not available!";
                            return randomizationNumberDto;
                        }

                        randomizationNumberDto.KitCount = kitSequencedata.Select(x => x.SupplyManagementKITSeriesId).Distinct().Count();
                        foreach (var visititem in visits)
                        {
                            if (string.IsNullOrEmpty(randomizationNumberDto.KitNo))
                            {
                                randomizationNumberDto.RandomizationNumber = Convert.ToString(visititem.SupplyManagementUploadFileDetail.RandomizationNo);
                                randomizationNumberDto.ProductCode = visititem.Value;
                                randomizationNumberDto.DisplayRandomizationNumber = visititem.SupplyManagementUploadFileDetail.DisplayRandomizationNumber;
                                var kit = kitSequencedata.Where(x => x.SupplyManagementKITSeries.KitNo.ToLower() == visititem.SupplyManagementUploadFileDetail.KitNo.ToLower()).OrderBy(x => x.Id).FirstOrDefault();

                                if (kit != null)
                                {
                                    var currentdate = DateTime.Now.Date;
                                    var kitExpiryDate = kit.SupplyManagementKITSeries.KitExpiryDate;
                                    if (Convert.ToDateTime(kitExpiryDate).Date >= currentdate.Date)
                                    {
                                        randomizationNumberDto.KitNo = kit.SupplyManagementKITSeries.KitNo;
                                        randomizationNumberDto.KitDetailId = kit.Id;
                                        randomizationNumberDto.VisitId = visits.FirstOrDefault().ProjectDesignVisitId;
                                    }
                                }
                            }
                        }
                        return randomizationNumberDto;
                    }
                }
                if (supplyManagementKitNumberSettings.IsDoseWiseKit)
                {
                    randomizationNumberDto.IsDoseWiseKit = supplyManagementKitNumberSettings.IsDoseWiseKit;
                    if (string.IsNullOrEmpty(randomization.Dosefactor))
                    {
                        randomizationNumberDto.ErrorMessage = "Dose not found for this patient";
                        return randomizationNumberDto;
                    }

                    var visitlist = _context.SupplyManagementUploadFileVisit
                       .Include(x => x.SupplyManagementUploadFileDetail)
                       .ThenInclude(x => x.SupplyManagementUploadFile)
                       .Where(x => x.DeletedDate == null
                       && x.SupplyManagementUploadFileDetail.DeletedDate == null
                       && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.DeletedDate == null
                       && x.SupplyManagementUploadFileDetail.RandomizationId == null
                       && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve
                       && x.Isfirstvisit == true).OrderBy(x => x.Id).ToList();

                    if (visitlist == null || visitlist.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }

                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                    {
                        visitlist = visitlist.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.SiteId == siteId).ToList();
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                    {
                        var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == countryId).FirstOrDefault();
                        if (site != null)
                        {
                            visitlist = visitlist.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                             && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).ToList();
                        }
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                    {
                        visitlist = visitlist.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).ToList();
                    }

                    if (visitlist == null || visitlist.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }
                    if (!string.IsNullOrEmpty(productType))
                    {
                        var productarray = productType.Split(',').ToArray();
                        kitdata = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                                x.DeletedDate == null
                                                && x.SupplyManagementKIT.ProjectDesignVisitId == visitlist.FirstOrDefault().ProjectDesignVisitId
                                                && x.SupplyManagementKIT.ProjectId == projectid
                                                && productarray.Contains(x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode)
                                                && !x.IsRetension
                                                && x.SupplyManagementKIT.DeletedDate == null
                                                && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                                && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                                && x.RandomizationId == null).OrderBy(x => x.Id).ToList();
                    }
                    else
                    {
                        kitdata = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                          x.DeletedDate == null
                                          && x.SupplyManagementKIT.ProjectDesignVisitId == visitlist.FirstOrDefault().ProjectDesignVisitId
                                          && x.SupplyManagementKIT.DeletedDate == null
                                          && !x.IsRetension
                                          && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                          && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                          && x.RandomizationId == null).OrderBy(x => x.Id).ToList();
                    }

                    randomizationNumberDto.KitCount = kitdata.Count;

                    if (kitdata == null || kitdata.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Kit is not available!";
                        return randomizationNumberDto;
                    }

                    var priorites = _context.SupplyManagementKitDosePriority.Where(s => s.DeletedDate == null && s.ProjectId == projectid).ToList();
                    if (priorites.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please set dose priority for kit allocation!";
                        return randomizationNumberDto;
                    }


                    foreach (var visititem in visitlist)
                    {
                        randomizationNumberDto.Dose = 0;
                        if ((!string.IsNullOrEmpty(productType) && productType.Split(',').AsEnumerable().Contains(visititem.Value.Trim()) && randomizationNumberDto.Dose != Convert.ToDecimal(randomization.Dosefactor)) || (randomizationNumberDto.Dose != Convert.ToDecimal(randomization.Dosefactor)))
                        {

                            var kit = kitdata.Where(x => x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode == visititem.Value.Trim()).OrderBy(x => x.Id).ToList();
                            if (kit.Count > 0)
                            {
                                randomizationNumberDto.RandomizationNumber = Convert.ToString(visititem.SupplyManagementUploadFileDetail.RandomizationNo);
                                randomizationNumberDto.ProductCode = visititem.Value;
                                randomizationNumberDto.DisplayRandomizationNumber = visititem.SupplyManagementUploadFileDetail.DisplayRandomizationNumber;
                                randomizationNumberDto.KitDoseList = new List<KitDoseList>();
                                decimal firstpriority = priorites.First(s => s.DosePriority == DosePriority.Priority1).Dose;
                                decimal secondpriority = priorites.First(s => s.DosePriority == DosePriority.Priority2).Dose;

                                var firstPrioritykit = kit.Where(s => (decimal)s.SupplyManagementKIT.Dose == firstpriority).ToList();
                                decimal totaldose = 0;
                                foreach (var kitdose in firstPrioritykit)
                                {
                                    if (firstpriority == kitdose.SupplyManagementKIT.Dose && randomizationNumberDto.Dose != Convert.ToDecimal(randomization.Dosefactor))
                                    {
                                        var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == kitdose.SupplyManagementKIT.ProductReceiptId).FirstOrDefault();
                                        if (productreciept != null)
                                        {
                                            var expiry = Convert.ToDateTime(productreciept.RetestExpiryDate).Date;
                                            var date = expiry.AddDays(-(int)kitdose.SupplyManagementKIT.Days);
                                            var currentdate = DateTime.Now.Date;
                                            if (date > currentdate)
                                            {
                                                KitDoseList obj = new KitDoseList();
                                                obj.kitNo = kitdose.KitNo;
                                                obj.KitDetailId = kitdose.Id;
                                                obj.VisitId = visititem.ProjectDesignVisitId;
                                                obj.Dose = kitdose.SupplyManagementKIT.Dose;
                                                obj.ProductCode = visititem.Value;
                                                randomizationNumberDto.KitDoseList.Add(obj);
                                                randomizationNumberDto.Dose += kitdose.SupplyManagementKIT.Dose;
                                                totaldose = Convert.ToDecimal(randomization.Dosefactor) - (decimal)kitdose.SupplyManagementKIT.Dose;
                                                if (totaldose < firstpriority)
                                                {
                                                    break;
                                                }

                                            }
                                        }
                                        if (randomizationNumberDto.Dose == Convert.ToDecimal(randomization.Dosefactor))
                                        {
                                            return randomizationNumberDto;
                                        }
                                    }
                                }

                                var secondPrioritykit = kit.Where(s => (decimal)s.SupplyManagementKIT.Dose == secondpriority).ToList();

                                foreach (var kitdose in secondPrioritykit)
                                {
                                    if (secondpriority == kitdose.SupplyManagementKIT.Dose && randomizationNumberDto.Dose != Convert.ToDecimal(randomization.Dosefactor))
                                    {
                                        var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == kitdose.SupplyManagementKIT.ProductReceiptId).FirstOrDefault();
                                        if (productreciept != null)
                                        {
                                            var expiry = Convert.ToDateTime(productreciept.RetestExpiryDate).Date;
                                            var date = expiry.AddDays(-(int)kitdose.SupplyManagementKIT.Days);
                                            var currentdate = DateTime.Now.Date;
                                            if (date > currentdate)
                                            {
                                                KitDoseList obj = new KitDoseList();
                                                obj.kitNo = kitdose.KitNo;
                                                obj.KitDetailId = kitdose.Id;
                                                obj.VisitId = visititem.ProjectDesignVisitId;
                                                obj.Dose = kitdose.SupplyManagementKIT.Dose;
                                                obj.ProductCode = visititem.Value;
                                                randomizationNumberDto.KitDoseList.Add(obj);
                                                randomizationNumberDto.Dose += kitdose.SupplyManagementKIT.Dose;
                                            }
                                        }
                                    }
                                    if (randomizationNumberDto.Dose == Convert.ToDecimal(randomization.Dosefactor))
                                    {
                                        return randomizationNumberDto;
                                    }
                                }

                                if (randomizationNumberDto.Dose != Convert.ToDecimal(randomization.Dosefactor))
                                {
                                    randomizationNumberDto.ErrorMessage = "Kit is not available!";
                                    return randomizationNumberDto;

                                }
                                return randomizationNumberDto;
                            }
                        }
                    }

                    if (string.IsNullOrEmpty(randomizationNumberDto.RandomizationNumber) && kitdata.Count > 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }

                    return randomizationNumberDto;
                }

                var visit = _context.SupplyManagementUploadFileVisit
                      .Include(x => x.SupplyManagementUploadFileDetail)
                      .ThenInclude(x => x.SupplyManagementUploadFile)
                      .Where(x => x.DeletedDate == null
                      && x.SupplyManagementUploadFileDetail.DeletedDate == null
                      && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.DeletedDate == null
                      && x.SupplyManagementUploadFileDetail.RandomizationId == null
                      && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve
                      && x.Isfirstvisit == true).OrderBy(x => x.Id).ToList();

                if (visit == null || visit.Count == 0)
                {
                    randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                    return randomizationNumberDto;
                }

                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                {
                    visit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.SiteId == siteId).OrderBy(x => x.Id).ToList();
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                {
                    var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == countryId).FirstOrDefault();
                    if (site != null)
                    {
                        visit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                         && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).OrderBy(x => x.Id).ToList();
                    }
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                {
                    visit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).OrderBy(x => x.Id).ToList();
                }

                if (visit == null || visit.Count == 0)
                {
                    randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                    return randomizationNumberDto;
                }

                if (!string.IsNullOrEmpty(productType))
                {
                    var productarray = productType.Split(',').ToArray();
                    if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                    {
                        kitdata = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                                x.DeletedDate == null
                                                && x.SupplyManagementKIT.ProjectDesignVisitId == visit.FirstOrDefault().ProjectDesignVisitId
                                                && x.SupplyManagementKIT.ProjectId == projectid
                                                && productarray.Contains(x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode)
                                                && x.SupplyManagementKIT.DeletedDate == null
                                                && !x.IsRetension
                                                && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                                && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                                && x.RandomizationId == null).OrderBy(x => x.Id).ToList();

                        randomizationNumberDto.KitCount = kitdata.Count;

                        if (kitdata == null || kitdata.Count == 0)
                        {
                            randomizationNumberDto.ErrorMessage = "Kit is not available!";
                            return randomizationNumberDto;
                        }
                    }
                    else
                    {
                        kitSequencedata = _context.SupplyManagementKITSeriesDetail
                            .Include(x => x.SupplyManagementKITSeries)
                            .ThenInclude(x => x.SupplyManagementShipment)
                            .ThenInclude(x => x.SupplyManagementRequest)
                            .Include(x => x.PharmacyStudyProductType)
                            .ThenInclude(x => x.ProductType)
                            .Where(x =>
                                            x.DeletedDate == null
                                            && x.ProjectDesignVisitId == visit.FirstOrDefault().ProjectDesignVisitId
                                            && x.SupplyManagementKITSeries.ProjectId == projectid
                                            && productarray.Contains(x.PharmacyStudyProductType.ProductType.ProductTypeCode)
                                            && x.SupplyManagementKITSeries.DeletedDate == null
                                            && !x.SupplyManagementKITSeries.IsRetension
                                            && x.SupplyManagementKITSeries.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                            && (x.SupplyManagementKITSeries.Status == KitStatus.WithIssue || x.SupplyManagementKITSeries.Status == KitStatus.WithoutIssue)
                                            && x.RandomizationId == null).OrderBy(x => x.Id).ToList();

                        if (kitSequencedata == null || kitSequencedata.Count == 0)
                        {
                            randomizationNumberDto.ErrorMessage = "Kit is not available!";
                            return randomizationNumberDto;
                        }

                        randomizationNumberDto.KitCount = kitSequencedata.Select(x => x.SupplyManagementKITSeriesId).Distinct().Count();
                    }

                    foreach (var visititem in visit)
                    {
                        if (productarray.Contains(visititem.Value.Trim()) && string.IsNullOrEmpty(randomizationNumberDto.KitNo))
                        {
                            randomizationNumberDto.RandomizationNumber = Convert.ToString(visititem.SupplyManagementUploadFileDetail.RandomizationNo);
                            randomizationNumberDto.ProductCode = visititem.Value;
                            randomizationNumberDto.DisplayRandomizationNumber = visititem.SupplyManagementUploadFileDetail.DisplayRandomizationNumber;
                            if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                            {
                                var kit = kitdata.Where(x => x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode == visititem.Value.Trim()).OrderBy(x => x.Id).FirstOrDefault();

                                if (kit != null)
                                {
                                    var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == kit.SupplyManagementKIT.ProductReceiptId).FirstOrDefault();
                                    if (productreciept != null)
                                    {
                                        var expiry = Convert.ToDateTime(productreciept.RetestExpiryDate).Date;
                                        var date = expiry.AddDays(-(int)kit.SupplyManagementKIT.Days);
                                        var currentdate = DateTime.Now.Date;
                                        if (date > currentdate)
                                        {
                                            randomizationNumberDto.KitNo = kit.KitNo;
                                            randomizationNumberDto.KitDetailId = kit.Id;
                                            randomizationNumberDto.VisitId = visit.FirstOrDefault().ProjectDesignVisitId;
                                        }
                                    }
                                }

                            }
                            else
                            {
                                var kit = kitSequencedata.Where(x => x.PharmacyStudyProductType.ProductType.ProductTypeCode == visititem.Value).OrderBy(x => x.Id).FirstOrDefault();

                                if (kit != null)
                                {
                                    var currentdate = DateTime.Now.Date;
                                    var kitExpiryDate = kit.SupplyManagementKITSeries.KitExpiryDate;
                                    if (Convert.ToDateTime(kitExpiryDate).Date >= currentdate.Date)
                                    {
                                        randomizationNumberDto.KitNo = kit.SupplyManagementKITSeries.KitNo;
                                        randomizationNumberDto.KitDetailId = kit.Id;
                                        randomizationNumberDto.VisitId = visit.FirstOrDefault().ProjectDesignVisitId;
                                    }

                                }

                            }
                        }
                    }
                    if (string.IsNullOrEmpty(randomizationNumberDto.RandomizationNumber))
                    {
                        if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                        {
                            if (kitdata.Count > 0)
                            {
                                randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                                return randomizationNumberDto;

                            }
                        }
                        else
                        {
                            if (kitSequencedata.Count > 0)
                            {
                                randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                                return randomizationNumberDto;
                            }
                        }
                    }

                    return randomizationNumberDto;
                }
                else
                {

                    if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                    {
                        kitdata = _context.SupplyManagementKITDetail.Include(s => s.SupplyManagementKIT).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                           x.DeletedDate == null
                                           && x.SupplyManagementKIT.ProjectDesignVisitId == visit.FirstOrDefault().ProjectDesignVisitId
                                           && x.SupplyManagementKIT.DeletedDate == null
                                           && !x.IsRetension
                                           && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                           && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                           && x.RandomizationId == null).OrderBy(x => x.Id).ToList();
                        randomizationNumberDto.KitCount = kitdata.Count;

                        if (kitdata == null || kitdata.Count == 0)
                        {
                            randomizationNumberDto.ErrorMessage = "Kit is not available!";
                            return randomizationNumberDto;
                        }
                    }
                    else
                    {
                        kitSequencedata = _context.SupplyManagementKITSeriesDetail
                            .Include(x => x.SupplyManagementKITSeries)
                            .ThenInclude(x => x.SupplyManagementShipment)
                            .ThenInclude(x => x.SupplyManagementRequest)
                            .Include(x => x.PharmacyStudyProductType)
                            .ThenInclude(x => x.ProductType)
                            .Where(x =>
                                            x.DeletedDate == null
                                            && x.ProjectDesignVisitId == visit.FirstOrDefault().ProjectDesignVisitId
                                            && x.SupplyManagementKITSeries.ProjectId == projectid
                                            && x.SupplyManagementKITSeries.DeletedDate == null
                                            && !x.SupplyManagementKITSeries.IsRetension
                                            && x.SupplyManagementKITSeries.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                            && (x.SupplyManagementKITSeries.Status == KitStatus.WithIssue || x.SupplyManagementKITSeries.Status == KitStatus.WithoutIssue)
                                            && x.RandomizationId == null).OrderBy(x => x.Id).ToList();

                        if (kitSequencedata == null || kitSequencedata.Count == 0)
                        {
                            randomizationNumberDto.ErrorMessage = "Kit is not available!";
                            return randomizationNumberDto;
                        }

                        randomizationNumberDto.KitCount = kitSequencedata.Select(x => x.SupplyManagementKITSeriesId).Distinct().Count();
                    }

                    foreach (var visititem in visit)
                    {
                        if (string.IsNullOrEmpty(randomizationNumberDto.KitNo))
                        {
                            randomizationNumberDto.RandomizationNumber = Convert.ToString(visititem.SupplyManagementUploadFileDetail.RandomizationNo);
                            randomizationNumberDto.ProductCode = visititem.Value;
                            randomizationNumberDto.DisplayRandomizationNumber = visititem.SupplyManagementUploadFileDetail.DisplayRandomizationNumber;
                            if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                            {
                                var kit = kitdata.Where(x => x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode == visititem.Value.Trim()).OrderBy(x => x.Id).FirstOrDefault();

                                if (kit != null)
                                {
                                    var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == kit.SupplyManagementKIT.ProductReceiptId).FirstOrDefault();
                                    if (productreciept != null)
                                    {
                                        var expiry = Convert.ToDateTime(productreciept.RetestExpiryDate).Date;
                                        var date = expiry.AddDays(-(int)kit.SupplyManagementKIT.Days);
                                        var currentdate = DateTime.Now.Date;
                                        if (date > currentdate)
                                        {
                                            randomizationNumberDto.KitNo = kit.KitNo;
                                            randomizationNumberDto.KitDetailId = kit.Id;
                                            randomizationNumberDto.VisitId = visit.FirstOrDefault().ProjectDesignVisitId;
                                        }
                                    }

                                }
                            }
                            else
                            {
                                var kit = kitSequencedata.Where(x => x.PharmacyStudyProductType.ProductType.ProductTypeCode == visititem.Value).OrderBy(x => x.Id).FirstOrDefault();
                                if (kit != null)
                                {
                                    var currentdate = DateTime.Now.Date;
                                    var kitExpiryDate = kit.SupplyManagementKITSeries.KitExpiryDate;
                                    if (Convert.ToDateTime(kitExpiryDate).Date >= currentdate.Date)
                                    {
                                        randomizationNumberDto.KitNo = kit.SupplyManagementKITSeries.KitNo;
                                        randomizationNumberDto.KitDetailId = kit.Id;
                                        randomizationNumberDto.VisitId = visit.FirstOrDefault().ProjectDesignVisitId;
                                    }

                                }

                            }
                        }
                    }

                    if (string.IsNullOrEmpty(randomizationNumberDto.RandomizationNumber))
                    {
                        if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                        {
                            if (kitdata.Count > 0)
                            {
                                randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                                return randomizationNumberDto;

                            }
                        }
                        else
                        {
                            if (kitSequencedata.Count > 0)
                            {
                                randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                                return randomizationNumberDto;
                            }
                        }
                    }
                    return randomizationNumberDto;
                }
            }
            else
            {
                if (!string.IsNullOrEmpty(productType))
                {
                    var productarray = productType.Split(',').ToArray();

                    var data = _context.SupplyManagementUploadFileDetail
                                                    .Where(x => x.RandomizationId == null
                                                    && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve
                                                    && x.DeletedDate == null).OrderBy(x => x.RandomizationNo).ToList();

                    if (data == null || data.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }

                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                    {
                        data = data.Where(x => x.SupplyManagementUploadFile.SiteId == siteId).ToList();
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                    {
                        var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == countryId).FirstOrDefault();
                        if (site != null)
                        {
                            data = data.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId && x.SupplyManagementUploadFile.ProjectId == projectid).ToList();
                        }
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                    {
                        data = data.Where(x => x.SupplyManagementUploadFile.ProjectId == projectid).ToList();
                    }

                    if (data == null || data.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }

                    if (data != null)
                    {
                        foreach (var item in data)
                        {
                            var visits = _context.SupplyManagementUploadFileVisit.Where(x => x.DeletedDate == null && x.SupplyManagementUploadFileDetailId == item.Id && x.Isfirstvisit == true).ToList();
                            if (visits.Count > 0)
                            {
                                foreach (var visit in visits)
                                {
                                    if (productarray.Contains(visit.Value))
                                    {
                                        randomizationNumberDto.RandomizationNumber = Convert.ToString(item.RandomizationNo);
                                        randomizationNumberDto.ProductCode = visit.Value;
                                        randomizationNumberDto.VisitId = visit.ProjectDesignVisitId;
                                        randomizationNumberDto.DisplayRandomizationNumber = item.DisplayRandomizationNumber;
                                        return randomizationNumberDto;
                                    }
                                }
                            }

                        }

                    }
                }
                else
                {
                    var data = _context.SupplyManagementUploadFileDetail
                                                  .Where(x =>
                                                  x.RandomizationId == null
                                                  && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve
                                                  && x.DeletedDate == null).OrderBy(x => x.RandomizationNo).ToList();

                    if (data == null || data.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }


                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                    {
                        uploaddetail = data.Find(x => x.SupplyManagementUploadFile.SiteId == siteId);
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                    {
                        var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).FirstOrDefault(x => x.Id == countryId);
                        if (site != null)
                        {
                            uploaddetail = data.Find(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId && x.SupplyManagementUploadFile.ProjectId == projectid);
                        }
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                    {
                        uploaddetail = data.Find(x => x.SupplyManagementUploadFile.ProjectId == projectid);
                    }

                    if (uploaddetail == null)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }
                    var visits = _context.SupplyManagementUploadFileVisit.Where(x => x.DeletedDate == null && x.SupplyManagementUploadFileDetailId == uploaddetail.Id && x.Isfirstvisit == true).FirstOrDefault();
                    if (visits != null)
                    {
                        randomizationNumberDto.ProductCode = visits.Value;
                        randomizationNumberDto.VisitId = visits.ProjectDesignVisitId;
                    }
                    randomizationNumberDto.RandomizationNumber = Convert.ToString(uploaddetail.RandomizationNo);
                    randomizationNumberDto.DisplayRandomizationNumber = uploaddetail.DisplayRandomizationNumber;
                }
            }

            return randomizationNumberDto;

        }
        public void UpdateRandomizationIdForIWRS(RandomizationDto obj)
        {
            SupplyManagementUploadFileDetail data = new SupplyManagementUploadFileDetail();
            var numerformate = _context.RandomizationNumberSettings.Where(x => x.ProjectId == obj.ParentProjectId).FirstOrDefault();
            if (numerformate != null && numerformate.IsIGT)
            {
                var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                if (SupplyManagementUploadFile == null)
                {
                    return;
                }
                var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.ParentProjectId).FirstOrDefault();
                if (setting == null)
                {
                    return;
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                {
                    if ((setting.IsStaticRandomizationNo == null || setting.IsStaticRandomizationNo == false) && !string.IsNullOrEmpty(numerformate.PrefixRandomNo))
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId && x.DeletedDate == null
                       && (numerformate.PrefixRandomNo.Trim() + x.RandomizationNo.ToString()) == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }
                    else if (setting.IsStaticRandomizationNo == true)
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId && x.DeletedDate == null
                      && x.DisplayRandomizationNumber == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }
                    else
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId && x.DeletedDate == null
                        && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }

                    if (data != null)
                    {
                        data.RandomizationId = obj.Id;
                        _context.SupplyManagementUploadFileDetail.Update(data);

                    }
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                {
                    var country = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                    var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == country.ManageSiteId).FirstOrDefault();
                    if (site != null)
                    {
                        if ((setting.IsStaticRandomizationNo == null || setting.IsStaticRandomizationNo == false) && !string.IsNullOrEmpty(numerformate.PrefixRandomNo))
                        {
                            data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                           && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId && x.DeletedDate == null
                           && (numerformate.PrefixRandomNo.Trim() + x.RandomizationNo.ToString()) == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                        }
                        else if (setting.IsStaticRandomizationNo == true)
                        {
                            data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                             && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId && x.DeletedDate == null
                             && x.DisplayRandomizationNumber == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                        }
                        else
                        {
                            data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                          && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId && x.DeletedDate == null
                          && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                        }

                        if (data != null)
                        {
                            data.RandomizationId = obj.Id;
                            _context.SupplyManagementUploadFileDetail.Update(data);

                        }
                    }
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                {
                    if ((setting.IsStaticRandomizationNo == null || setting.IsStaticRandomizationNo == false) && !string.IsNullOrEmpty(numerformate.PrefixRandomNo))
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                        && (numerformate.PrefixRandomNo.Trim() + x.RandomizationNo.ToString()) == obj.RandomizationNumber && x.DeletedDate == null && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }
                    else if (setting.IsStaticRandomizationNo == true)
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                       && x.DisplayRandomizationNumber == obj.RandomizationNumber && x.DeletedDate == null && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }
                    else
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                        && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.DeletedDate == null && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                    }

                    if (data != null)
                    {
                        data.RandomizationId = obj.Id;
                        _context.SupplyManagementUploadFileDetail.Update(data);

                    }
                }
                _context.Save();
            }
        }
        public bool ValidateRandomizationIdForIWRS(RandomizationDto obj)
        {
            SupplyManagementUploadFileDetail data = new SupplyManagementUploadFileDetail();
            var numerformate = _context.RandomizationNumberSettings.Where(x => x.ProjectId == obj.ParentProjectId).FirstOrDefault();
            if (numerformate != null && numerformate.IsIGT)
            {
                var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.DeletedDate == null && x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                if (SupplyManagementUploadFile == null)
                {
                    return false;
                }
                var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.ParentProjectId).FirstOrDefault();
                if (setting == null)
                {
                    return false;
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                {
                    if ((setting.IsStaticRandomizationNo == null || setting.IsStaticRandomizationNo == false) && !string.IsNullOrEmpty(numerformate.PrefixRandomNo))
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId
                        && x.DeletedDate == null && (numerformate.PrefixRandomNo.Trim() + x.RandomizationNo.ToString()) == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }
                    else if (setting.IsStaticRandomizationNo == true)
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId
                       && x.DeletedDate == null && x.DisplayRandomizationNumber == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }
                    else
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId
                           && x.DeletedDate == null && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                    }

                    if (data != null && data.RandomizationId != null && data.RandomizationId != obj.Id)
                    {
                        return false;
                    }
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                {
                    var country = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                    var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == country.ManageSiteId).FirstOrDefault();
                    if (site != null)
                    {
                        if ((setting.IsStaticRandomizationNo == null || setting.IsStaticRandomizationNo == false) && !string.IsNullOrEmpty(numerformate.PrefixRandomNo))
                        {
                            data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                            && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                            && x.DeletedDate == null && (numerformate.PrefixRandomNo.Trim() + x.RandomizationNo.ToString()) == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                        }
                        else if (setting.IsStaticRandomizationNo == true)
                        {
                            data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                              && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                              && x.DeletedDate == null && x.DisplayRandomizationNumber == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                        }
                        else
                        {
                            data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                              && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                              && x.DeletedDate == null && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                        }

                        if (data != null && data.RandomizationId != null && data.RandomizationId != obj.Id)
                        {
                            return false;
                        }
                    }

                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                {
                    if ((setting.IsStaticRandomizationNo == null || setting.IsStaticRandomizationNo == false) && !string.IsNullOrEmpty(numerformate.PrefixRandomNo))
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                        && x.DeletedDate == null && (numerformate.PrefixRandomNo.Trim() + x.RandomizationNo.ToString()) == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }
                    else if (setting.IsStaticRandomizationNo == true)
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                       && x.DeletedDate == null && x.DisplayRandomizationNumber == obj.RandomizationNumber && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    }
                    else
                    {
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                          && x.DeletedDate == null && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                    }

                    if (data != null && data.RandomizationId != null && data.RandomizationId != obj.Id)
                    {
                        return false;
                    }
                }
            }
            return true;
        }
        public bool IsScreeningFormatSetInStudy(int id)
        {
            var randomization = Find(id);
            var sitedata = _projectRepository.Find(randomization.ProjectId);
            var studydata = _context.ScreeningNumberSettings.FirstOrDefault(x => x.ProjectId == (int)sitedata.ParentProjectId);
            if (studydata?.ScreeningLength <= 0)
                return false;
            return true;
        }

        public bool IsRandomFormatSetInStudy(int id)
        {
            var randomization = Find(id);
            var sitedata = _projectRepository.Find(randomization.ProjectId);
            var studydata = _context.RandomizationNumberSettings.FirstOrDefault(x => x.ProjectId == (int)sitedata.ParentProjectId);
            if (studydata?.RandomNoLength <= 0 && !studydata.IsIGT)
                return false;
            return true;
        }

        public RandomizationNumberDto GenerateScreeningNumber(int id)
        {
            var randomization = Find(id);
            var site = _projectRepository.Find(randomization.ProjectId);
            var sitedata = _context.ScreeningNumberSettings.First(x => x.ProjectId == randomization.ProjectId);
            var studydata = _context.ScreeningNumberSettings.First(x => x.ProjectId == (int)site.ParentProjectId);
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            randomizationNumberDto.ProjectId = randomization.ProjectId;
            randomizationNumberDto.ParentProjectId = (int)site.ParentProjectId;
            randomizationNumberDto.IsManualScreeningNo = studydata.IsManualScreeningNo;
            randomizationNumberDto.IsSiteDependentScreeningNo = studydata.IsSiteDependentScreeningNo;
            randomizationNumberDto.ScreeningLength = studydata.ScreeningLength;

            if (studydata.IsManualScreeningNo)
            {
                randomizationNumberDto.ScreeningNumber = "";
            }
            else
            {
                int latestno;
                if (studydata.IsSiteDependentScreeningNo)
                {
                    latestno = sitedata.ScreeningNoseries;
                    randomizationNumberDto.ScreeningNoseries = sitedata.ScreeningNoseries;
                    if (string.IsNullOrEmpty(sitedata.PrefixScreeningNo))
                        randomizationNumberDto.ScreeningNumber = latestno.ToString().PadLeft(studydata.ScreeningLength, '0');
                    else
                        randomizationNumberDto.ScreeningNumber = sitedata.PrefixScreeningNo + latestno.ToString().PadLeft(studydata.ScreeningLength, '0');
                }
                else
                {
                    latestno = studydata.ScreeningNoseries;
                    randomizationNumberDto.ScreeningNoseries = studydata.ScreeningNoseries;
                    randomizationNumberDto.ScreeningNumber = latestno.ToString().PadLeft(studydata.ScreeningLength, '0');
                }
            }
            randomizationNumberDto.IsTestSite = site.IsTestSite;
            if (site.IsTestSite)
            {
                var patientCount = All.Where(x => x.ProjectId == randomization.ProjectId && x.DeletedDate == null && x.ScreeningNumber != null).Count() + 1;
                randomizationNumberDto.ScreeningNumber = "TS -" + patientCount.ToString().PadLeft(studydata.ScreeningLength, '0');
                return randomizationNumberDto;
            }
            return randomizationNumberDto;
        }

        //public List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted)
        //{
        //    var result = All.Where(x => x.ProjectId == projectId && !x.IsGeneric && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
        //          ProjectTo<RandomizationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        //    var projectright = _projectRightRepository.FindBy(x => x.ProjectId == projectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId).FirstOrDefault();
        //    var rolelist = _context.SiteTeam.Where(x => x.ProjectId == projectId && x.DeletedDate == null && x.IsIcfApproval == true).Select(x => x.RoleId).ToList();
        //    result.ForEach(x =>
        //    {
        //        x.PatientStatusName = x.PatientStatusId.GetDescription();
        //        x.IsShowEconsentIcon = (rolelist.Contains(_jwtTokenAccesser.RoleId) && projectright != null);
        //    });

        //    var project = _context.Project.Find(_context.Project.Find(projectId).ParentProjectId);
        //    var ProjectSettings = _context.ProjectSettings.Where(x => x.ProjectId == project.Id && x.DeletedDate == null).FirstOrDefault();

        //    result.ForEach(x =>
        //    {
        //        x.IsEicf = ProjectSettings?.IsEicf ?? false;
        //        x.IsAllEconsentReviewed = _context.EconsentReviewDetails.Any(c => c.RandomizationId == x.Id) ? _context.EconsentReviewDetails.Where(c => c.RandomizationId == x.Id).All(z => z.IsReviewedByPatient) : false;
        //        x.ParentProjectCode = project.ProjectCode;
        //        var screeningtemplate = _screeningTemplateRepository.FindByInclude(y => y.ScreeningVisit.ScreeningEntry.RandomizationId == x.Id && y.DeletedDate == null).ToList();
        //        x.IsLocked = screeningtemplate.Count() <= 0 || screeningtemplate.Any(y => y.IsLocked == false) ? false : true;
        //        x.isDocumentUpload = _context.IDVerification.Any(q => q.DeletedDate == null && q.UserId == x.UserId);
        //    });

        //    return result;
        //}
        public List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted)
        {
            // Get the base query for randomization entries
            var randomizationQuery = All.Where(x => x.ProjectId == projectId && !x.IsGeneric && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null));

            // Project to DTO and order by Id
            var result = randomizationQuery
                .ProjectTo<RandomizationGridDto>(_mapper.ConfigurationProvider)
                .OrderByDescending(x => x.Id)
                .ToList();

            // Get the project rights and roles
            var projectRight = _projectRightRepository.FindBy(x => x.ProjectId == projectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId).FirstOrDefault();
            var roleList = _context.SiteTeam.Where(x => x.ProjectId == projectId && x.DeletedDate == null && x.IsIcfApproval == true)
                                            .Select(x => x.RoleId)
                                            .ToList();

            // Get the project and settings
            var project = _context.Project.Find(_context.Project.Find(projectId).ParentProjectId);
            var projectSettings = _context.ProjectSettings.FirstOrDefault(x => x.ProjectId == project.Id && x.DeletedDate == null);

            // Get all necessary data in advance to avoid querying in a loop
            var econsentReviewDetails = _context.EconsentReviewDetails
                .Where(c => result.Select(r => r.Id).Contains(c.RandomizationId))
                .ToList();

            //var screeningTemplates = _context.ScreeningTemplate.Include(s => s.ScreeningVisit).ThenInclude(s => s.ScreeningEntry)
            //    .Where(y => result.Select(r => r.Id).Contains((int)y.ScreeningVisit.ScreeningEntry.RandomizationId) && y.DeletedDate == null)
            //    .ToList();

            var idVerifications = _context.IDVerification
                .Where(q => q.DeletedDate == null && result.Select(r => r.UserId).Contains(q.UserId))
                .ToList();

            result.ForEach(x =>
            {
                x.PatientStatusName = x.PatientStatusId.GetDescription();
                x.IsShowEconsentIcon = (roleList.Contains(_jwtTokenAccesser.RoleId) && projectRight != null);
                x.IsEicf = projectSettings?.IsEicf ?? false;
                x.IsAllEconsentReviewed = econsentReviewDetails.Any(c => c.RandomizationId == x.Id) &&
                                          econsentReviewDetails.Where(c => c.RandomizationId == x.Id).All(z => z.IsReviewedByPatient);
                x.ParentProjectCode = project.ProjectCode;
                //var screeningTemplate = screeningTemplates.Where(y => y.ScreeningVisit.ScreeningEntry.RandomizationId == x.Id).ToList();
                //x.IsLocked = screeningTemplate.Count() > 0 && screeningTemplate.All(y => y.IsLocked);
                x.isDocumentUpload = idVerifications.Any(q => q.UserId == x.UserId);
            });

            return result;
        }

        public List<RandomizationGridDto> GetRandomizationById(int id, int projectId)
        {
            var result = All.Where(x => x.Id == id).
                  ProjectTo<RandomizationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            var projectright = _projectRightRepository.FindBy(x => x.ProjectId == projectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId).FirstOrDefault();
            var rolelist = _context.SiteTeam.Where(x => x.ProjectId == projectId && x.DeletedDate == null && x.IsIcfApproval == true).Select(x => x.RoleId).ToList();
            result.ForEach(x =>
            {
                x.PatientStatusName = x.PatientStatusId.GetDescription();
                x.IsShowEconsentIcon = (rolelist.Contains(_jwtTokenAccesser.RoleId) && projectright != null);
            });

            var project = _context.Project.Find(_context.Project.Find(projectId).ParentProjectId);
            var ProjectSettings = _context.ProjectSettings.Where(x => x.ProjectId == project.Id && x.DeletedDate == null).FirstOrDefault();

            result.ForEach(x =>
            {
                if (x.ProjectId > 0)
                {
                    var project = _context.Project.Find(x.ProjectId);
                    if (project != null)
                    {
                        x.IsTestSite = project.IsTestSite;
                    }

                }
                x.IsEicf = ProjectSettings?.IsEicf ?? false;
                x.IsAllEconsentReviewed = _context.EconsentReviewDetails.Any(c => c.RandomizationId == x.Id) ? _context.EconsentReviewDetails.Where(c => c.RandomizationId == x.Id).All(z => z.IsReviewedByPatient) : false;
                x.ParentProjectCode = project.ProjectCode;
                x.ParentProjectId = project.Id;
                var screeningtemplate = _screeningTemplateRepository.FindByInclude(y => y.ScreeningVisit.ScreeningEntry.RandomizationId == x.Id && y.DeletedDate == null).ToList();
                x.IsLocked = screeningtemplate.Count <= 0 || screeningtemplate.Exists(y => !y.IsLocked) ? false : true;
                x.isDocumentUpload = _context.IDVerification.Any(q => q.DeletedDate == null && q.UserId == x.UserId);
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
                x.DeletedDate == null))
            {
                return "Duplicate Randomization Number : " + objSave.RandomizationNumber;
            }

            var screeningTemplates = _context.ScreeningTemplate.Include(s => s.ScreeningVisit).ThenInclude(s => s.ScreeningEntry)
            .Where(y => (int)y.ScreeningVisit.ScreeningEntry.RandomizationId == objSave.Id)
            .ToList();
            if (screeningTemplates.Count() > 0 && screeningTemplates.All(y => y.IsLocked))
            {
                return "Patient status is locked!";
            }

            return "";
        }

        public async Task SendEmailOfScreenedtoPatient(Randomization randomization, int sendtype)
        {
            var studyId = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var studydata = _projectRepository.Find((int)studyId);
            if (studydata.IsSendSMS || studydata.IsSendEmail)
            {
                var userdata = _userRepository.Find((int)randomization.UserId);
                var userotp = await _centreUserService.GetUserOtpDetails($"{_environmentSetting.Value.CentralApi}UserOtp/GetuserOtpDetails/{userdata.Id}");
                await _emailSenderRespository.SendEmailOfScreenedPatient(randomization.Email, randomization.ScreeningNumber + " " + randomization.Initial, userdata.UserName, userotp.Otp, studydata.ProjectCode, randomization.PrimaryContactNumber, sendtype, studydata.IsSendEmail, studydata.IsSendSMS);
            }
        }

        public async Task SendEmailOfScreenedtoPatientLAR(Randomization randomization, int sendtype)
        {
            var studyId = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var studydata = _projectRepository.Find((int)studyId);
            if (studydata.IsSendSMS || studydata.IsSendEmail)
            {
                var userdata = _userRepository.Find((int)randomization.LARUserId);
                var userotp = await _centreUserService.GetUserOtpDetails($"{_environmentSetting.Value.CentralApi}UserOtp/GetuserOtpDetails/{userdata.Id}");
                await _emailSenderRespository.SendEmailOfScreenedPatient(randomization.LegalEmail, randomization.LegalFirstName + " " + randomization.LegalLastName, userdata.UserName, userotp.Otp, studydata.ProjectCode, randomization.LegalEmergencyCoNumber, sendtype, studydata.IsSendEmail, studydata.IsSendSMS);
            }
        }

        public void SendEmailOfStartEconsent(Randomization randomization)
        {
            var studyid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var study = _projectRepository.Find((int)studyid);
            var documentDetails = _context.EconsentSetup.Where(x => x.DeletedDate == null
            && x.ProjectId == study.Id
            && x.LanguageId == randomization.LanguageId
            && x.DeletedDate == null && x.DocumentStatusId == DocumentStatus.Final
            ).Select(x => new
            {
                x.Id,
                x.DocumentName
            }).ToList();

            if (documentDetails.Count > 0)
            {
                var reviewrecord = documentDetails.Select(x => new EconsentReviewDetails
                {
                    RandomizationId = randomization.Id,
                    EconsentSetupId = x.Id,
                    IsReviewedByPatient = false,
                }).ToList();
                _context.EconsentReviewDetails.AddRange(reviewrecord);
                _context.Save();
                //add audit report  
                foreach (var data in reviewrecord)
                {
                    EconsentReviewDetailsAudit audit = new EconsentReviewDetailsAudit();
                    audit.EconsentReviewDetailsId = data.Id;
                    audit.Activity = ICFAction.Screened;
                    audit.PateientStatus = data.Randomization?.PatientStatusId;
                    _econsentReviewDetailsAuditRepository.Add(audit);
                }
                string documentname = string.Join(",", documentDetails.Select(x => x.DocumentName).ToArray());
                _emailSenderRespository.SendEmailOfStartEconsent(randomization.Email, randomization.ScreeningNumber + " " + randomization.Initial, documentname, study.ProjectCode);
            }
        }

        public void SendEmailOfStartEconsentLAR(Randomization randomization)
        {
            var studyid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var study = _projectRepository.Find((int)studyid);
            var documentDetails = _context.EconsentSetup.Where(x => x.DeletedDate == null && x.ProjectId == study.Id
                && x.LanguageId == randomization.LanguageId && x.DeletedDate == null && x.DocumentStatusId == DocumentStatus.Final)
                .Select(x => new
                {
                    x.Id,
                    x.DocumentName
                }).ToList();

            if (documentDetails.Count > 0)
            {
                var reviewrecord = documentDetails.Select(x => new EconsentReviewDetails
                {
                    RandomizationId = randomization.Id,
                    EconsentSetupId = x.Id,
                    IsReviewedByPatient = false,
                    IsLAR = true
                }).ToList();
                _context.EconsentReviewDetails.AddRange(reviewrecord);
                _context.Save();
                //add audit report  
                foreach (var data in reviewrecord)
                {
                    EconsentReviewDetailsAudit audit = new EconsentReviewDetailsAudit();
                    audit.EconsentReviewDetailsId = data.Id;
                    audit.Activity = ICFAction.Screened;
                    audit.PateientStatus = data.Randomization?.PatientStatusId;
                    _econsentReviewDetailsAuditRepository.Add(audit);
                }
                string documentname = string.Join(",", documentDetails.Select(x => x.DocumentName).ToArray());
                _emailSenderRespository.SendEmailOfStartEconsent(randomization.LegalEmail, randomization.LegalFirstName + " " + randomization.LegalLastName, documentname, study.ProjectCode);
            }
        }
        public void ChangeStatustoConsentCompleted(int id)
        {
            var randomization = Find(id);
            if (randomization.PatientStatusId == ScreeningPatientStatus.ConsentInProcess || randomization.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess && (!_context.EconsentReviewDetails.Any(x => x.RandomizationId == id && !x.IsReviewDoneByInvestigator)))
            {
                randomization.PatientStatusId = ScreeningPatientStatus.ConsentCompleted;
                Update(randomization);
                _context.Save();
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
            // Change for Volunteer screening by Tinku Mahato in 21-06-2022
            if (randomization != null)
            {
                if (randomization.PatientStatusId != patientStatus)
                {
                    randomization.PatientStatusId = patientStatus;
                    Update(randomization);
                    if (patientStatus == ScreeningPatientStatus.ScreeningFailure || patientStatus == ScreeningPatientStatus.Withdrawal)
                    {

                        int userId = (int)randomization.UserId;
                        User user = _userRepository.Find(userId);
                        user.ValidTo = DateTime.Today.AddDays(-1);
                        _userRepository.Update(user);

                        user = await _centreUserService.GetUserData($"{_environmentSetting.Value.CentralApi}Login/GetUserData/{user.UserName}");
                        user.ValidTo = DateTime.Today.AddDays(-1);
                        var userDto = _mapper.Map<UserDto>(user);
                        await _centreUserService.UpdateUser(userDto, _environmentSetting.Value.CentralApi);
                    }
                }
            }

        }
        public void ChangeStatustoWithdrawal()
        {
            var roleName = _jwtTokenAccesser.RoleName;
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).AsEnumerable().First();

            if (roleName == "LAR")
            {
                randomization = FindBy(x => x.LARUserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            }
            randomization.PatientStatusId = ScreeningPatientStatus.Withdrawal;
            Update(randomization);
            _context.Save();
        }
        public DashboardPatientDto GetDashboardPatientDetail()
        {
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).AsEnumerable().FirstOrDefault();

            if (_jwtTokenAccesser.RoleName == "LAR")
            {
                randomization = FindBy(x => x.LARUserId == _jwtTokenAccesser.UserId).AsEnumerable().FirstOrDefault();
            }

            if (randomization != null)
            {
                var project = _context.Project.Where(x => x.Id == randomization.ProjectId).AsEnumerable().First();
                var parentproject = _context.Project.Where(x => x.Id == project.ParentProjectId).AsEnumerable().First();
                var idVerification = _context.IDVerification.Where(x => x.UserId == randomization.UserId && x.DeletedDate == null).FirstOrDefault();
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

                    x.UserPicUrl = _context.UploadSetting.FirstOrDefault()?.ImageUrl + (_roleRepository.Find(x.RoleId).RoleIcon ?? _userRepository.Find(x.UserId).ProfilePic ?? DocumentService.DefulatProfilePic);
                });
                dashboardPatientDto.siteTeams = siteteamdtos;
                if (project.ManageSiteId != null)
                {
                    dashboardPatientDto.hospitalName = _manageSiteRepository.Find((int)project.ManageSiteId).SiteName;

                    dashboardPatientDto.siteAddress = _manageSiteRepository.Find((int)project.ManageSiteId).SiteAddress;
                }
                dashboardPatientDto.patientdetail = _jwtTokenAccesser.RoleName == "LAR" ? "LAR FirstName: " + randomization.LegalFirstName + ", LAR LastName: " + randomization.LegalLastName : "Screening Number: " + randomization.ScreeningNumber + " Initial: " + randomization.Initial;
                if (idVerification != null)
                {
                    dashboardPatientDto.IsUpload = idVerification.IsUpload;
                    dashboardPatientDto.VerifyStatus = idVerification.VerifyStatus;
                }
                else
                {
                    dashboardPatientDto.IsUpload = false;
                }
                return dashboardPatientDto;
            }
            else
            {
                return new DashboardPatientDto();
            }

        }

        public List<ProjectDesignVisitMobileDto> GetPatientVisits()
        {
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).AsEnumerable().FirstOrDefault();
            if (randomization == null) return new List<ProjectDesignVisitMobileDto>();

            var data = _context.ScreeningVisit.Include(x => x.ScreeningEntry).Include(x => x.ProjectDesignVisit).Include(x => x.ScreeningTemplates).
                        Where(x => x.ScreeningEntry.RandomizationId == randomization.Id && (int)x.Status >= 4 && x.DeletedDate == null && x.ProjectDesignVisit.DeletedDate == null && x.ScreeningTemplates.Any(x => x.ProjectDesignTemplate.IsParticipantView == true)).
                        Select(r => new ProjectDesignVisitMobileDto
                        {
                            Id = r.Id,
                            DisplayName = (_jwtTokenAccesser.Language != 1) ?
                                r.ProjectDesignVisit.VisitLanguage.Where(x => x.LanguageId == _jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault()
                                // changes on 13/06/2023 for add visit name in screeningvisit table change by vipul rokad
                                : r.ScreeningVisitName,
                            ScreeningEntryId = r.ScreeningEntryId
                        }).ToList();

            return data;
        }

        public List<ProjectDesignTemplateMobileDto> GetPatientTemplates(int screeningVisitId)
        {
            var data = _context.ScreeningTemplate.Where(x => x.ScreeningVisitId == screeningVisitId && x.DeletedDate == null && x.ProjectDesignTemplate.IsParticipantView == true).
                        Select(r => new ProjectDesignTemplateMobileDto
                        {
                            ScreeningTemplateId = r.Id,
                            ProjectDesignTemplateId = r.ProjectDesignTemplateId,
                            ProjectDesignVisitId = r.ScreeningVisit.ProjectDesignVisitId,
                            // changes on 13/06/2023 for add template name in screeningtemplate table change by vipul rokad
                            TemplateName = ((_jwtTokenAccesser.Language != 1) ?
                r.ProjectDesignTemplate.TemplateLanguage.Where(x => x.DeletedDate == null && x.LanguageId == _jwtTokenAccesser.Language
                && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : r.ScreeningTemplateName),
                            Status = r.Status,
                            DesignOrder = r.ProjectDesignTemplate.DesignOrder,
                            ScheduleDate = r.ScheduleDate,
                            IsTemplateRestricted = false,
                            IsPastTemplate = false,
                            IsHide = r.IsHide ?? false,
                            IsDateTime = false
                        }).OrderBy(r => r.DesignOrder).ToList();

            data = data.Where(x => !x.IsHide).ToList();
            data.ForEach(x =>
            {
                if (x.Status == ScreeningTemplateStatus.Submitted)
                {
                    x.SubmittedDate = _context.ScreeningTemplateReview.FirstOrDefault(t => t.ScreeningTemplateId == x.ScreeningTemplateId && t.Status == ScreeningTemplateStatus.Submitted)?.CreatedDate;
                }
                if (x.ScheduleDate != null)
                {
                    var ProjectScheduleTemplates = _context.ProjectScheduleTemplate
                    .Where(t => t.ProjectDesignTemplateId == x.ProjectDesignTemplateId
                    && t.ProjectDesignVisitId == x.ProjectDesignVisitId && t.DeletedDate == null).Include(x => x.ProjectDesignVariable);

                    if (ProjectScheduleTemplates != null)
                    {
                        var datetime = ProjectScheduleTemplates.Where(x => x.ProjectDesignVariable.CollectionSource == CollectionSources.DateTime).ToList();
                        if (datetime.Count != 0)
                            x.IsDateTime = true;
                    }

                    var noofday = ProjectScheduleTemplates.Min(t => t.NoOfDay);
                    var noofHH = ProjectScheduleTemplates.Min(t => t.HH);
                    var noofMM = ProjectScheduleTemplates.Min(t => t.MM);
                    var ProjectScheduleTemplate = ProjectScheduleTemplates.First(x => x.NoOfDay == noofday);

                    if ((noofday == null) && (noofHH != null || noofMM != null))
                    {
                        var mindate = ((DateTime)x.ScheduleDate).AddMinutes(ProjectScheduleTemplate.NegativeDeviation * -1);
                        var maxdate = ((DateTime)x.ScheduleDate).AddMinutes(ProjectScheduleTemplate.PositiveDeviation);
                        var dateWithMin = _jwtTokenAccesser.GetClientDate().ToString("yyyy-MMM-dd HH:mm");
                        var clientDate = DateTime.ParseExact(dateWithMin, "yyyy-MMM-dd HH:mm", null);
                        if (clientDate >= mindate && clientDate <= maxdate)
                            x.IsTemplateRestricted = false;
                        else
                        {
                            x.IsTemplateRestricted = true;
                            if (DateTime.Now > maxdate)
                                x.IsPastTemplate = true;
                        }
                    }
                    else
                    {
                        var mindate = ((DateTime)x.ScheduleDate);
                        var maxdate = ((DateTime)x.ScheduleDate);
                        if (ProjectScheduleTemplate != null)
                        {
                            mindate = ((DateTime)x.ScheduleDate).AddDays(ProjectScheduleTemplate.NegativeDeviation * -1);
                            maxdate = ((DateTime)x.ScheduleDate).AddDays(ProjectScheduleTemplate.PositiveDeviation);
                        }
                        if (DateTime.Today >= mindate && DateTime.Today <= maxdate)
                            x.IsTemplateRestricted = false;
                        else
                        {
                            x.IsTemplateRestricted = true;
                            if (DateTime.Today > maxdate)
                                x.IsPastTemplate = true;
                        }
                    }


                }
            });
            return data;
        }


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
            var ParentProject = _context.Project.First(x => x.Id == projectid).ParentProjectId;
            var sites = _context.Project.Where(x => x.ParentProjectId == projectid).AsEnumerable().Select(x => x.Id).ToList();

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
           x.Project.ParentProjectId == projectId) && (!x.Project.IsTestSite) && x.DeletedDate == null).GroupBy(
               t => new { t.PatientStatusId }).Select(g => new DashboardQueryStatusDto
               {
                   DisplayName = g.Key.PatientStatusId.GetDescription(),
                   Total = g.Count()
               }).ToList();
            return result;

        }

        public List<DropDownDto> GetAttendanceForMeddraCodingDropDown(MeddraCodingSearchDto filters)
        {
            var projectList = new List<int>();
            int ProjectId = 0;
            if (filters.ProjectId == 0)
                ProjectId = filters.ProjectDesignId;
            else
                ProjectId = (int)filters.ProjectId;
            projectList = GetProjectList(ProjectId);
            if (projectList == null || projectList.Count == 0)
                return new List<DropDownDto>();

            var Isstatic = _context.Project.Where(x => x.Id == ProjectId).Select(r => r.IsStatic).FirstOrDefault();

            if (Isstatic)
            {
                return All.Where(t => (t.CompanyId == null
                               || t.CompanyId == _jwtTokenAccesser.CompanyId)
                              && projectList.Any(c => c == t.ProjectId)
                               ).Select(r => new DropDownDto
                               {
                                   Id = r.Id,
                                   Value = r.ScreeningNumber + "-" + r.Initial + "-" + r.RandomizationNumber
                               }).ToList();
            }


            return new List<DropDownDto>();
        }

        private List<int> GetProjectList(int ProjectId)
        {
            var projectList = _projectRightRepository.GetProjectRightIdList();
            return _context.Project.Where(c => c.DeletedDate == null &&
            (c.Id == ProjectId || c.ParentProjectId == ProjectId) && projectList.Any(t => t == c.Id)).Select(x => x.Id).ToList();
        }

        // Dashboard chart for target status
        public List<DashboardPatientStatusDto> GetDashboardPatientStatus(int projectId)
        {
            var pro = _context.Project.First(x => x.Id == projectId);
            var project = new List<Data.Entities.Master.Project>();
            if (pro.ParentProjectId == null)
            {
                project = _context.Project.Where(x => x.ParentProjectId == projectId && _context.ProjectRight.Any(a => a.ProjectId == x.Id
                                                  && a.UserId == _jwtTokenAccesser.UserId && a.RoleId == _jwtTokenAccesser.RoleId
                                                  && a.DeletedDate == null && a.RollbackReason == null) && x.DeletedDate == null).ToList();
            }
            else
            {
                project = _context.Project.Where(x => x.Id == projectId).ToList();
            }

            var result = new List<DashboardPatientStatusDto>();
            project.ForEach(t =>
            {
                var data = new DashboardPatientStatusDto();
                data.ProjectId = t.Id;
                data.ProjectName = t.ProjectCode;
                data.Target = 0;
                data.IsParentProject = pro.ParentProjectId == null;
                data.ParentProjectTarget = t.AttendanceLimit;
                data.StatusList = new List<DashboardPatientStatusDisplayDto>();
                result.Add(data);
            });

            result.ForEach(x =>
            {
                var ScreeningPatientStatus = Enum.GetValues(typeof(ScreeningPatientStatus))
                                           .Cast<ScreeningPatientStatus>().Select(e => new DashboardPatientStatusDisplayDto
                                           {
                                               Id = Convert.ToInt16(e),
                                               DisplayName = e.GetDescription()
                                           }).ToList();

                x.StatusList.AddRange(ScreeningPatientStatus);

                x.StatusList.ForEach(status =>
                {
                    var randomization = All.Include(q => q.Project)
                    .Where(q => q.ProjectId == x.ProjectId && q.DeletedDate == null && (int)q.PatientStatusId == status.Id)
                    .GroupBy(y => y.PatientStatusId)
                    .Select(g => new DashboardPatientStatusDisplayDto
                    {
                        Id = (int)g.Key,
                        ProjectName = x.ProjectName,
                        DisplayName = g.Key.GetDescription(),
                        Avg = g.Count()
                    }).FirstOrDefault();

                    status.ProjectName = x.ProjectName;
                    status.Avg = randomization != null ? randomization.Avg : 0;
                });
                x.Target = All.Where(q => q.ProjectId == x.ProjectId && q.DeletedDate == null).Count();
            });

            return result;
        }

        public List<DashboardRecruitmentStatusDisplayDto> GetDashboardRecruitmentStatus(int projectId)
        {
            var pro = _context.Project.First(x => x.Id == projectId);
            var project = new List<Data.Entities.Master.Project>();
            if (pro.ParentProjectId == null)
            {
                project = _context.Project.Where(x => x.ParentProjectId == projectId && _context.ProjectRight.Any(a => a.ProjectId == x.Id
                                                  && a.UserId == _jwtTokenAccesser.UserId && a.RoleId == _jwtTokenAccesser.RoleId
                                                  && a.DeletedDate == null && a.RollbackReason == null) && x.DeletedDate == null).ToList();
            }
            else
            {
                project = _context.Project.Where(x => x.Id == projectId).ToList();
            }

            var randomization = All.Include(q => q.Project)
                .Where(q => project.Select(x => x.Id).Contains(q.ProjectId) && q.DeletedDate == null)
                .Select(g => new DashboardRecruitmentStatusDisplayDto
                {
                    ProjectName = g.Project.ProjectName,
                    ScreeningDate = g.DateOfScreening,
                    RandomizationDate = g.DateOfRandomization,
                    ScreeningMonth = g.DateOfScreening.Value.ToString("MMM yyyy"),
                    RandomizationMonth = g.DateOfRandomization.Value.ToString("MMM yyyy"),
                }).ToList();

            var screening = randomization.Where(y => y.ScreeningDate != null).GroupBy(x => x.ScreeningMonth)
                .Select(g => new DashboardRecruitmentStatusDisplayDto
                {
                    ScreeningMonth = g.Key,
                    ScreeningMonthNo = DateTime.ParseExact(g.Key, "MMM yyyy", CultureInfo.CurrentCulture).Month,
                    ScreeningDataCount = g.Count(),
                    DisplayName = "Screening"
                }).OrderBy(x => x.ScreeningMonthNo).ToList();

            var randomizationCount = randomization.Where(y => y.RandomizationDate != null).GroupBy(x => x.RandomizationMonth)
               .Select(g => new DashboardRecruitmentStatusDisplayDto
               {
                   ScreeningMonth = g.Key,
                   ScreeningMonthNo = DateTime.ParseExact(g.Key, "MMM yyyy", CultureInfo.CurrentCulture).Month,
                   ScreeningDataCount = g.Count(),
                   DisplayName = "Randomization"
               }).OrderBy(x => x.ScreeningMonthNo).ToList();

            screening.AddRange(randomizationCount);

            return screening;
        }

        public DashboardRecruitmentRateDto GetDashboardRecruitmentRate(int projectId)
        {
            DateTime today = DateTime.Today;
            var pro = _context.Project.Where(x => x.Id == projectId).FirstOrDefault();

            var randomization = All.Include(q => q.Project)
               .Where(q => q.ProjectId == projectId && q.DeletedDate == null)
               .Select(g => new DashboardRecruitmentStatusDisplayDto
               {
                   ProjectName = g.Project.ProjectName,
                   ScreeningDate = g.DateOfScreening,
                   RandomizationDate = g.DateOfRandomization,
                   ScreeningMonth = g.DateOfScreening.Value.ToString("MMM yyyy"),
                   RandomizationMonth = g.DateOfRandomization.Value.ToString("MMM yyyy"),
               }).ToList();

            var screeningCount = new DashboardRecruitmentRateDto();
            var randomizationCount = new DashboardRecruitmentRateDto();

            var screeningFilter = randomization.Where(y => y.ScreeningDate != null).ToList();
            if (screeningFilter.Any())
            {
                var AveragePerMonthScreening = screeningFilter.Count / (pro.Recruitment == null ? 1 : (int)pro.Recruitment);
                screeningCount = screeningFilter.GroupBy(x => x.ScreeningMonth)
                   .Select(g => new DashboardRecruitmentRateDto
                   {
                       ScreeningDataCount = g.Count(),
                       ScreeningAvgValue = AveragePerMonthScreening >= 1 ? (int?)Math.Round((decimal)(g.Count() * 100) / AveragePerMonthScreening, 2) : 0,
                       ScreeningMonth = DateTime.ParseExact(g.Key, "MMM yyyy", CultureInfo.CurrentCulture).Month,
                       IsScreeningAchive = g.Count() >= (screeningFilter.Count / (pro.Recruitment == null ? 1 : (int)pro.Recruitment))
                   }).FirstOrDefault(x => x.ScreeningMonth == today.Month);
            }

            var randomizationFilter = randomization.Where(y => y.RandomizationDate != null).ToList();
            if (randomizationFilter.Any())
            {
                var AveragePerMonth = randomizationFilter.Count / (pro.Recruitment == null ? 1 : (int)pro.Recruitment);

                randomizationCount = randomizationFilter.GroupBy(x => x.RandomizationMonth)
               .Select(g => new DashboardRecruitmentRateDto
               {
                   RandomizationDataCount = g.Count(),
                   RandomizationAvgValue = AveragePerMonth >= 1 ? (int?)Math.Round((decimal)(g.Count() * 100) / AveragePerMonth, 2) : 0,
                   RandomizationMonth = DateTime.ParseExact(g.Key, "MMM yyyy", CultureInfo.CurrentCulture).Month,
                   IsRandomizationAchive = g.Count() >= (randomizationFilter.Count / (pro.Recruitment == null ? 1 : (int)pro.Recruitment))
               }).FirstOrDefault(x => x.RandomizationMonth == today.Month);
            }

            if (randomizationCount != null)
            {
                if (screeningCount == null)
                    screeningCount = new DashboardRecruitmentRateDto();
                screeningCount.RandomizationDataCount = randomizationCount.RandomizationDataCount;
                screeningCount.RandomizationAvgValue = randomizationCount.RandomizationAvgValue;
                screeningCount.RandomizationMonth = randomizationCount.RandomizationMonth;
                screeningCount.IsRandomizationAchive = randomizationCount.IsRandomizationAchive;
            }

            return screeningCount;
        }


        // Dashboard chart for target status
        public List<DashboardPatientStatusDto> GetDashboardBoxData(int projectId, int countryId, int siteId)
        {
            var pro = _context.Project.Where(x => x.Id == projectId).FirstOrDefault();
            var project = new List<Data.Entities.Master.Project>();
            if (pro.ParentProjectId == null)
            {
                project = _context.Project.Where(x => x.ParentProjectId == projectId && _context.ProjectRight.Any(a => a.ProjectId == x.Id
                                                  && a.UserId == _jwtTokenAccesser.UserId && a.RoleId == _jwtTokenAccesser.RoleId
                                                  && a.DeletedDate == null && a.RollbackReason == null) && x.DeletedDate == null).ToList();
            }
            else
            {
                project = _context.Project.Where(x => x.Id == projectId).ToList();
            }

            var result = new List<DashboardPatientStatusDto>();
            project.ForEach(t =>
            {
                var data = new DashboardPatientStatusDto();
                data.ProjectId = t.Id;
                data.ProjectName = t.ProjectCode;
                data.Target = t.AttendanceLimit;
                data.IsParentProject = pro.ParentProjectId == null;
                data.ParentProjectTarget = pro.AttendanceLimit;
                data.StatusList = new List<DashboardPatientStatusDisplayDto>();
                result.Add(data);
            });

            result.ForEach(x =>
            {
                var ScreeningPatientStatus = Enum.GetValues(typeof(ScreeningPatientStatus))
                                           .Cast<ScreeningPatientStatus>().Select(e => new DashboardPatientStatusDisplayDto
                                           {
                                               Id = Convert.ToInt16(e),
                                               DisplayName = e.GetDescription()
                                           }).ToList();

                x.StatusList.AddRange(ScreeningPatientStatus);

                x.StatusList.ForEach(status =>
                {
                    var randomization = All.Include(q => q.Project)
                    .Where(q => q.ProjectId == x.ProjectId && q.DeletedDate == null && (int)q.PatientStatusId == status.Id)
                    .GroupBy(y => y.PatientStatusId)
                    .Select(g => new DashboardPatientStatusDisplayDto
                    {
                        Id = (int)g.Key,
                        ProjectName = x.ProjectName,
                        DisplayName = g.Key.GetDescription(),
                        Avg = g.Count()
                    }).FirstOrDefault();

                    status.ProjectName = x.ProjectName;
                    status.Avg = randomization != null ? randomization.Avg : 0;
                });
            });

            return result;
        }

        public void AddRandomizationUser(UserDto userDto, CommonResponceView userdetails)
        {
            var user = _mapper.Map<Data.Entities.UserMgt.User>(userDto);
            user.Id = userdetails.Id;
            _userRepository.Add(user);
            UserRole userRole = new UserRole();
            userRole.UserId = userdetails.Id;
            userRole.UserRoleId = 2;
            _userRoleRepository.Add(userRole);
        }

        public void AddRandomizationUserLAR(UserDto userLarDto, CommonResponceView userLardetails)
        {
            var userLar = _mapper.Map<Data.Entities.UserMgt.User>(userLarDto);
            userLar.Id = userLardetails.Id;
            _userRepository.Add(userLar);
            UserRole userLarRole = new UserRole();
            userLarRole.UserId = userLardetails.Id;
            userLarRole.UserRoleId = _context.SecurityRole.FirstOrDefault(c => c.RoleShortName == "LAR")?.Id ?? 0;
            _userRoleRepository.Add(userLarRole);
        }

        public RandomizationDto GetRandomizationNumberIWRS(RandomizationDto randomizationDto)
        {
            var randomizationNumberDto = GenerateRandomizationNumber(randomizationDto.Id);
            randomizationDto.RandomizationNumber = randomizationNumberDto.RandomizationNumber;
            randomizationDto.ProductCode = randomizationNumberDto.ProductCode;
            randomizationDto.KitNo = randomizationNumberDto.KitNo;
            return randomizationDto;
        }
        public RandomizationDto SetKitNumber(RandomizationDto obj)
        {
            var numbersetting = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == obj.ParentProjectId && x.DeletedDate == null).FirstOrDefault();
            var randomizationNumberDto = GenerateRandomizationNumber(obj.Id);
            obj.IsDoseWiseKit = randomizationNumberDto.IsDoseWiseKit;
            obj.KitDoseList = randomizationNumberDto.KitDoseList;

            if (!ValidateRandomizationIdForIWRS(obj))
            {
                UpdateRandmizationKitNotAssigned(obj);
                obj.ErrorMessage = "Randmization Number Already assigned please try again!";
                return obj;
            }
            if (obj.RandomizationNumber != randomizationNumberDto.RandomizationNumber)
            {
                UpdateRandmizationKitNotAssigned(obj);
                obj.ErrorMessage = "Randmization Number Already assigned please try again!";
                return obj;
            }
            var checkduplicate = Duplicate(obj, obj.ParentProjectId);
            if (!string.IsNullOrEmpty(checkduplicate))
            {
                UpdateRandmizationKitNotAssigned(obj);
                obj.ErrorMessage = checkduplicate;
                return obj;
            }
            obj.RandomizationNumber = randomizationNumberDto.RandomizationNumber;
            if (!string.IsNullOrEmpty(randomizationNumberDto.RandomizationNumber) && randomizationNumberDto.IsIGT)
                UpdateRandomizationIdForIWRS(obj);

            if (randomizationNumberDto.IsDoseWiseKit)
            {
                foreach (var item in obj.KitDoseList)
                {
                    var kitdata = _context.SupplyManagementKITDetail.Where(x => x.Id == item.KitDetailId).FirstOrDefault();
                    if (kitdata != null && kitdata.RandomizationId == null)
                    {
                        kitdata.RandomizationId = obj.Id;
                        kitdata.Status = KitStatus.Allocated;
                        _context.SupplyManagementKITDetail.Update(kitdata);

                        var supplyManagementVisitKITDetailDto = new SupplyManagementVisitKITDetailDto
                        {
                            RandomizationId = obj.Id,
                            ProjectDesignVisitId = item.VisitId,
                            KitNo = item.kitNo,
                            ProductCode = item.ProductCode,
                            SupplyManagementKITDetailId = kitdata.Id,
                            SupplyManagementShipmentId = kitdata.SupplyManagementShipmentId,
                            IpAddress = _jwtTokenAccesser.IpAddress,
                            TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")
                        };
                        _supplyManagementKITRepository.InsertKitRandomizationDetail(supplyManagementVisitKITDetailDto);

                        SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                        history.SupplyManagementKITDetailId = kitdata.Id;
                        history.SupplyManagementShipmentId = kitdata.SupplyManagementShipmentId;
                        history.Status = KitStatus.Allocated;
                        history.RoleId = _jwtTokenAccesser.RoleId;
                        history.RandomizationId = obj.Id;
                        _supplyManagementKITRepository.InsertKitHistory(history);
                        _context.Save();
                    }
                }
            }
            else
            {
                obj.ProductCode = randomizationNumberDto.ProductCode;
                obj.VisitId = randomizationNumberDto.VisitId;
                obj.KitCount = randomizationNumberDto.KitCount;
                obj.KitDetailId = randomizationNumberDto.KitDetailId;

                if (randomizationNumberDto.IsIWRS && !string.IsNullOrEmpty(randomizationNumberDto.RandomizationNumber))
                {
                    if (numbersetting != null && numbersetting.KitCreationType == KitCreationType.KitWise)
                    {
                        var kitdata = _context.SupplyManagementKITDetail.Where(x => x.Id == randomizationNumberDto.KitDetailId).FirstOrDefault();
                        if (kitdata != null && kitdata.RandomizationId == null)
                        {
                            kitdata.RandomizationId = obj.Id;
                            kitdata.Status = KitStatus.Allocated;
                            _context.SupplyManagementKITDetail.Update(kitdata);
                            var supplyManagementVisitKITDetailDto = new SupplyManagementVisitKITDetailDto
                            {
                                RandomizationId = obj.Id,
                                ProjectDesignVisitId = randomizationNumberDto.VisitId,
                                KitNo = randomizationNumberDto.KitNo,
                                ProductCode = randomizationNumberDto.ProductCode,
                                SupplyManagementKITDetailId = kitdata.Id,
                                SupplyManagementShipmentId = kitdata.SupplyManagementShipmentId,
                                IpAddress = _jwtTokenAccesser.IpAddress,
                                TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")
                            };
                            _supplyManagementKITRepository.InsertKitRandomizationDetail(supplyManagementVisitKITDetailDto);

                            SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                            history.SupplyManagementKITDetailId = kitdata.Id;
                            history.SupplyManagementShipmentId = kitdata.SupplyManagementShipmentId;
                            history.Status = KitStatus.Allocated;
                            history.RoleId = _jwtTokenAccesser.RoleId;
                            history.RandomizationId = obj.Id;
                            _supplyManagementKITRepository.InsertKitHistory(history);
                            _context.Save();
                            obj.KitNo = randomizationNumberDto.KitNo;
                        }
                    }
                    else
                    {
                        var kitdata = _context.SupplyManagementKITSeriesDetail.Where(x => x.Id == randomizationNumberDto.KitDetailId).FirstOrDefault();
                        if (kitdata != null && kitdata.RandomizationId == null)
                        {
                            kitdata.RandomizationId = obj.Id;
                            _context.SupplyManagementKITSeriesDetail.Update(kitdata);

                            var kit = _context.SupplyManagementKITSeries.Where(x => x.Id == kitdata.SupplyManagementKITSeriesId && x.RandomizationId == null).FirstOrDefault();
                            if (kit != null)
                            {
                                kit.RandomizationId = obj.Id;
                                kit.Status = KitStatus.Allocated;
                                _context.SupplyManagementKITSeries.Update(kit);

                                var supplyManagementVisitKITDetailDto = new SupplyManagementVisitKITSequenceDetailDto
                                {
                                    RandomizationId = obj.Id,
                                    ProjectDesignVisitId = randomizationNumberDto.VisitId,
                                    KitNo = randomizationNumberDto.KitNo,
                                    ProductCode = randomizationNumberDto.ProductCode,
                                    SupplyManagementKITSeriesdetailId = kitdata.Id,
                                    SupplyManagementShipmentId = kit.SupplyManagementShipmentId,
                                    IpAddress = _jwtTokenAccesser.IpAddress,
                                    TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")
                                };
                                _supplyManagementKITRepository.InsertKitSequenceRandomizationDetail(supplyManagementVisitKITDetailDto);

                                SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                                history.SupplyManagementKITSeriesId = kitdata.SupplyManagementKITSeriesId;
                                history.Status = KitStatus.Allocated;
                                history.RoleId = _jwtTokenAccesser.RoleId;
                                history.RandomizationId = obj.Id;
                                _supplyManagementKITRepository.InsertKitSequenceHistory(history);
                                _context.Save();
                                obj.KitNo = randomizationNumberDto.KitNo;
                            }
                        }
                    }
                }
            }

            var randomization = All.Where(x => x.Id == obj.Id).FirstOrDefault();
            if (randomization != null && randomizationNumberDto.IsIGT && !string.IsNullOrEmpty(randomizationNumberDto.ProductCode))
            {
                randomization.ProductCode = randomizationNumberDto.ProductCode;
                _context.Randomization.Update(randomization);
                _context.Save();
            }
            return obj;
        }
        public void UpdateRandmizationKitNotAssigned(RandomizationDto obj)
        {
            SupplyManagementUploadFileDetail data = new SupplyManagementUploadFileDetail();

            var randomization = All.First(x => x.Id == obj.Id);
            randomization.DateOfRandomization = null;
            randomization.RandomizationNumber = null;
            randomization.ProductCode = null;
            _context.Randomization.Update(randomization);
            _context.Save();

            var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.First(x => x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve);
            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
            {
                data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId
               && x.DeletedDate == null && x.RandomizationId == obj.Id && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

            }
            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
            {
                var country = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == country.ManageSiteId).FirstOrDefault();
                if (site != null)
                {
                    data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                       && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                      && x.DeletedDate == null && x.RandomizationId == obj.Id && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                }
            }

            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
            {
                data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                && x.DeletedDate == null && x.RandomizationId == obj.Id && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

            }

            if (data != null)
            {
                data.RandomizationId = null;
                _context.SupplyManagementUploadFileDetail.Update(data);
                _context.Save();
            }
        }
        public void SendRandomizationIWRSEMail(RandomizationDto obj)
        {
            SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
            IwrsEmailModel iWRSEmailModel = new IwrsEmailModel();
            var study = _context.Project.Where(x => x.Id == obj.ParentProjectId).FirstOrDefault();
            if (study != null)
            {
                var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive && x.ProjectId == obj.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.SubjectRandomization).ToList();
                if (emailconfiglist.Any())
                {
                    var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                    if (siteconfig.Count > 0)
                    {
                        emailconfig = siteconfig.Find(x => x.SiteId == obj.ProjectId);
                    }
                    else
                    {
                        emailconfig = emailconfiglist.FirstOrDefault();
                    }
                    var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                    if (details.Any())
                    {
                        iWRSEmailModel.StudyCode = _context.Project.FirstOrDefault(x => x.Id == obj.ParentProjectId)?.ProjectCode;

                        var site = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                        if (site != null)
                        {
                            iWRSEmailModel.SiteCode = site.ProjectCode;
                            var managesite = _context.ManageSite.Where(x => x.Id == site.ManageSiteId).FirstOrDefault();
                            if (managesite != null)
                            {
                                iWRSEmailModel.SiteName = managesite.SiteName;
                            }
                        }
                        iWRSEmailModel.ScreeningNo = obj.ScreeningNumber;
                        iWRSEmailModel.RandomizationNo = obj.RandomizationNumber;
                        iWRSEmailModel.KitNo = obj.KitNo;
                        _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                        foreach (var item in details)
                        {
                            SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                            history.SupplyManagementEmailConfigurationDetailId = item.Id;
                            _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);
                            _context.Save();
                        }
                    }
                }
            }
        }

        public void SendRandomizationThresholdEMail(RandomizationDto obj)
        {
            var threshold = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == obj.ParentProjectId && x.DeletedDate == null).FirstOrDefault();
            if (threshold != null && obj.KitCount < threshold.ThresholdValue)
            {
                SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
                IwrsEmailModel iWRSEmailModel = new IwrsEmailModel();
                var study = _context.Project.Where(x => x.Id == obj.ParentProjectId).FirstOrDefault();
                if (study != null)
                {
                    var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive && x.ProjectId == obj.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.Threshold).ToList();
                    if (emailconfiglist.Any())
                    {
                        var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                        if (siteconfig.Count > 0)
                        {
                            emailconfig = siteconfig.Find(x => x.SiteId == obj.ProjectId);
                        }
                        else
                        {
                            emailconfig = emailconfiglist.FirstOrDefault();
                        }
                        var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                        if (details.Any())
                        {
                            iWRSEmailModel.StudyCode = _context.Project.FirstOrDefault(x => x.Id == obj.ParentProjectId)?.ProjectCode;

                            var site = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                            if (site != null)
                            {
                                iWRSEmailModel.SiteCode = site.ProjectCode;
                                var managesite = _context.ManageSite.Where(x => x.Id == site.ManageSiteId).FirstOrDefault();
                                if (managesite != null)
                                {
                                    iWRSEmailModel.SiteName = managesite.SiteName;
                                }
                            }
                            iWRSEmailModel.ThresholdValue = (int)threshold.ThresholdValue;
                            iWRSEmailModel.RemainingKit = obj.KitCount - 1;

                            _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                            foreach (var item in details)
                            {
                                SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                                history.SupplyManagementEmailConfigurationDetailId = item.Id;
                                _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);

                                SupplyManagementThresholdHistory thresoldhistory = new SupplyManagementThresholdHistory();
                                thresoldhistory.ProjectId = obj.ParentProjectId;
                                thresoldhistory.SiteId = obj.ProjectId;
                                _context.SupplyManagementThresholdHistory.Add(thresoldhistory);
                                _context.Save();
                            }
                        }
                    }
                }
            }
        }
        public async Task SendRandomizationThresholdEmailSchedule()
        {
            int? projectId = 0;
            int? recordId = 0;
            string recurenceType = string.Empty;
            try
            {
                var data = await _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null).ToListAsync();
                if (data != null && data.Count > 0)
                {
                    foreach (var threshold in data)
                    {
                        var sitedata = await _context.Project.Where(s => s.ParentProjectId == threshold.ProjectId && s.DeletedDate == null && (s.Status == MonitoringSiteStatus.Active || s.Status == MonitoringSiteStatus.Approved)).ToListAsync();
                        if (sitedata.Any())
                        {
                            foreach (var site in sitedata.Select(s => s.Id))
                            {
                                int KitCount = 0;
                                bool Issuccess = false;
                                SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
                                IwrsEmailModel iWRSEmailModel = new IwrsEmailModel();
                                SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();

                                if (threshold.KitCreationType == KitCreationType.SequenceWise)
                                {

                                    KitCount = _context.SupplyManagementKITSeriesDetail
                                        .Include(x => x.SupplyManagementKITSeries)
                                        .ThenInclude(x => x.SupplyManagementShipment)
                                        .ThenInclude(x => x.SupplyManagementRequest)
                                        .Where(x => x.DeletedDate == null
                                                        && x.SupplyManagementKITSeries.ProjectId == threshold.ProjectId
                                                        && !x.SupplyManagementKITSeries.IsRetension
                                                        && x.SupplyManagementKITSeries.DeletedDate == null
                                                        && x.SupplyManagementKITSeries.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == site
                                                        && (x.SupplyManagementKITSeries.Status == KitStatus.WithIssue || x.SupplyManagementKITSeries.Status == KitStatus.WithoutIssue)
                                                        && x.RandomizationId == null).Count();
                                }

                                if (threshold.KitCreationType == KitCreationType.KitWise)
                                {
                                    KitCount = _context.SupplyManagementKITDetail.
                                                Include(x => x.SupplyManagementKIT).
                                                ThenInclude(x => x.PharmacyStudyProductType).
                                                ThenInclude(x => x.ProductType)
                                                .Include(x => x.SupplyManagementShipment)
                                                .ThenInclude(x => x.SupplyManagementRequest)
                                                .Where(x => x.DeletedDate == null
                                                          && x.SupplyManagementKIT.DeletedDate == null
                                                          && x.SupplyManagementKIT.ProjectId == threshold.ProjectId
                                                          && !x.IsRetension
                                                          && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == site
                                                          && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                                          && x.RandomizationId == null).Count();
                                }

                                var study = _context.Project.Where(x => x.Id == threshold.ProjectId).FirstOrDefault();
                                if (study != null && KitCount < threshold.ThresholdValue)
                                {
                                    var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive && x.ProjectId == threshold.ProjectId && x.Triggers == SupplyManagementEmailTriggers.Threshold).ToList();
                                    if (emailconfiglist != null && emailconfiglist.Count > 0)
                                    {
                                        var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                                        if (siteconfig.Count > 0)
                                        {
                                            emailconfig = siteconfig.Find(x => x.SiteId == site);
                                        }
                                        else
                                        {
                                            emailconfig = emailconfiglist.FirstOrDefault();
                                        }

                                        projectId = threshold.ProjectId;
                                        recurenceType = emailconfig.RecurrenceType.GetDescription();
                                        recordId = site;

                                        var thresoldHistory = _context.SupplyManagementThresholdHistory.Where(s => s.ProjectId == threshold.ProjectId && s.SiteId == site).OrderByDescending(s => s.Id).FirstOrDefault();
                                        if (thresoldHistory != null)
                                        {
                                            supplyManagementEmailScheduleLog.ProjectId = threshold.ProjectId;
                                            supplyManagementEmailScheduleLog.TriggerType = emailconfig.Triggers.GetDescription();
                                            supplyManagementEmailScheduleLog.RecurrenceType = emailconfig.RecurrenceType.GetDescription();
                                            supplyManagementEmailScheduleLog.Message = "Threshold Schedule Start " + DateTime.Now;
                                            supplyManagementEmailScheduleLog.RecordId = site;
                                            _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                                            _context.Save();

                                            if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Daily)
                                            {
                                                Issuccess = true;
                                            }
                                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.AlternateDay)
                                            {
                                                DateTime start = Convert.ToDateTime(thresoldHistory.CreatedDate);
                                                DateTime end = DateTime.Now;
                                                TimeSpan span = end.Date - start.Date;
                                                double difference = span.TotalDays;
                                                if (difference % 2 == 0)
                                                    Issuccess = true;
                                            }
                                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Weekly)
                                            {
                                                DateTime start = Convert.ToDateTime(thresoldHistory.CreatedDate);
                                                DateTime end = DateTime.Now.Date;
                                                while (start < end)
                                                {
                                                    start = start.AddDays(7);
                                                    if (start.Date == end.Date)
                                                    {
                                                        Issuccess = true;
                                                        start = end;
                                                    }
                                                }
                                            }
                                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.FifteenDays)
                                            {
                                                DateTime start = Convert.ToDateTime(thresoldHistory.CreatedDate);
                                                DateTime end = DateTime.Now.Date;
                                                while (start < end)
                                                {
                                                    start = start.AddDays(15);
                                                    if (start.Date == end.Date)
                                                    {
                                                        Issuccess = true;
                                                        start = end;
                                                    }
                                                }
                                            }
                                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Monthly)
                                            {
                                                DateTime start = Convert.ToDateTime(thresoldHistory.CreatedDate);
                                                DateTime end = DateTime.Now.Date;
                                                while (start < end)
                                                {
                                                    start = start.AddMonths(1);
                                                    if (start.Date == end.Date)
                                                    {
                                                        Issuccess = true;
                                                        start = end;
                                                    }
                                                }
                                            }
                                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.EveryTwoMonth)
                                            {
                                                DateTime start = Convert.ToDateTime(thresoldHistory.CreatedDate);
                                                DateTime end = DateTime.Now.Date;
                                                while (start < end)
                                                {
                                                    start = start.AddMonths(2);
                                                    if (start.Date == end.Date)
                                                    {
                                                        Issuccess = true;
                                                        start = end;
                                                    }
                                                }
                                            }
                                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Quarterly)
                                            {
                                                DateTime start = Convert.ToDateTime(thresoldHistory.CreatedDate);
                                                DateTime end = DateTime.Now.Date;
                                                while (start < end)
                                                {
                                                    start = start.AddMonths(3);
                                                    if (start.Date == end.Date)
                                                    {
                                                        Issuccess = true;
                                                        start = end;
                                                    }
                                                }
                                            }
                                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.EverySixMonth)
                                            {
                                                DateTime start = Convert.ToDateTime(thresoldHistory.CreatedDate);
                                                DateTime end = DateTime.Now.Date;
                                                while (start < end)
                                                {
                                                    start = start.AddMonths(6);
                                                    if (start.Date == end.Date)
                                                    {
                                                        Issuccess = true;
                                                        start = end;
                                                    }
                                                }
                                            }
                                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Yearly)
                                            {
                                                DateTime start = Convert.ToDateTime(thresoldHistory.CreatedDate);
                                                DateTime end = DateTime.Now.Date;
                                                while (start < end)
                                                {
                                                    start = start.AddYears(1);
                                                    if (start.Date == end.Date)
                                                    {
                                                        Issuccess = true;
                                                        start = end;
                                                    }
                                                }
                                            }
                                            if (Issuccess)
                                            {
                                                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                                                if (details.Any())
                                                {
                                                    iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == threshold.ProjectId).FirstOrDefault().ProjectCode;
                                                    var project = _context.Project.Where(x => x.Id == site).FirstOrDefault();
                                                    if (project != null)
                                                    {
                                                        iWRSEmailModel.SiteCode = project.ProjectCode;
                                                        var managesite = _context.ManageSite.Where(x => x.Id == project.ManageSiteId).FirstOrDefault();
                                                        if (managesite != null)
                                                        {
                                                            iWRSEmailModel.SiteName = managesite.SiteName;
                                                        }
                                                    }
                                                    iWRSEmailModel.ThresholdValue = (int)threshold.ThresholdValue;
                                                    iWRSEmailModel.RemainingKit = KitCount;

                                                    _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                                                    foreach (var item in details)
                                                    {
                                                        SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                                                        history.SupplyManagementEmailConfigurationDetailId = item.Id;
                                                        _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);
                                                        _context.Save();
                                                    }
                                                }
                                            }

                                            var supplyManagementEmailScheduleLog1 = new SupplyManagementEmailScheduleLog();
                                            supplyManagementEmailScheduleLog1.ProjectId = threshold.ProjectId;
                                            supplyManagementEmailScheduleLog1.TriggerType = emailconfig.Triggers.GetDescription();
                                            supplyManagementEmailScheduleLog1.RecurrenceType = emailconfig.RecurrenceType.GetDescription();
                                            supplyManagementEmailScheduleLog1.Message = "Threshold Schedule end " + DateTime.Now;
                                            supplyManagementEmailScheduleLog1.RecordId = site;
                                            _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog1);
                                            _context.Save();

                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();
                supplyManagementEmailScheduleLog.Message = ex.Message.ToString();
                supplyManagementEmailScheduleLog.TriggerType = SupplyManagementEmailTriggers.Threshold.GetDescription();
                supplyManagementEmailScheduleLog.ProjectId = projectId;
                supplyManagementEmailScheduleLog.RecurrenceType = recurenceType;
                supplyManagementEmailScheduleLog.RecordId = recordId;
                _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                _context.Save();
            }
        }
        public bool CheckDUplicateRandomizationNumber(RandomizationDto obj)
        {
            var randomization = _context.Randomization.Include(x => x.Project).Where(x => x.DeletedDate == null && x.RandomizationNumber == obj.RandomizationNumber && x.Project.ParentProjectId == obj.ParentProjectId).ToList();
            var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.DeletedDate == null && x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
            if (SupplyManagementUploadFile == null)
            {
                return false;
            }

            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site || SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
            {
                randomization = randomization.Where(x => x.ProjectId == obj.ProjectId).ToList();
            }

            if (randomization.Count > 1)
            {
                UpdateRandmizationKitNotAssigned(obj);
                RevertKitData(obj);
                return false;
            }
            return true;
        }

        public void SetFactorMappingData(Randomization randomizationDto)
        {
            var parentproject = _context.Project.Where(x => x.Id == randomizationDto.ProjectId).FirstOrDefault();
            var setting = _context.RandomizationNumberSettings.Where(x => x.ProjectId == parentproject.ParentProjectId).FirstOrDefault();
            if (setting != null && (setting.IsIGT || setting.IsIWRS))
            {
                var mappingdata = _context.SupplyManagementFactorMapping.Where(s => s.ProjectId == parentproject.ParentProjectId && s.DeletedDate == null).ToList();
                if (mappingdata.Count > 0)
                {
                    foreach (var item in mappingdata)
                    {
                        var Isexist = false;
                        var screeningEntry = _context.ScreeningEntry.Where(x => x.RandomizationId == randomizationDto.Id).FirstOrDefault();
                        if (screeningEntry != null)
                        {
                            var screeningvisit = _context.ScreeningVisit.Where(x => x.ScreeningEntryId == screeningEntry.Id && x.ProjectDesignVisitId == item.ProjectDesignVisitId).FirstOrDefault();
                            if (screeningvisit != null)
                            {
                                var screeningTemplate = _context.ScreeningTemplate.Where(x => x.Status != ScreeningTemplateStatus.InProcess && x.Status != ScreeningTemplateStatus.Pending && x.ScreeningVisitId == screeningvisit.Id && x.ProjectDesignTemplateId == item.ProjectDesignTemplateId).FirstOrDefault();
                                if (screeningTemplate != null)
                                {
                                    var screeningtemplateValue = _context.ScreeningTemplateValue.Where(x => x.ScreeningTemplateId == screeningTemplate.Id && x.ProjectDesignVariableId == item.ProjectDesignVariableId).FirstOrDefault();
                                    if (screeningtemplateValue != null && string.IsNullOrEmpty(randomizationDto.RandomizationNumber))
                                    {
                                        if (item.Factor == Fector.Age)
                                        {
                                            randomizationDto.Agefactor = screeningtemplateValue.Value;
                                            Isexist = true;
                                        }
                                        if (item.Factor == Fector.Weight)
                                        {
                                            randomizationDto.Weightfactor = screeningtemplateValue.Value;
                                            Isexist = true;
                                        }
                                        if (item.Factor == Fector.Dose)
                                        {
                                            randomizationDto.Dosefactor = screeningtemplateValue.Value;
                                            Isexist = true;
                                        }
                                        if (item.Factor == Fector.BMI)
                                        {
                                            randomizationDto.BMIfactor = screeningtemplateValue.Value;
                                            Isexist = true;
                                        }
                                        if (!string.IsNullOrEmpty(screeningtemplateValue.Value) && !Isexist)
                                        {
                                            var screeningTemplateValueChild = _context.ProjectDesignVariableValue.Where(x => x.Id == Convert.ToInt32(screeningtemplateValue.Value) && x.DeletedDate == null).FirstOrDefault();

                                            if (screeningTemplateValueChild != null)
                                            {

                                                if (item.Factor == Fector.Diatory)
                                                {
                                                    if (screeningTemplateValueChild.ValueName.ToLower().Contains("non"))
                                                    {
                                                        randomizationDto.Diatoryfactor = DaitoryFector.NonVeg;
                                                    }
                                                    if (screeningTemplateValueChild.ValueName.ToLower().Contains("veg"))
                                                    {
                                                        randomizationDto.Diatoryfactor = DaitoryFector.Veg;
                                                    }
                                                }
                                                if (item.Factor == Fector.Joint)
                                                {
                                                    if (screeningTemplateValueChild.ValueName.ToLower().Contains("knee"))
                                                    {
                                                        randomizationDto.Jointfactor = Jointfactor.Knee;
                                                    }
                                                    if (screeningTemplateValueChild.ValueName.ToLower().Contains("low") || screeningTemplateValueChild.ValueName.ToLower().Contains("back"))
                                                    {
                                                        randomizationDto.Jointfactor = Jointfactor.LowBack;
                                                    }

                                                }
                                                if (item.Factor == Fector.Gender)
                                                {

                                                    if (screeningTemplateValueChild.ValueName.ToLower().Contains("fe"))
                                                    {
                                                        randomizationDto.Genderfactor = Gender.Female;
                                                    }
                                                    if (screeningTemplateValueChild.ValueName.ToLower() == "male")
                                                    {
                                                        randomizationDto.Genderfactor = Gender.Male;
                                                    }
                                                }
                                                if (item.Factor == Fector.Eligibility)
                                                {
                                                    if (screeningTemplateValueChild.ValueName.ToLower().Contains("yes"))
                                                    {
                                                        randomizationDto.Eligibilityfactor = Eligibilityfactor.Yes;
                                                    }
                                                    if (screeningTemplateValueChild.ValueName.ToLower().Contains("no"))
                                                    {
                                                        randomizationDto.Eligibilityfactor = Eligibilityfactor.No;
                                                    }

                                                }
                                            }
                                        }

                                        Update(randomizationDto);
                                        _context.Save();
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void RevertKitData(RandomizationDto obj)
        {
            var numbersetting = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == obj.ParentProjectId && x.DeletedDate == null).FirstOrDefault();
            if (numbersetting != null && numbersetting.KitCreationType == KitCreationType.KitWise)
            {
                var kitdata = _context.SupplyManagementKITDetail.Where(x => x.Id == obj.KitDetailId).FirstOrDefault();
                if (kitdata != null)
                {
                    var kithistory = _context.SupplyManagementKITDetailHistory.Where(x => x.SupplyManagementKITDetailId == obj.KitDetailId && x.Status != KitStatus.Allocated).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (kithistory != null)
                    {
                        kitdata.RandomizationId = null;
                        kitdata.Status = (KitStatus)kithistory.Status;
                        _context.SupplyManagementKITDetail.Update(kitdata);
                    }

                    var supplyManagementVisitKITDetail = _context.SupplyManagementVisitKITDetail.Where(s => s.SupplyManagementKITDetailId == obj.KitDetailId).FirstOrDefault();
                    if (supplyManagementVisitKITDetail != null)
                    {
                        supplyManagementVisitKITDetail.DeletedBy = _jwtTokenAccesser.UserId;
                        supplyManagementVisitKITDetail.DeletedDate = DateTime.Now;
                        _context.SupplyManagementVisitKITDetail.Update(supplyManagementVisitKITDetail);
                    }
                    var history = _context.SupplyManagementKITDetailHistory.Where(x => x.SupplyManagementKITDetailId == obj.KitDetailId).OrderByDescending(x => x.Id).FirstOrDefault();
                    if (history != null)
                    {
                        history.DeletedBy = _jwtTokenAccesser.UserId;
                        history.DeletedDate = DateTime.Now;
                        _context.SupplyManagementKITDetailHistory.Update(history);
                    }
                    _context.Save();

                }
            }
            else
            {
                var kitdata = _context.SupplyManagementKITSeriesDetail.Where(x => x.Id == obj.KitDetailId).FirstOrDefault();
                if (kitdata != null)
                {
                    kitdata.RandomizationId = null;
                    _context.SupplyManagementKITSeriesDetail.Update(kitdata);

                    var kit = _context.SupplyManagementKITSeries.Where(x => x.Id == kitdata.SupplyManagementKITSeriesId).FirstOrDefault();
                    if (kit != null)
                    {
                        var kithistory = _context.SupplyManagementKITSeriesDetailHistory.Where(x => x.SupplyManagementKITSeriesId == kitdata.SupplyManagementKITSeriesId && x.Status != KitStatus.Allocated).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (kithistory != null)
                        {
                            kit.RandomizationId = null;
                            kit.Status = (KitStatus)kithistory.Status;
                            _context.SupplyManagementKITSeries.Update(kit);
                        }
                        var supplyManagementVisitKITDetail = _context.SupplyManagementVisitKITSequenceDetail.Where(s => s.SupplyManagementKITSeriesdetailId == obj.KitDetailId).FirstOrDefault();
                        if (supplyManagementVisitKITDetail != null)
                        {
                            supplyManagementVisitKITDetail.DeletedBy = _jwtTokenAccesser.UserId;
                            supplyManagementVisitKITDetail.DeletedDate = DateTime.Now;
                            _context.SupplyManagementVisitKITSequenceDetail.Update(supplyManagementVisitKITDetail);
                        }
                        var history = _context.SupplyManagementKITSeriesDetailHistory.Where(x => x.SupplyManagementKITSeriesId == kitdata.SupplyManagementKITSeriesId).OrderByDescending(x => x.Id).FirstOrDefault();
                        if (history != null)
                        {
                            history.DeletedBy = _jwtTokenAccesser.UserId;
                            history.DeletedDate = DateTime.Now;
                            _context.SupplyManagementKITSeriesDetailHistory.Update(history);
                        }
                    }
                    _context.Save();

                }
            }
        }

        public RandomizationDto CheckDuplicateRandomizationNumberIWRS(RandomizationDto obj, RandomizationNumberSettings numerformate)
        {
            var validateduplicate = Duplicate(obj, obj.ProjectId);
            if (!string.IsNullOrEmpty(validateduplicate))
            {
                UpdateRandmizationKitNotAssigned(obj);
                obj.ErrorMessage = "Randmization Number Already assigned please try again!";
                return obj;
            }
            obj = SetKitNumber(obj);
            if (!string.IsNullOrEmpty(obj.ErrorMessage))
            {
                return obj;
            }
            if (numerformate.IsIWRS && string.IsNullOrEmpty(obj.KitNo) && !obj.IsDoseWiseKit)
            {
                UpdateRandmizationKitNotAssigned(obj);
                obj.ErrorMessage = "Kit is not available";
                return obj;
            }
            if (numerformate.IsIWRS && obj.KitDoseList != null && obj.KitDoseList.Count == 0 && obj.IsDoseWiseKit)
            {
                UpdateRandmizationKitNotAssigned(obj);
                obj.ErrorMessage = "Kit is not available";
                return obj;
            }
            if (numerformate.IsIGT && string.IsNullOrEmpty(obj.RandomizationNumber))
            {
                obj.ErrorMessage = "Please upload randomization sheet";
                return obj;

            }
            if (!ValidateRandomizationIdForIWRS(obj) && !obj.IsDoseWiseKit)
            {
                UpdateRandmizationKitNotAssigned(obj);
                RevertKitData(obj);
                obj.ErrorMessage = "Randmization Number Already assigned please try again!";
                return obj;

            }
            if (!CheckDUplicateRandomizationNumber(obj) && !obj.IsDoseWiseKit)
            {
                obj.ErrorMessage = "Randmization Number Already assigned please try again!";
                return obj;
            }

            return obj;
        }

        public List<ScreeningVisitForSubject> GetPatientVisitsForMobile()
        {
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).AsEnumerable().FirstOrDefault();
            if (randomization != null)
            {
                var data = _context.ScreeningVisit.Include(x => x.ScreeningEntry).Include(x => x.ProjectDesignVisit).Include(x => x.ScreeningTemplates).
                            Where(x => x.ScreeningEntry.RandomizationId == randomization.Id && x.DeletedDate == null && x.ProjectDesignVisit.DeletedDate == null
                            && (x.Status == ScreeningVisitStatus.Scheduled || x.Status == ScreeningVisitStatus.ReSchedule ||
                            x.Status == ScreeningVisitStatus.Open || x.Status == ScreeningVisitStatus.InProgress ||
                            x.Status == ScreeningVisitStatus.Missed || x.Status == ScreeningVisitStatus.Withdrawal ||
                            x.Status == ScreeningVisitStatus.OnHold || x.Status == ScreeningVisitStatus.ScreeningFailure || x.Status == ScreeningVisitStatus.Completed)).
                            Select(a => new ScreeningVisitForSubject
                            {
                                VisitName = a.ProjectDesignVisit.DisplayName + Convert.ToString(a.ParentId != null ? "-" + a.RepeatedVisitNumber.ToString() : ""),
                                VisitStatus = a.Status.GetDescription(),
                                ActualDate = (int)a.Status > 3 ? a.VisitStartDate : null,
                                OffOnSite = a.ProjectDesignVisit.OffSite,
                                ScheduleDate = a.ScheduleDate
                            }).ToList();

                return data;
            }
            return new List<ScreeningVisitForSubject>();
        }
    }
}