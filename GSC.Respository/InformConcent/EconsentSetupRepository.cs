using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;

namespace GSC.Respository.InformConcent
{
    public class EconsentSetupRepository : GenericRespository<EconsentSetup>, IEconsentSetupRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly ILanguageRepository _languageRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEconsentSetupPatientStatusRepository _econsentSetupPatientStatusRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IEconsentSetupRolesRepository _econsentSetupRolesRepository;
        private readonly IUnitOfWork _uow;
        public EconsentSetupRepository(IGSCContext context, 
            ILanguageRepository languageRepository,
            IProjectRepository projectRepository,
            IMapper mapper,
            IUploadSettingRepository uploadSettingRepository,
            IEmailSenderRespository emailSenderRespository,
            IEconsentSetupPatientStatusRepository econsentSetupPatientStatusRepository,
            IRandomizationRepository randomizationRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IEconsentSetupRolesRepository econsentSetupRolesRepository,
            IUnitOfWork uow) : base(context)
        {
            _languageRepository = languageRepository;
            _projectRepository = projectRepository;
            _mapper = mapper;
            _context = context;
            _uploadSettingRepository = uploadSettingRepository;
            _econsentSetupPatientStatusRepository = econsentSetupPatientStatusRepository;
            _emailSenderRespository = emailSenderRespository;
            _randomizationRepository = randomizationRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _econsentSetupRolesRepository = econsentSetupRolesRepository;
            _uow = uow;
        }

        public string validatebeforeadd(EconsentSetupDto econsentSetupDto)
        {
            Data.Dto.InformConcent.SaveFileDto obj = new Data.Dto.InformConcent.SaveFileDto();
            obj.Path = _uploadSettingRepository.GetDocumentPath();
            obj.FolderType = FolderType.InformConcent;
            obj.RootName = "EconsentSetup";
            obj.FileModel = econsentSetupDto.FileModel;

            if (All.Where(x => x.DocumentName == econsentSetupDto.DocumentName && x.LanguageId == econsentSetupDto.LanguageId && x.ProjectId == econsentSetupDto.ProjectId && x.DeletedDate == null).ToList().Count > 0)
            {
                return "Please add different document name";
            }

            if (econsentSetupDto.FileModel?.Base64?.Length > 0)
            {
                econsentSetupDto.DocumentPath = DocumentService.SaveEconsentFile(obj.FileModel, obj.Path, obj.FolderType, obj.RootName);
                string fullpath = Path.Combine(obj.Path, econsentSetupDto.DocumentPath);
                string isvaliddoc = validateDocument(fullpath);
                if (isvaliddoc != "")
                {
                    if (Directory.Exists(fullpath))
                    {
                        Directory.Delete(fullpath, true);
                    }
                    return isvaliddoc;
                }
            }
            return "";
        }

