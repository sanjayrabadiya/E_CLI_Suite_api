using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
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
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository)
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
            //result.ForEach(x => x.PatientStatusName = x.PatientStatusId.GetDescription());
            result.ForEach(x => {
                x.PatientStatusName = x.PatientStatusId == null ? "" : _patientStatusRepository.Find((int)x.PatientStatusId).StatusName;
                x.IsShowEconsentIcon = _context.EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true).ToList().Count > 0 ? true : false;
                if (x.IsShowEconsentIcon == true)
                {
                    var EconsentReviewDetails = _context.EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true).ToList();
                    x.EconsentReviewDetails = _mapper.Map<List<EconsentReviewDetailsDto>>(EconsentReviewDetails);
                    x.EconsentReviewDetails.ForEach(a => {
                        a.EconsentDocumentName = Context.EconsentSetup.Where(t => t.Id == a.EconsentDocumentId).ToList().FirstOrDefault().DocumentName;
                    });
                    x.IsEconsentReviewPending = _context.EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true && t.IsApprovedByInvestigator == false).ToList().Count > 0 ? true : false;
                    x.IsmultipleEconsentReviewDetails = _context.EconsentReviewDetails.Where(t => t.AttendanceId == x.Id && t.IsReviewedByPatient == true).ToList().Count > 1 ? true : false;
                }
                else
                {
                    x.IsEconsentReviewPending = false;
                }
            });
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

        public void ChangeStatustoConsentInProgress(int id)
        {

            var randomization = Find(id);
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
                    econsentReviewDetails.AttendanceId = id;
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
            if (randomization.PatientStatusId == ScreeningPatientStatus.ConsentInProcess)
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


    }
}