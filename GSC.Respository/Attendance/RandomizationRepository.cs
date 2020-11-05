﻿using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.ProjectRight;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Attendance
{
    public class RandomizationRepository : GenericRespository<Randomization, GscContext>, IRandomizationRepository
    {

        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IStateRepository _stateRepository;
        private readonly ICountryRepository _countryRepository;
        private readonly ICityRepository _cityRepository;
        private readonly IMapper _mapper;
        private readonly IPatientStatusRepository _patientStatusRepository;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly GscContext _context;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRightRepository _projectRightRepository;
        public RandomizationRepository(IUnitOfWork<GscContext> uow,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectDesignRepository projectDesignRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IStateRepository stateRepository,
            ICountryRepository countryRepository,
            ICityRepository cityRepository,
             IMapper mapper,
             IPatientStatusRepository patientStatusRepository,
             IEmailSenderRespository emailSenderRespository,
            IProjectRepository projectRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IProjectRightRepository projectRightRepository)
            : base(uow, jwtTokenAccesser)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectDesignRepository = projectDesignRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _stateRepository = stateRepository;
            _countryRepository = countryRepository;
            _cityRepository = cityRepository;
            _mapper = mapper;
            _patientStatusRepository = patientStatusRepository;
            _context = uow.Context;
            _emailSenderRespository = emailSenderRespository;
            _projectRepository = projectRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRightRepository = projectRightRepository;
        }

        public void SaveRandomization(Randomization randomization, RandomizationDto randomizationDto)
        {
            randomization.ScreeningNumber = randomizationDto.ScreeningNumber;
            randomization.DateOfScreening = randomizationDto.DateOfScreening;
            randomization.DateOfRandomization = randomizationDto.DateOfRandomization;
            randomization.RandomizationNumber = randomizationDto.RandomizationNumber;
            randomization.PatientStatusId = ScreeningPatientStatus.Screening;
            Update(randomization);
        }

        public List<RandomizationGridDto> GetRandomizationList(int projectId, bool isDeleted)
        {
            var result = All.Where(x => x.ProjectId == projectId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).
                  ProjectTo<RandomizationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            var projectright = _projectRightRepository.FindBy(x => x.ProjectId == projectId && x.UserId == _jwtTokenAccesser.UserId && x.RoleId == _jwtTokenAccesser.RoleId).ToList();
            result.ForEach(x => {
                x.PatientStatusName = x.PatientStatusId == null ? "" : _patientStatusRepository.Find((int)x.PatientStatusId).StatusName;
                if (projectright.Count > 0)
                {
                    var EconsentReviewDetails = (from econsentreviewdetails in _context.EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true).ToList()
                                                 join econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == projectId && x.DeletedDate == null) on econsentreviewdetails.EconsentDocumentId equals econsentsetups.Id
                                                 join roles in _context.EconsentSetupRoles.Where(a => a.RoleId == _jwtTokenAccesser.RoleId && a.DeletedDate == null) on econsentsetups.Id equals roles.EconsentDocumentId
                                                 select new EconsentReviewDetailsDto
                                                 {
                                                     Id = econsentreviewdetails.Id,
                                                     EconsentDocumentName = econsentsetups.DocumentName,
                                                     IsApprovedByInvestigator = econsentreviewdetails.IsApprovedByInvestigator,
                                                     IsReviewedByPatient = econsentreviewdetails.IsReviewedByPatient,
                                                     AttendanceId = x.Id,
                                                     EconsentDocumentId = econsentsetups.Id,
                                                     patientdigitalSignImagepath = econsentreviewdetails.patientdigitalSignImagepath,
                                                     pdfpath = econsentreviewdetails.pdfpath,
                                                     ApprovedByRoleId = econsentreviewdetails.ApprovedByRoleId
                                                 }
                                            ).ToList();
                    if (EconsentReviewDetails.Count > 0)
                    {
                        x.IsShowEconsentIcon = true;
                        x.EconsentReviewDetails = EconsentReviewDetails;
                        x.IsEconsentReviewPending = EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true && t.IsApprovedByInvestigator == false).ToList().Count > 0 ? true : false;
                        x.IsmultipleEconsentReviewDetails = EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true).ToList().Count > 1 ? true : false;
                    }
                    else
                    {
                        x.IsShowEconsentIcon = false;
                    }
                }
                else
                {
                    x.IsShowEconsentIcon = false;
                }
            });

            var projectCode = Context.Project.Find(Context.Project.Find(projectId).ParentProjectId).ProjectCode;
            result.ForEach(x => { x.ParentProjectCode = projectCode; });

            return result;
        }

        public string Duplicate(Randomization objSave, int projectId)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.RandomizationNumber == objSave.RandomizationNumber &&
                x.ProjectId == projectId && !string.IsNullOrEmpty(x.RandomizationNumber) &&
                x.DeletedDate == null)) return "Duplicate Randomization Number : " + objSave.RandomizationNumber;

            return "";
        }

        public void SendEmailOfStartEconsent(Randomization randomization)
        {
            var projectname = _projectRepository.Find(randomization.ProjectId).ProjectCode;
            var Econsentdocuments = (from econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == randomization.ProjectId && x.LanguageId == randomization.LanguageId && x.DeletedDate == null)
                                     join status in _context.EconsentSetupPatientStatus.Where(a => a.PatientStatusId == (int)randomization.PatientStatusId && a.DeletedDate == null) on econsentsetups.Id equals status.EconsentDocumentId
                                     select new EconsentSetup
                                     {
                                         Id = econsentsetups.Id,
                                         DocumentName = econsentsetups.DocumentName
                                     }).ToList();
            string documentname = "";
            for (var i = 0; i < Econsentdocuments.Count; i++)
            {
                documentname = documentname + ((i == 0) ? "" : " , ")  + Econsentdocuments[i].DocumentName;
            }
            if (Econsentdocuments.Count > 0)
            {
                _emailSenderRespository.SendEmailOfStartEconsent(randomization.Email, randomization.ScreeningNumber + " " + randomization.Initial, documentname, projectname);
            }
        }


        public void ChangeStatustoConsentInProgress()
        {

            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization.PatientStatusId == ScreeningPatientStatus.PreScreening || randomization.PatientStatusId == ScreeningPatientStatus.Screening)
            {
                var Econsentdocuments = (from econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == randomization.ProjectId && x.LanguageId == randomization.LanguageId && x.DeletedDate == null)
                                         join status in _context.EconsentSetupPatientStatus.Where(a => a.PatientStatusId == (int)randomization.PatientStatusId && a.DeletedDate == null) on econsentsetups.Id equals status.EconsentDocumentId
                                         select new EconsentSetup
                                         {
                                             Id = econsentsetups.Id,
                                             DocumentName = econsentsetups.DocumentName
                                         }).ToList();
                for (var i = 0; i < Econsentdocuments.Count; i++)
                {
                    EconsentReviewDetails econsentReviewDetails = new EconsentReviewDetails();
                    econsentReviewDetails.AttendanceId = randomization.Id;
                    econsentReviewDetails.EconsentDocumentId = Econsentdocuments[i].Id;
                    econsentReviewDetails.IsReviewedByPatient = false;
                    _econsentReviewDetailsRepository.Add(econsentReviewDetails);
                }

                randomization.PatientStatusId = ScreeningPatientStatus.ConsentInProcess;
                Update(randomization);
            }
        }

        public void ChangeStatustoConsentCompleted(int id)
        {
            var randomization = Find(id);
            if (randomization.PatientStatusId == ScreeningPatientStatus.ConsentInProcess || randomization.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess)
            {
                if (_econsentReviewDetailsRepository.FindByInclude(x => x.AttendanceId == id && x.IsApprovedByInvestigator == false).ToList().Count > 0)
                {
                }
                else
                {
                    var Econsentdocuments = (from econsentsetups in _context.EconsentSetup.Where(x => x.DeletedDate == null && x.LanguageId == randomization.LanguageId && x.ProjectId == randomization.ProjectId).ToList()
                                             join doc in _econsentReviewDetailsRepository.FindByInclude(x => x.AttendanceId == id).ToList() on econsentsetups.Id equals doc.EconsentDocumentId into ps
                                             from p in ps.DefaultIfEmpty()
                                             select new EconsentSetup
                                             {
                                                 Id = (p == null) ? 0 : econsentsetups.Id
                                             }).ToList();


                    if (Econsentdocuments.Where(x => x.Id == 0).ToList().Count > 0)
                    {
                    }
                    else
                    {
                        randomization.PatientStatusId = ScreeningPatientStatus.ConsentCompleted;
                        Update(randomization);
                        _uow.Save();
                    }
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
                _uow.Save();
            }
        }


        public void PatientStatus(ScreeningPatientStatus patientStatus, int screeningEntryId)
        {
            var randomization = All.Where(x => x.ScreeningEntry.Id == screeningEntryId).FirstOrDefault();
            randomization.PatientStatusId = patientStatus;
            Update(randomization);
        }

        public void ChangeStatustoWithdrawal()
        {
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization.PatientStatusId == ScreeningPatientStatus.ConsentCompleted)
            {
                randomization.PatientStatusId = ScreeningPatientStatus.Withdrawal;
                Update(randomization);
            }
        }

        public DashboardPatientDto GetDashboardPatientDetail()
        {
            var randomization = FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault();
            if (randomization != null)
            {
                var project = Context.Project.Where(x => x.Id == randomization.ProjectId).ToList().FirstOrDefault();
                var parentproject = Context.Project.Where(x => x.Id == project.ParentProjectId).ToList().FirstOrDefault();
                var investigator = Context.InvestigatorContact.Where(x => x.Id == project.InvestigatorContactId).ToList().FirstOrDefault();
                DashboardPatientDto dashboardPatientDto = new DashboardPatientDto();
                dashboardPatientDto.projectId = project.Id;
                dashboardPatientDto.studycode = parentproject.ProjectCode;
                dashboardPatientDto.sitecode = project.ProjectCode;
                dashboardPatientDto.patientStatusId = (int)randomization.PatientStatusId;
                dashboardPatientDto.patientStatus = randomization.PatientStatusId.GetDescription();
                dashboardPatientDto.investigatorName = investigator.NameOfInvestigator;
                dashboardPatientDto.investigatorcontact = investigator.ContactNumber;
                return dashboardPatientDto;
            } else
            {
                return new DashboardPatientDto();
            }
            
        }
    }
}