        public int AddEconsentSetup(EconsentSetupDto econsentSetupDto)
        {
            Data.Dto.InformConcent.SaveFileDto obj = new Data.Dto.InformConcent.SaveFileDto();
            obj.Path = _uploadSettingRepository.GetDocumentPath();
            obj.FolderType = FolderType.InformConcent;
            obj.RootName = "EconsentSetup";
            obj.FileModel = econsentSetupDto.FileModel;

            econsentSetupDto.Id = 0;
            var econsent = _mapper.Map<EconsentSetup>(econsentSetupDto);

            Add(econsent);
            for (int i = 0; i <= econsent.PatientStatus.Count - 1; i++)
            {
                _econsentSetupPatientStatusRepository.Add(econsent.PatientStatus[i]);
            }

            for (int i = 0; i <= econsent.Roles.Count - 1; i++)
            {
                _econsentSetupRolesRepository.Add(econsent.Roles[i]);
            }
            string root = Path.Combine(obj.Path, obj.FolderType.ToString(), obj.RootName);
            if (_uow.Save() <= 0)
            {
                if (Directory.Exists(root))
                {
                    Directory.Delete(root, true);
                }
                throw new Exception($"Creating EConsent File failed on save.");
            }
            var patientstatus = _context.EconsentSetupPatientStatus.Where(x => x.EconsentDocumentId == econsent.Id).ToList();
            EconsentSetupPatientStatus econsentSetupPatientStatus = new EconsentSetupPatientStatus();
            econsentSetupPatientStatus.Id = 0;
            econsentSetupPatientStatus.EconsentDocumentId = econsent.Id;
            econsentSetupPatientStatus.PatientStatusId = (int)ScreeningPatientStatus.ConsentInProcess;
            EconsentSetupPatientStatus econsentSetupPatientStatus1 = new EconsentSetupPatientStatus();
            econsentSetupPatientStatus1.Id = 0;
            econsentSetupPatientStatus1.EconsentDocumentId = econsent.Id;
            econsentSetupPatientStatus1.PatientStatusId = (int)ScreeningPatientStatus.ReConsentInProcess;
            patientstatus.Add(econsentSetupPatientStatus);
            patientstatus.Add(econsentSetupPatientStatus1);

            var result = (from patients in _context.Randomization.Include(x => x.Project).Where(x => x.Project.ParentProjectId == econsent.ProjectId && x.LanguageId == econsent.LanguageId).ToList()
                          join status in patientstatus on (int)patients.PatientStatusId equals status.PatientStatusId
                          select new Randomization
                          {
                              Id = patients.Id,
                              FirstName = patients.FirstName,
                              MiddleName = patients.MiddleName,
                              LastName = patients.LastName,
                              Initial = patients.Initial,
                              ScreeningNumber = patients.ScreeningNumber,
                              Email = patients.Email,
                              PatientStatusId = patients.PatientStatusId
                          }).ToList();
            string projectcode = _projectRepository.Find(econsent.ProjectId).ProjectCode;
            for (var i = 0; i <= result.Count - 1; i++)
            {
                if (result[i].PatientStatusId == ScreeningPatientStatus.ConsentInProcess || result[i].PatientStatusId == ScreeningPatientStatus.ReConsentInProcess || result[i].PatientStatusId == ScreeningPatientStatus.ConsentCompleted)
                {
                    EconsentReviewDetails econsentReviewDetails = new EconsentReviewDetails();
                    econsentReviewDetails.RandomizationId = result[i].Id;
                    econsentReviewDetails.EconsentSetupId = econsent.Id;
                    econsentReviewDetails.IsReviewedByPatient = false;
                    _econsentReviewDetailsRepository.Add(econsentReviewDetails);
                    if (result[i].PatientStatusId == ScreeningPatientStatus.ConsentCompleted)
                    {
                        _randomizationRepository.ChangeStatustoReConsentInProgress(result[i].Id);
                    }
                }
                if (result[i].Email != "")
                {
                    string patientName = "";
                    if (result[i].ScreeningNumber != "")
                    {
                        patientName = result[i].Initial + " " + result[i].ScreeningNumber;
                    }
                    else
                    {
                        patientName = result[i].FirstName + " " + result[i].MiddleName + " " + result[i].LastName;
                    }
                    _emailSenderRespository.SendEmailOfEconsentDocumentuploaded(result[i].Email, patientName, econsent.DocumentName, projectcode);
                }
            }
            _uow.Save();
            return econsent.Id;
        }

