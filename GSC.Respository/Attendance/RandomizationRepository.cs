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
        private readonly ISupplyManagementKITRepository _supplyManagementKITRepository;
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
            ISupplyManagementKITRepository supplyManagementKITRepository
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
            if (!randomizationNumberDto.IsTestSite && !randomizationNumberDto.IsIGT)
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
                    _randomizationNumberSettingsRepository.Update(projectRandom);
                }
            }
        }

        public void SaveScreeningNumber(Randomization randomization, RandomizationDto randomizationDto)
        {
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            randomizationNumberDto = GenerateScreeningNumber(randomization.Id);
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

                    var projectSeries = _context.ScreeningNumberSettings.Where(x => x.ProjectId == projectidforscreeningNo).FirstOrDefault();//_projectRepository.Find(projectidforscreeningNo);
                    projectSeries.ScreeningNoseries = randomizationNumberDto.ScreeningNoseries + 1;
                    _screeningNumberSettingsRepository.Update(projectSeries);
                }
            }
        }

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
                        return "Please enter the number as length of " + randomizationNumberDto.ScreeningLength.ToString();
                    //return "Please add " + randomizationNumberDto.ScreeningLength.ToString() + " characters in Screening Number";
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
                RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
                randomizationNumberDto = GenerateRandomizationNumber(randomization.Id);
                randomizationNumberDto.RandomizationNumber = randomization.RandomizationNumber;
                if (randomizationNumberDto.IsManualRandomNo == true)
                {
                    if (randomizationNumberDto.RandomizationNumber.Length != randomizationNumberDto.RandomNoLength)
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
            var sitedata = _context.RandomizationNumberSettings.Where(x => x.ProjectId == randomization.ProjectId).FirstOrDefault();//_projectRepository.Find(randomization.ProjectId);
            var studydata = _context.RandomizationNumberSettings.Where(x => x.ProjectId == (int)site.ParentProjectId).FirstOrDefault();//_projectRepository.Find((int)sitedata.ParentProjectId);
            RandomizationNumberDto randomizationNumberDto = new RandomizationNumberDto();
            randomizationNumberDto.ProjectId = randomization.ProjectId;
            randomizationNumberDto.ParentProjectId = (int)site.ParentProjectId;
            randomizationNumberDto.IsManualRandomNo = studydata.IsManualRandomNo;
            randomizationNumberDto.IsSiteDependentRandomNo = studydata.IsSiteDependentRandomNo;
            randomizationNumberDto.RandomNoLength = studydata.RandomNoLength;
            randomizationNumberDto.IsIWRS = studydata.IsIWRS;
            randomizationNumberDto.IsIGT = studydata.IsIGT;
            //randomizationNumberDto.PrefixRandomNo = studydata.PrefixRandomNo;

            if (!studydata.IsIGT)
            {
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
                            randomizationNumberDto.RandomizationNumber = sitedata.PrefixRandomNo + latestno.ToString().PadLeft((int)studydata.RandomNoLength, '0');
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
            }
            else
            {
                var result = _supplyManagementFectorRepository.ValidateSubjecWithFactor(randomization);
                if (result != null)
                {
                    if (!string.IsNullOrEmpty(result.ErrorMessage))
                    {
                        randomizationNumberDto.ErrorMessage = result.ErrorMessage;
                    }
                    if (!string.IsNullOrEmpty(result.Result))
                    {
                        randomizationNumberDto.ErrorMessage = result.Result;

                    }
                    if (string.IsNullOrEmpty(result.ErrorMessage) || string.IsNullOrEmpty(result.Result))
                        randomizationNumberDto = GetRandNoIWRS(studydata.ProjectId, randomization.ProjectId, site.ManageSiteId, result.ProductType, randomizationNumberDto, randomization, studydata.IsIWRS);
                }
            }
            return randomizationNumberDto;
        }
        public RandomizationNumberDto GetRandNoIWRS(int projectid, int siteId, int? countryId, string productType, RandomizationNumberDto randomizationNumberDto, Randomization randomization, bool isIwrs)
        {
            string randno = string.Empty;
            List<SupplyManagementKITDetail> kitdata = new List<SupplyManagementKITDetail>();
            List<SupplyManagementKITSeriesDetail> kitSequencedata = new List<SupplyManagementKITSeriesDetail>();
            SupplyManagementUploadFileVisit singlevisit = new SupplyManagementUploadFileVisit();
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

            if (isIwrs)
            {
                if (!string.IsNullOrEmpty(productType))
                {
                    var productarray = productType.Split(',').ToArray();

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
                        visit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.SiteId == siteId).ToList();
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                    {
                        var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == countryId).FirstOrDefault();
                        if (site != null)
                        {
                            visit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                             && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).ToList();
                        }
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                    {
                        visit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).ToList();
                    }

                    if (visit == null || visit.Count == 0)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }

                    if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                    {
                        kitdata = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                          x.DeletedDate == null
                                          && x.SupplyManagementKIT.ProjectDesignVisitId == visit.FirstOrDefault().ProjectDesignVisitId
                                          && x.SupplyManagementKIT.ProjectId == projectid
                                          && productarray.Contains(x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode)
                                          && x.SupplyManagementShipmentId != null
                                          && x.SupplyManagementKIT.DeletedDate == null
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
                                            && x.SupplyManagementKITSeries.SupplyManagementShipmentId != null
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
                    }


                    foreach (var visititem in visit)
                    {
                        if (productarray.Contains(visititem.Value) && string.IsNullOrEmpty(randomizationNumberDto.KitNo))
                        {
                            randomizationNumberDto.RandomizationNumber = Convert.ToString(visititem.SupplyManagementUploadFileDetail.RandomizationNo);
                            randomizationNumberDto.ProductCode = visititem.Value;

                            if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                            {
                                var kit = kitdata.Where(x => x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode == visititem.Value).OrderBy(x => x.Id).FirstOrDefault();

                                if (kit != null)
                                {
                                    randomizationNumberDto.KitNo = kit.KitNo;
                                    randomizationNumberDto.KitDetailId = kit.Id;
                                    randomizationNumberDto.VisitId = visit.FirstOrDefault().ProjectDesignVisitId;
                                }

                            }
                            else
                            {
                                var kit = kitSequencedata.Where(x => x.PharmacyStudyProductType.ProductType.ProductTypeCode == visititem.Value).OrderBy(x => x.Id).FirstOrDefault();

                                if (kit != null)
                                {
                                    randomizationNumberDto.KitNo = kit.SupplyManagementKITSeries.KitNo;
                                    randomizationNumberDto.KitDetailId = kit.Id;
                                    randomizationNumberDto.VisitId = visit.FirstOrDefault().ProjectDesignVisitId;
                                }

                            }
                        }
                    }
                    return randomizationNumberDto;
                }
                else
                {
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
                        singlevisit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.SiteId == siteId).OrderBy(x => x.Id).FirstOrDefault();
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                    {
                        var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == countryId).FirstOrDefault();
                        if (site != null)
                        {
                            singlevisit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                             && x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).OrderBy(x => x.Id).FirstOrDefault();
                        }
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                    {
                        singlevisit = visit.Where(x => x.SupplyManagementUploadFileDetail.SupplyManagementUploadFile.ProjectId == projectid).OrderBy(x => x.Id).FirstOrDefault();
                    }

                    if (singlevisit == null)
                    {
                        randomizationNumberDto.ErrorMessage = "Please upload randomization sheet";
                        return randomizationNumberDto;
                    }

                    if (supplyManagementKitNumberSettings.KitCreationType == KitCreationType.KitWise)
                    {
                        kitdata = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                          x.DeletedDate == null
                                          && x.SupplyManagementKIT.ProjectDesignVisitId == singlevisit.ProjectDesignVisitId
                                          && x.SupplyManagementKIT.ProjectId == projectid
                                          && x.SupplyManagementShipmentId != null
                                          && x.SupplyManagementKIT.DeletedDate == null
                                          && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId
                                          && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                          && x.RandomizationId == null).OrderBy(x => x.Id).ToList();
                        randomizationNumberDto.KitCount = kitdata.Count;

                        if (kitdata == null || kitdata.Count == 0)
                        {
                            randomizationNumberDto.ErrorMessage = "Kit is not available!";
                            return randomizationNumberDto;
                        }

                        randomizationNumberDto.ProductCode = singlevisit.Value;
                        randomizationNumberDto.RandomizationNumber = Convert.ToString(singlevisit.SupplyManagementUploadFileDetail.RandomizationNo);
                        randomizationNumberDto.KitNo = kitdata.FirstOrDefault().KitNo;
                        randomizationNumberDto.KitDetailId = kitdata.FirstOrDefault().Id;
                        randomizationNumberDto.VisitId = singlevisit.ProjectDesignVisitId;
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
                                            && x.ProjectDesignVisitId == visit.FirstOrDefault().ProjectDesignVisitId
                                            && x.SupplyManagementKITSeries.ProjectId == projectid
                                            && x.SupplyManagementKITSeries.SupplyManagementShipmentId != null
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

                        randomizationNumberDto.ProductCode = singlevisit.Value;
                        randomizationNumberDto.RandomizationNumber = Convert.ToString(singlevisit.SupplyManagementUploadFileDetail.RandomizationNo);
                        randomizationNumberDto.KitNo = kitSequencedata.FirstOrDefault().SupplyManagementKITSeries.KitNo;
                        randomizationNumberDto.KitDetailId = kitSequencedata.FirstOrDefault().Id;
                        randomizationNumberDto.VisitId = singlevisit.ProjectDesignVisitId;
                        return randomizationNumberDto;
                    }
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
                        uploaddetail = data.Where(x => x.SupplyManagementUploadFile.SiteId == siteId).FirstOrDefault();
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
                    {
                        var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == countryId).FirstOrDefault();
                        if (site != null)
                        {
                            uploaddetail = data.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId && x.SupplyManagementUploadFile.ProjectId == projectid).FirstOrDefault();
                        }
                    }
                    if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                    {
                        uploaddetail = data.Where(x => x.SupplyManagementUploadFile.ProjectId == projectid).FirstOrDefault();
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
                }

            }

            return randomizationNumberDto;

        }
        public void UpdateRandomizationIdForIWRS(RandomizationDto obj)
        {
            string randno = string.Empty;
            var data = new SupplyManagementUploadFileDetail();
            var numerformate = _context.RandomizationNumberSettings.Where(x => x.ProjectId == obj.ParentProjectId).FirstOrDefault();
            if (numerformate != null && numerformate.IsIGT)
            {
                var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                if (SupplyManagementUploadFile == null)
                {
                    return;
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                {
                    data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId && x.DeletedDate == null
                    && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

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
                        data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                           && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId && x.DeletedDate == null
                           && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                        if (data != null)
                        {
                            data.RandomizationId = obj.Id;
                            _context.SupplyManagementUploadFileDetail.Update(data);

                        }
                    }
                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                {
                    data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                    && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.DeletedDate == null && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

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
            var numerformate = _context.RandomizationNumberSettings.Where(x => x.ProjectId == obj.ParentProjectId).FirstOrDefault();
            if (numerformate != null && numerformate.IsIGT)
            {
                var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.DeletedDate == null && x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                if (SupplyManagementUploadFile == null)
                {
                    return false;
                }

                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
                {
                    var data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId
                   && x.DeletedDate == null && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

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
                        var data1 = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                           && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                          && x.DeletedDate == null && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                        if (data1 != null && data1.RandomizationId != null && data1.RandomizationId != obj.Id)
                        {
                            return false;
                        }
                    }

                }
                if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
                {
                    var data1 = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                    && x.DeletedDate == null && x.RandomizationNo == Convert.ToInt32(obj.RandomizationNumber) && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                    if (data1 != null && data1.RandomizationId != null && data1.RandomizationId != obj.Id)
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
            if (studydata.RandomNoLength <= 0 && !studydata.IsIGT)
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
                        randomizationNumberDto.ScreeningNumber = sitedata.PrefixScreeningNo + latestno.ToString().PadLeft((int)studydata.ScreeningLength, '0');
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

        public List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted)
        {
            var result = All.Where(x => x.ProjectId == projectId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                  ProjectTo<RandomizationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            var projectright = _projectRightRepository.FindBy(x => x.ProjectId == projectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId).FirstOrDefault();
            var rolelist = _context.SiteTeam.Where(x => x.ProjectId == projectId && x.DeletedDate == null && x.IsIcfApproval == true).Select(x => x.RoleId).ToList();
            result.ForEach(x =>
            {
                x.PatientStatusName = x.PatientStatusId == null ? "" : x.PatientStatusId.GetDescription();//_patientStatusRepository.Find((int)x.PatientStatusId).StatusName;
                                                                                                          //x.IsShowEconsentIcon = x.EconsentReviewDetails.Any(x => x.IsReviewedByPatient == true);
                                                                                                          //x.EconsentReviewDetails.Any(x => !String.IsNullOrEmpty(x.PdfPath))
                x.IsShowEconsentIcon = (rolelist.Contains(_jwtTokenAccesser.RoleId) && projectright != null);
            });

            var project = _context.Project.Find(_context.Project.Find(projectId).ParentProjectId);
            var ProjectSettings = _context.ProjectSettings.Where(x => x.ProjectId == project.Id && x.DeletedDate == null).FirstOrDefault();

            result.ForEach(x =>
            {
                x.IsEicf = ProjectSettings != null ? ProjectSettings.IsEicf : false;
                x.IsAllEconsentReviewed = _context.EconsentReviewDetails.Where(c => c.RandomizationId == x.Id).Count() > 0 ? _context.EconsentReviewDetails.Where(c => c.RandomizationId == x.Id).All(z => z.IsReviewedByPatient == true) : false;
                x.ParentProjectCode = project.ProjectCode;
                var screeningtemplate = _screeningTemplateRepository.FindByInclude(y => y.ScreeningVisit.ScreeningEntry.RandomizationId == x.Id && y.DeletedDate == null).ToList();
                x.IsLocked = screeningtemplate.Count() <= 0 || screeningtemplate.Any(y => y.IsLocked == false) ? false : true;
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

        public async Task SendEmailOfScreenedtoPatientLAR(Randomization randomization, int sendtype)
        {
            var studyId = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var studydata = _projectRepository.Find((int)studyId);
            if (studydata.IsSendSMS == true || studydata.IsSendEmail == true)
            {
                var userdata = _userRepository.Find((int)randomization.LARUserId);
                var userotp = await _centreUserService.GetUserOtpDetails($"{_environmentSetting.Value.CentralApi}UserOtp/GetuserOtpDetails/{userdata.Id}");
                await _emailSenderRespository.SendEmailOfScreenedPatient(randomization.LegalEmail, randomization.LegalFirstName + " " + randomization.LegalLastName, userdata.UserName, userotp.Otp, studydata.ProjectCode, randomization.LegalEmergencyCoNumber, sendtype, studydata.IsSendEmail, studydata.IsSendSMS);
            }
        }

        public void SendEmailOfStartEconsent(Randomization randomization)
        {
            //var projectname = _projectRepository.Find(randomization.ProjectId).ProjectCode;
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
            var studyid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            if (randomization.PatientStatusId == ScreeningPatientStatus.ConsentInProcess || randomization.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess)
            {
                if (_context.EconsentReviewDetails.Where(x => x.RandomizationId == id && x.IsReviewDoneByInvestigator == false).Count() == 0)
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
            var roleName = _jwtTokenAccesser.RoleName;
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();

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
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();

            if (_jwtTokenAccesser.RoleName == "LAR")
            {
                randomization = FindBy(x => x.LARUserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            }

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
                    //x.UserPicUrl = _context.UploadSetting.FirstOrDefault().ImageUrl + (_userRepository.Find(x.UserId).ProfilePic ?? DocumentService.DefulatProfilePic);
                    x.UserPicUrl = _context.UploadSetting.FirstOrDefault().ImageUrl + (_roleRepository.Find(x.RoleId).RoleIcon ?? _userRepository.Find(x.UserId).ProfilePic ?? DocumentService.DefulatProfilePic);
                });
                dashboardPatientDto.siteTeams = siteteamdtos;
                if (project.ManageSiteId != null)
                {
                    dashboardPatientDto.hospitalName = _manageSiteRepository.Find((int)project.ManageSiteId).SiteName;

                    dashboardPatientDto.siteAddress = _manageSiteRepository.Find((int)project.ManageSiteId).SiteAddress;
                }
                dashboardPatientDto.patientdetail = _jwtTokenAccesser.RoleName == "LAR" ? "LAR FirstName: " + randomization.LegalFirstName + ", LAR LastName: " + randomization.LegalLastName : "Screening Number: " + randomization.ScreeningNumber + " Initial: " + randomization.Initial;
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
            var data = _context.ScreeningTemplate.Where(x => x.ScreeningVisitId == screeningVisitId && x.DeletedDate == null && x.ProjectDesignTemplate.IsParticipantView == true).
                        Select(r => new ProjectDesignTemplateMobileDto
                        {
                            ScreeningTemplateId = r.Id,
                            ProjectDesignTemplateId = r.ProjectDesignTemplateId,
                            ProjectDesignVisitId = r.ScreeningVisit.ProjectDesignVisitId,
                            TemplateName = ((_jwtTokenAccesser.Language != 1) ?
                r.ProjectDesignTemplate.TemplateLanguage.Where(x => x.DeletedDate == null && x.LanguageId == (int)_jwtTokenAccesser.Language && x.DeletedDate == null).Select(a => a.Display).FirstOrDefault() : r.ProjectDesignTemplate.TemplateName),// r.ProjectDesignTemplate.TemplateName,
                            Status = r.Status,
                            DesignOrder = r.ProjectDesignTemplate.DesignOrder,
                            ScheduleDate = r.ScheduleDate,
                            IsTemplateRestricted = false,
                            IsPastTemplate = false,
                            IsHide = r.IsHide ?? false
                        }).OrderBy(r => r.DesignOrder).ToList();

            data = data.Where(x => x.IsHide == false).ToList();
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
                    var noofHH = ProjectScheduleTemplates.Min(t => t.HH);
                    var noofMM = ProjectScheduleTemplates.Min(t => t.MM);
                    var ProjectScheduleTemplate = ProjectScheduleTemplates.Where(x => x.NoOfDay == noofday).FirstOrDefault();

                    if ((noofday == null) && (noofHH != null || noofMM != null))
                    {
                        var mindate = ((DateTime)x.ScheduleDate).AddMinutes(ProjectScheduleTemplate.NegativeDeviation * -1);
                        var maxdate = ((DateTime)x.ScheduleDate).AddMinutes(ProjectScheduleTemplate.PositiveDeviation);
                        var clientDate = DateTime.Now.AddHours(4).AddMinutes(30);

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


            return null;
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
            var pro = _context.Project.Where(x => x.Id == projectId).FirstOrDefault();
            var project = new List<Data.Entities.Master.Project>();
            if (pro.ParentProjectId == null)
            {
                var projectList = _projectRightRepository.GetProjectRightIdList();
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
                data.IsParentProject = pro.ParentProjectId == null ? true : false;
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
            var pro = _context.Project.Where(x => x.Id == projectId).FirstOrDefault();

            var project = new List<Data.Entities.Master.Project>();
            if (pro.ParentProjectId == null)
            {
                var projectList = _projectRightRepository.GetProjectRightIdList();
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
            if (screeningFilter.Count() > 0)
            {
                var AveragePerMonthScreening = screeningFilter.Count() / (pro.Recruitment == null ? 1 : (int)pro.Recruitment);
                screeningCount = screeningFilter.GroupBy(x => x.ScreeningMonth)
                   .Select(g => new DashboardRecruitmentRateDto
                   {
                       ScreeningDataCount = g.Count(),
                       ScreeningAvgValue = AveragePerMonthScreening >= 1 ? (int?)Math.Round((decimal)(g.Count() * 100) / AveragePerMonthScreening, 2) : 0,
                       ScreeningMonth = DateTime.ParseExact(g.Key, "MMM yyyy", CultureInfo.CurrentCulture).Month,
                       IsScreeningAchive = g.Count() >= (screeningFilter.Count() / (pro.Recruitment == null ? 1 : (int)pro.Recruitment))
                   }).Where(x => x.ScreeningMonth == today.Month).FirstOrDefault();
            }

            var randomizationFilter = randomization.Where(y => y.RandomizationDate != null).ToList();
            if (randomizationFilter.Count() > 0)
            {
                var AveragePerMonth = randomizationFilter.Count() / (pro.Recruitment == null ? 1 : (int)pro.Recruitment);

                randomizationCount = randomizationFilter.GroupBy(x => x.RandomizationMonth)
               .Select(g => new DashboardRecruitmentRateDto
               {
                   RandomizationDataCount = g.Count(),
                   RandomizationAvgValue = AveragePerMonth >= 1 ? (int?)Math.Round((decimal)(g.Count() * 100) / AveragePerMonth, 2) : 0,
                   RandomizationMonth = DateTime.ParseExact(g.Key, "MMM yyyy", CultureInfo.CurrentCulture).Month,
                   IsRandomizationAchive = g.Count() >= (randomizationFilter.Count() / (pro.Recruitment == null ? 1 : (int)pro.Recruitment))
               }).Where(x => x.RandomizationMonth == today.Month).FirstOrDefault();
            }

            if (randomizationCount != null)
            {
                if (screeningCount == null)
                    screeningCount = new DashboardRecruitmentRateDto();
                screeningCount.RandomizationDataCount = randomizationCount.RandomizationDataCount;
                screeningCount.RandomizationAvgValue = randomizationCount?.RandomizationAvgValue;
                screeningCount.RandomizationMonth = randomizationCount?.RandomizationMonth;
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
                var projectList = _projectRightRepository.GetProjectRightIdList();
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
                data.IsParentProject = pro.ParentProjectId == null ? true : false;
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
            userLarRole.UserRoleId = _context.SecurityRole.Where(c => c.RoleShortName == "LAR").FirstOrDefault().Id;
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
            obj.RandomizationNumber = randomizationNumberDto.RandomizationNumber;
            obj.ProductCode = randomizationNumberDto.ProductCode;
            obj.VisitId = randomizationNumberDto.VisitId;
            obj.KitCount = randomizationNumberDto.KitCount;

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
                            SupplyManagementKITDetailId = kitdata.Id
                        };
                        _supplyManagementKITRepository.InsertKitRandomizationDetail(supplyManagementVisitKITDetailDto);

                        SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                        history.SupplyManagementKITDetailId = kitdata.Id;
                        history.Status = KitStatus.Allocated;
                        history.RoleId = _jwtTokenAccesser.RoleId;
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
                        }
                        var supplyManagementVisitKITDetailDto = new SupplyManagementVisitKITSequenceDetailDto
                        {
                            RandomizationId = obj.Id,
                            ProjectDesignVisitId = randomizationNumberDto.VisitId,
                            KitNo = randomizationNumberDto.KitNo,
                            ProductCode = randomizationNumberDto.ProductCode,
                            SupplyManagementKITSeriesdetailId = kitdata.Id
                        };
                        _supplyManagementKITRepository.InsertKitSequenceRandomizationDetail(supplyManagementVisitKITDetailDto);

                        SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                        history.SupplyManagementKITSeriesId = kitdata.SupplyManagementKITSeriesId;
                        history.Status = KitStatus.Allocated;
                        history.RoleId = _jwtTokenAccesser.RoleId;
                        _supplyManagementKITRepository.InsertKitSequenceHistory(history);

                        _context.Save();

                        obj.KitNo = randomizationNumberDto.KitNo;
                    }
                }
            }
            var randomization = All.Where(x => x.Id == obj.Id).FirstOrDefault();
            if (randomization != null && randomizationNumberDto.IsIGT)
            {
                randomization.ProductCode = randomizationNumberDto.ProductCode;
                _context.Randomization.Update(randomization);
                _context.Save();
            }
            if (!string.IsNullOrEmpty(randomizationNumberDto.RandomizationNumber) && randomizationNumberDto.IsIGT)
                UpdateRandomizationIdForIWRS(obj);


            return obj;
        }
        public void UpdateRandmizationKitNotAssigned(RandomizationDto obj)
        {
            SupplyManagementUploadFileDetail data = new SupplyManagementUploadFileDetail();

            var randomization = All.Where(x => x.Id == obj.Id).FirstOrDefault();
            randomization.DateOfRandomization = null;
            randomization.RandomizationNumber = null;
            randomization.ProductCode = null;
            _context.Randomization.Update(randomization);
            _context.Save();

            var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
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

            var data1 = _context.SupplyManagementUploadFileDetail.Where(x => x.Id == data.Id && x.RandomizationId == obj.Id).FirstOrDefault();
            if (data1 != null)
            {
                data1.RandomizationId = null;
                _context.SupplyManagementUploadFileDetail.Update(data1);
                _context.Save();
            }
        }
        public bool CheckKitNumber(RandomizationDto obj, bool Isiwrs)
        {
            if (!Isiwrs)
            {
                return true;
            }
            SupplyManagementUploadFileDetail data = new SupplyManagementUploadFileDetail();
            var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.DeletedDate == null && x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
            if (SupplyManagementUploadFile == null)
                return false;
            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
            {
                data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId
               && x.DeletedDate == null && x.RandomizationId == obj.Id && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                if (data == null)
                    return false;
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

                    if (data == null)
                        return false;
                }
            }

            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
            {
                data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                && x.DeletedDate == null && x.RandomizationId == obj.Id && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                if (data == null)
                    return false;
            }

            var visit = _context.SupplyManagementUploadFileVisit.Where(x => x.DeletedDate == null && x.SupplyManagementUploadFileDetailId == data.Id && x.Isfirstvisit == true).FirstOrDefault();
            if (visit == null)
                return false;

            var kitdata = _context.SupplyManagementKITDetail.Where(x => x.DeletedDate == null
                          && x.SupplyManagementKIT.ProjectDesignVisitId == visit.ProjectDesignVisitId
                          && x.SupplyManagementKIT.PharmacyStudyProductType.ProjectId == obj.ParentProjectId
                          && x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode == visit.Value
                          && x.SupplyManagementShipmentId != null
                          && x.SupplyManagementKIT.DeletedDate == null
                          && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ProjectId
                          && (x.Status == KitStatus.Allocated)
                          && x.RandomizationId == obj.Id).OrderBy(x => x.Id).FirstOrDefault();
            if (kitdata != null)
                return false;

            return true;
        }
        public void SendRandomizationIWRSEMail(RandomizationDto obj)
        {
            SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
            IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
            var study = _context.Project.Where(x => x.Id == obj.ParentProjectId).FirstOrDefault();
            if (study != null)
            {
                var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == obj.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.SubjectRandomization).ToList();
                if (emailconfiglist != null && emailconfiglist.Count > 0)
                {
                    var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                    if (siteconfig.Count > 0)
                    {
                        emailconfig = siteconfig.Where(x => x.SiteId == obj.ProjectId).FirstOrDefault();
                    }
                    else
                    {
                        emailconfig = emailconfiglist.FirstOrDefault();
                    }
                    var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                    if (details.Count() > 0)
                    {
                        iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == obj.ParentProjectId).FirstOrDefault().ProjectCode;

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
                IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
                var study = _context.Project.Where(x => x.Id == obj.ParentProjectId).FirstOrDefault();
                if (study != null)
                {
                    var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == obj.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.Threshold).ToList();
                    if (emailconfiglist != null && emailconfiglist.Count > 0)
                    {
                        var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                        if (siteconfig.Count > 0)
                        {
                            emailconfig = siteconfig.Where(x => x.SiteId == obj.ProjectId).FirstOrDefault();
                        }
                        else
                        {
                            emailconfig = emailconfiglist.FirstOrDefault();
                        }
                        var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                        if (details.Count() > 0)
                        {
                            iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == obj.ParentProjectId).FirstOrDefault().ProjectCode;

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
                                _context.Save();
                            }
                        }
                    }
                }
            }

        }
    }
}