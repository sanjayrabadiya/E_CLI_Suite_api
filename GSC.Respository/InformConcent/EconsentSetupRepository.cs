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
        private readonly IProjectRepository _projectRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IUnitOfWork _uow;
        private readonly IEconsentReviewDetailsAuditRepository _econsentReviewDetailsAuditRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public EconsentSetupRepository(IGSCContext context,
            IProjectRepository projectRepository,
            IMapper mapper,
            IEmailSenderRespository emailSenderRespository,
            IRandomizationRepository randomizationRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IUnitOfWork uow,
            IUploadSettingRepository uploadSettingRepository,
            IEconsentReviewDetailsAuditRepository econsentReviewDetailsAuditRepository) : base(context)
        {
            _projectRepository = projectRepository;
            _mapper = mapper;
            _context = context;
            _emailSenderRespository = emailSenderRespository;
            _randomizationRepository = randomizationRepository;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _uow = uow;
            _econsentReviewDetailsAuditRepository = econsentReviewDetailsAuditRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        public string Duplicate(EconsentSetup objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.Version == objSave.Version && x.ProjectId == objSave.ProjectId && x.LanguageId == objSave.LanguageId && x.DeletedDate == null && x.DocumentName == objSave.DocumentName))
            {
                return "Duplicate Document";
            }
            return "";
        }

        public List<DropDownDto> GetEconsentDocumentDropDown(int projectId)
        {
            return All.Where(x =>
                   x.ProjectId == projectId && x.DeletedDate == null && x.DocumentStatusId == DocumentStatus.Final)
               .Select(c => new DropDownDto { Id = c.Id, Value = c.DocumentName, IsDeleted = false }).OrderBy(o => o.Value)
               .ToList();
        }

        public List<EconsentSetupGridDto> GetEconsentSetupList(int projectid, bool isDeleted)
        {
            var econsentSetups = All.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == projectid).
                   ProjectTo<EconsentSetupGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            econsentSetups.ForEach(t =>
            {
                if (t.IntroVideoPath != null)
                {
                    t.IntroVideoPath = System.IO.Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), t.IntroVideoPath).Replace('\\', '/');
                }
            });

            return econsentSetups;
        }

        public List<DropDownDto> GetPatientStatusDropDown()
        {
            var result = Enum.GetValues(typeof(ScreeningPatientStatus))
                  .Cast<ScreeningPatientStatus>().Where(x =>
                                                                     //x == ScreeningPatientStatus.PreScreening ||
                                                                     x == ScreeningPatientStatus.Screening ||
                                                                     x == ScreeningPatientStatus.ConsentCompleted ||
                                                                     x == ScreeningPatientStatus.OnTrial).Select(e => new DropDownDto
                                                                     {
                                                                         Id = Convert.ToInt16(e),
                                                                         Value = e.GetDescription()
                                                                     }).OrderBy(o => o.Value).ToList();
            return result;
        }

        public string validateDocument(string path)
        {
            bool isheaderpresent = false;
            bool isheaderblank = false;
            Stream stream = System.IO.File.OpenRead(path);
            string sfdtText = "";
            EJ2WordDocument wdocument = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(wdocument);

            wdocument.OptimizeSfdt = false;
            string json = Newtonsoft.Json.JsonConvert.SerializeObject(wdocument);
            wdocument.Dispose();


            //wdocument.Dispose();
            //string json = sfdtText;
            stream.Position = 0;
            stream.Close();
            JObject jsonstr = JObject.Parse(json);
            Root jsonobj = JsonConvert.DeserializeObject<Root>(jsonstr.ToString());
            foreach (var e1 in jsonobj.sections)
            {
                foreach (var e2 in e1.blocks)
                {
                    StringBuilder headerstring = new StringBuilder();
                    if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                    {
                        foreach (var e3 in e2.inlines.Select(s => s.text))
                        {
                            if (!string.IsNullOrEmpty(e3))
                            {
                                headerstring.Append(e3);
                            }
                        }
                        isheaderpresent = true;
                        if (headerstring.ToString() == "")
                        {
                            isheaderblank = true;
                            break;
                        }
                    }
                }
            }
            if (!isheaderpresent)
            {
                return "Please apply 'Heading 1' style in all headings in the document for proper sections.";
            }
            if (isheaderblank)
            {
                return "Please check all occurances of 'Heading 1' format in the document, content in 'Heading 1' format must be not empty";
            }
            return "";
        }

        public void SendDocumentEmailPatient(EconsentSetup econsent)
        {
            var result = _context.Randomization.Where(x => x.Project.ParentProjectId == econsent.ProjectId && x.DeletedDate == null && x.LanguageId == econsent.LanguageId
            && (x.PatientStatusId != ScreeningPatientStatus.Completed) && (x.PatientStatusId != ScreeningPatientStatus.Withdrawal)
            ).Include(x => x.Project).ToList();

            string projectcode = _projectRepository.Find(econsent.ProjectId).ProjectCode;
            foreach (var item in result)
            {
                EconsentReviewDetails econsentReviewDetails = new EconsentReviewDetails();
                econsentReviewDetails.RandomizationId = item.Id;
                econsentReviewDetails.EconsentSetupId = econsent.Id;
                econsentReviewDetails.IsReviewedByPatient = false;
                _econsentReviewDetailsRepository.Add(econsentReviewDetails);
                if (item.PatientStatusId == ScreeningPatientStatus.ConsentCompleted || item.PatientStatusId == ScreeningPatientStatus.OnTrial)
                {
                    _randomizationRepository.ChangeStatustoReConsentInProgress(item.Id);
                }
                EconsentReviewDetailsAudit audit = new EconsentReviewDetailsAudit();
                audit.EconsentReviewDetailsId = econsentReviewDetails.Id;
                audit.Activity = ICFAction.Screened;
                audit.PateientStatus = item.PatientStatusId;
                _econsentReviewDetailsAuditRepository.Add(audit);
                if (item.Email != "")
                {
                    string patientName = "";
                    if (item.ScreeningNumber != "")
                    {
                        patientName = item.Initial + " " + item.ScreeningNumber;
                    }
                    else
                    {
                        patientName = item.FirstName + " " + item.MiddleName + " " + item.LastName;
                    }
                    _emailSenderRespository.SendEmailOfEconsentDocumentuploaded(item.Email, patientName, econsent.DocumentName, projectcode);
                }
            }
            _uow.Save();
        }

    }
}