        public string Duplicate(EconsentSetup objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Version == objSave.Version && x.ProjectId == objSave.ProjectId && x.LanguageId == objSave.LanguageId && x.DeletedDate == null))
            {
                return "Duplicate Document";
            }
            return "";
        }

        public EconsentSetupDto GetEconsent(int id)
        {
            var econsentSetup = FindByInclude(x => x.Id == id, x => x.PatientStatus, x => x.Roles).FirstOrDefault();
            var econsentSetupDto = _mapper.Map<EconsentSetupDto>(econsentSetup);
            if (econsentSetupDto != null && econsentSetupDto.PatientStatus != null)
                econsentSetupDto.PatientStatus = econsentSetupDto.PatientStatus.Where(x => x.DeletedDate == null).ToList();
            if (econsentSetupDto != null && econsentSetupDto.Roles != null)
                econsentSetupDto.Roles = econsentSetupDto.Roles.Where(x => x.DeletedDate == null).ToList();
            return econsentSetupDto;
        }

        public List<DropDownDto> GetEconsentDocumentDropDown(int projectId)
        {
            return All.Where(x =>
                   x.ProjectId == projectId && x.DeletedDate == null)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.DocumentName, IsDeleted = false }).OrderBy(o => o.Value)
               .ToList();
        }

        public List<EconsentSetupGridDto> GetEconsentSetupList(int projectid, bool isDeleted)
        {
            var econsentSetups = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == projectid). //intList.Contains(x.ProjectId
                   ProjectTo<EconsentSetupGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            foreach (var item in econsentSetups)
            {
                item.LanguageName = _languageRepository.Find(item.LanguageId).LanguageName;
                item.ProjectName = _projectRepository.Find(item.ProjectId).ProjectCode;
                item.IsDeleted = isDeleted;
            }
            return econsentSetups;
        }

        public List<DropDownDto> GetPatientStatusDropDown()
        {
           var result = Enum.GetValues(typeof(ScreeningPatientStatus))
                 .Cast<ScreeningPatientStatus>().Where(x => x == ScreeningPatientStatus.PreScreening ||
                                                                    x == ScreeningPatientStatus.Screening ||
                                                                    x == ScreeningPatientStatus.ConsentCompleted ||
                                                                    x == ScreeningPatientStatus.OnTrial).Select(e => new DropDownDto
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).OrderBy(o => o.Value).ToList();
            return result;
        }

        public int UpdateEconsentSetup(EconsentSetupDto econsentSetupDto)
        {
            var document = Find(econsentSetupDto.Id);
            document.ProjectId = econsentSetupDto.ProjectId;
            document.DocumentName = econsentSetupDto.DocumentName;
            document.LanguageId = econsentSetupDto.LanguageId;
            document.Version = econsentSetupDto.Version;
            document.PatientStatus = econsentSetupDto.PatientStatus;
            document.Roles = econsentSetupDto.Roles;
            document.OriginalFileName = econsentSetupDto.OriginalFileName;

            if (econsentSetupDto.FileModel?.Base64?.Length > 0)
            {
                document.DocumentPath = DocumentService.SaveEconsentFile(econsentSetupDto.FileModel, _uploadSettingRepository.GetDocumentPath(), FolderType.InformConcent, "EconsentSetup");

            }

            var patientstatusDelete = _econsentSetupPatientStatusRepository.FindBy(x => x.EconsentDocumentId == document.Id).ToList();
            foreach (var item in patientstatusDelete)
            {
                _econsentSetupPatientStatusRepository.Remove(item);
            }

            var RolesDelete = _econsentSetupRolesRepository.FindBy(x => x.EconsentDocumentId == document.Id).ToList();
            foreach (var item in RolesDelete)
            {
                _econsentSetupRolesRepository.Remove(item);
            }
            for (int i = 0; i <= document.PatientStatus.Count - 1; i++)
            {
                _econsentSetupPatientStatusRepository.Add(document.PatientStatus[i]);
            }

            for (int i = 0; i <= document.Roles.Count - 1; i++)
            {
                _econsentSetupRolesRepository.Add(document.Roles[i]);
            }
            Update(document);

            if (_uow.Save() <= 0)
            {
                throw new Exception($"Updating Econsent file failed on save.");
            }
            return document.Id;
        }

        string validateDocument(string path)
        {
            bool isheaderpresent = false;
            bool isheaderblank = false;
            Stream stream = System.IO.File.OpenRead(path);
            string sfdtText = "";
            EJ2WordDocument wdocument = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(wdocument);
            wdocument.Dispose();
            string json = sfdtText;
            stream.Position = 0;
            stream.Close();
            JObject jsonstr = JObject.Parse(json);
            Root jsonobj = JsonConvert.DeserializeObject<Root>(jsonstr.ToString());
            foreach (var e1 in jsonobj.sections)
            {
                foreach (var e2 in e1.blocks)
                {
                    string headerstring = "";
                    if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                    {
                        foreach (var e3 in e2.inlines)
                        {
                            if (e3.text != null)
                            {
                                headerstring = headerstring + e3.text;
                            }
                        }
                        isheaderpresent = true;
                        if (headerstring == "")
                        {
                            isheaderblank = true;
                            break;
                        }
                    }
                }
            }
            if (isheaderpresent == false)
            {
                return "Please apply 'Heading 1' style in all headings in the document for proper sections.";
            }
            if (isheaderblank == true)
            {
                return "Please check all occurances of 'Heading 1' format in the document, content in 'Heading 1' format must be not empty";
            }
            return "";
        }


    }
}
