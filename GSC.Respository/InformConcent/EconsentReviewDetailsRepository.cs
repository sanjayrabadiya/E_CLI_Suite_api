using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Attendance;
using GSC.Respository.Master;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using GSC.Data.Dto.Etmf;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using Syncfusion.Pdf.Graphics;
using Syncfusion.Drawing;
using GSC.Shared.Extension;
using GSC.Data.Dto.Configuration;
using Syncfusion.Pdf.Parsing;
using Microsoft.AspNetCore.Mvc;
using GSC.Data.Entities.Attendance;

namespace GSC.Respository.InformConcent
{
    public class EconsentReviewDetailsRepository : GenericRespository<EconsentReviewDetails>, IEconsentReviewDetailsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IUnitOfWork _uow;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IAppSettingRepository _appSettingRepository;
        private readonly IEconsentReviewDetailsAuditRepository _econsentReviewDetailsAuditRepository;

        public EconsentReviewDetailsRepository(IGSCContext context,
                                                IJwtTokenAccesser jwtTokenAccesser,
                                                IProjectRepository projectRepository,
                                                IUserRepository userRepository,
                                                IMapper mapper,
                                                IUploadSettingRepository uploadSettingRepository,
                                                IEmailSenderRespository emailSenderRespository,
                                                IUnitOfWork uow,
                                                IRandomizationRepository randomizationRepository, IAppSettingRepository appSettingRepository,
                                                IEconsentReviewDetailsAuditRepository econsentReviewDetailsAuditRepository) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _emailSenderRespository = emailSenderRespository;
            _uow = uow;
            _randomizationRepository = randomizationRepository;
            _appSettingRepository = appSettingRepository;
            _econsentReviewDetailsAuditRepository = econsentReviewDetailsAuditRepository;
        }

        public List<EConsentDocumentHeader> GetEconsentDocumentHeaders()
        {
            // this method calls when patient login and click on menu inform consent, documents with headers displays on left side returned in this API
            var roleName = _jwtTokenAccesser.RoleName;

            var noneregister = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();

            if (roleName == "LAR")
            {
                noneregister = _context.Randomization.Where(x => x.LARUserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            }

            if (noneregister == null) return new List<EConsentDocumentHeader>();
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var eConsentResult = _context.EconsentReviewDetails.Where(x => x.RandomizationId == noneregister.Id && x.EconsentSetup.DeletedDate == null
                && x.EconsentSetup.LanguageId == noneregister.LanguageId
                && (roleName == "LAR" ? x.IsLAR == true : x.IsLAR == null || x.IsLAR == false)).Include(x => x.EconsentSetup).Include(x => x.EconsentReviewDetailsSections).ToList();

            if (eConsentResult.Count <= 0)
                return null;

            var lastRecords = eConsentResult.Where(x => x.EconsentSetup.CreatedDate.GetValueOrDefault().Ticks < noneregister.CreatedDate.GetValueOrDefault().Ticks)
                .OrderByDescending(o => o.EconsentSetupId).FirstOrDefault();
            var afterRecords = eConsentResult.Where(x => x.EconsentSetup.CreatedDate.GetValueOrDefault().Ticks > noneregister.CreatedDate.GetValueOrDefault().Ticks).ToList();
            afterRecords.Add(lastRecords);
            var result = afterRecords.Select(x => new EConsentDocumentHeader
            {
                DocumentId = x.EconsentSetup.Id,
                DocumentName = x.EconsentSetup.DocumentName,
                DocumentPath = x.EconsentSetup.DocumentPath,
                ReviewId = x.Id,
                IsReviewed = x.IsReviewedByPatient,
                TotalReviewTime = x.EconsentReviewDetailsSections.Sum(x => x.TimeInSeconds),
                IntroVideoPath = x.EconsentSetup.IntroVideoPath
            }).OrderByDescending(x => x.DocumentId).ToList();

            result.ForEach(t =>
            {
                t.DocumentPath = System.IO.Path.Combine(upload.DocumentPath, t.DocumentPath);
                if (t.IntroVideoPath != null)
                {
                    t.IntroVideoPath = System.IO.Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), t.IntroVideoPath).Replace('\\', '/');
                }
            });

            return result;
        }

        public List<SectionsHeader> GetEconsentSectionHeaders(int id)
        {
            var econsentreviewdetail = FindByInclude(x => x.Id == id, x => x.EconsentSetup, x => x.EconsentReviewDetailsSections).FirstOrDefault();
            // var document = _context.EconsentSetup.Where(x => x.Id == econsentreviewdetail.EconsentSetupId).FirstOrDefault();
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, econsentreviewdetail.EconsentSetup.DocumentPath);
            string path = FullPath;
            List<SectionsHeader> sectionsHeaders = new List<SectionsHeader>();
            if (System.IO.File.Exists(path))
            {
                Stream stream = System.IO.File.OpenRead(path);
                EJ2WordDocument doc = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                stream.Close();
                doc.Dispose();
                JObject jsonstr = JObject.Parse(json);
                Root jsonobj = JsonConvert.DeserializeObject<Root>(jsonstr.ToString());
                int sectioncount = 1;
                foreach (var e1 in jsonobj.sections)
                {
                    foreach (var e2 in e1.blocks)
                    {
                        if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                        {
                            SectionsHeader sectionsHeader = new SectionsHeader();
                            sectionsHeader.SectionNo = sectioncount;
                            sectionsHeader.SectionName = "Section " + sectioncount.ToString();
                            string headerstring = "";
                            foreach (var e3 in e2.inlines)
                            {
                                if (e3.text != null)
                                {
                                    headerstring = headerstring + e3.text;
                                }
                            }
                            sectionsHeader.Header = headerstring;
                            sectionsHeader.DocumentId = econsentreviewdetail.EconsentSetup.Id;
                            sectionsHeader.DocumentReviewId = econsentreviewdetail.Id;
                            sectionsHeader.DocumentName = econsentreviewdetail.EconsentSetup.DocumentName;
                            sectionsHeader.IsReadCompelete = econsentreviewdetail.IsReviewedByPatient;
                            sectionsHeader.IsReviewed = econsentreviewdetail.IsReviewedByPatient;
                            sectionsHeader.ReviewTime = econsentreviewdetail.EconsentReviewDetailsSections.Count > 0 ? econsentreviewdetail.EconsentReviewDetailsSections[sectioncount - 1].TimeInSeconds : 0;
                            sectionsHeaders.Add(sectionsHeader);
                            sectioncount++;
                        }
                    }
                }
            }
            return sectionsHeaders;
        }

        public List<SectionsHeader> GetEconsentDocumentHeadersByDocumentId(int documentId)
        {
            // this method is called from Econsent setup page, when clicking on eye icon in the grid (display section wise documents)
            var Econsentdocument = _context.EconsentSetup.Where(x => x.Id == documentId).ToList().FirstOrDefault();
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            List<SectionsHeader> sectionsHeaders = new List<SectionsHeader>();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, Econsentdocument.DocumentPath);
            string path = FullPath;
            if (System.IO.File.Exists(path))
            {
                Stream stream = System.IO.File.OpenRead(path);
                EJ2WordDocument doc = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
                string json = Newtonsoft.Json.JsonConvert.SerializeObject(doc);
                stream.Close();
                doc.Dispose();
                JObject jsonstr = JObject.Parse(json);
                Root jsonobj = JsonConvert.DeserializeObject<Root>(jsonstr.ToString());
                int sectioncount = 1;
                foreach (var e1 in jsonobj.sections)
                {
                    foreach (var e2 in e1.blocks)
                    {
                        if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                        {
                            SectionsHeader sectionsHeader = new SectionsHeader();
                            sectionsHeader.SectionNo = sectioncount;
                            sectionsHeader.SectionName = "Section " + sectioncount.ToString();
                            string headerstring = "";
                            foreach (var e3 in e2.inlines)
                            {
                                if (e3.text != null)
                                {
                                    headerstring = headerstring + e3.text;
                                }
                            }
                            sectionsHeader.Header = headerstring;
                            sectionsHeader.DocumentId = Econsentdocument.Id;
                            sectionsHeader.DocumentName = Econsentdocument.DocumentName;
                            sectionsHeaders.Add(sectionsHeader);
                            sectioncount++;
                        }
                    }
                }
            }
            return sectionsHeaders;
        }


        public AppEConsentSection ImportSectionDataHtml(int id, int sectionno)
        {
            // this method is called when clicking particular sections from the left side grid in Inform consent page(patient portal)
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var Econsentdocument = _context.EconsentSetup.Where(x => x.Id == id).FirstOrDefault();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, Econsentdocument.DocumentPath);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            string sfdtText = "";
            EJ2WordDocument wdocument = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(wdocument);
            wdocument.Dispose();
            string json = sfdtText;
            stream.Position = 0;
            stream.Close();
            GSC.Helper.DocumentReader.Root jsonobj = JsonConvert.DeserializeObject<GSC.Helper.DocumentReader.Root>(json);
            List<GSC.Helper.DocumentReader.Block> blocks = new List<GSC.Helper.DocumentReader.Block>();
            int headercount = 0;
            foreach (var e1 in jsonobj.sections)
            {
                foreach (var e2 in e1.blocks)
                {
                    if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                    {
                        headercount++;
                    }
                    if (sectionno == headercount)
                    {
                        blocks.Add(e2);
                    }
                }
            }

            for (int i = 0; i <= jsonobj.sections.Count - 1; i++)
            {
                jsonobj.sections[i].blocks = new List<GSC.Helper.DocumentReader.Block>();
                if (i == 0)
                {
                    jsonobj.sections[0].blocks = blocks;
                }
            }
            List<GSC.Helper.DocumentReader.Section> newsections = new List<GSC.Helper.DocumentReader.Section>();
            for (int i = 0; i <= jsonobj.sections.Count - 1; i++)
            {
                if (jsonobj.sections[i].blocks.Count > 0)
                {
                    newsections.Add(jsonobj.sections[i]);
                }
            }
            jsonobj.sections = newsections;
            string jsonnew = JsonConvert.SerializeObject(jsonobj, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });

            Stream document = EJ2WordDocument.Save(jsonnew, Syncfusion.EJ2.DocumentEditor.FormatType.Html);
            StreamReader reader = new StreamReader(document);
            var htmlStringText = reader.ReadToEnd();
            document.Close();
            document.Dispose();
            reader.Close();
            reader.Dispose();

            AppEConsentSection section = new AppEConsentSection();
            section.SectionHtml = htmlStringText;
            section.isReference = _context.EconsentSectionReference.Any(x => x.EconsentSetupId == id && x.DeletedDate == null);

            return section;
        }

        public string ImportSectionData(int id, int sectionno)
        {
            // this method is called when clicking particular sections from the left side grid in Inform consent page (patient portal)
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var Econsentdocument = _context.EconsentSetup.Where(x => x.Id == id).FirstOrDefault();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, Econsentdocument.DocumentPath);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
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
            List<Block> blocks = new List<Block>();
            int headercount = 0;
            foreach (var e1 in jsonobj.sections)
            {
                foreach (var e2 in e1.blocks)
                {
                    if (e2.paragraphFormat != null && e2.paragraphFormat.styleName == "Heading 1")
                    {
                        headercount++;
                    }
                    if (sectionno == headercount)
                    {
                        blocks.Add(e2);
                    }
                }
            }

            for (int i = 0; i <= jsonobj.sections.Count - 1; i++)
            {
                jsonobj.sections[i].blocks = new List<Block>();
                if (i == 0)
                {
                    jsonobj.sections[0].blocks = blocks;
                }
            }
            List<Section> newsections = new List<Section>();
            for (int i = 0; i <= jsonobj.sections.Count - 1; i++)
            {
                if (jsonobj.sections[i].blocks.Count > 0)
                {
                    newsections.Add(jsonobj.sections[i]);
                }
            }
            jsonobj.sections = newsections;
            string jsonnew = JsonConvert.SerializeObject(jsonobj, Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            return jsonnew;
        }
        public FileStreamResult GetEconsentDocument(int EconcentReviewId)
        {
            var document = Find(EconcentReviewId);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var FullPath = Path.Combine(upload.DocumentPath, document.PdfPath);
            //if (!File.Exists(FullPath))
            //    return null;
            GC.Collect();
            FileStream stream = new FileStream(FullPath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
            return new FileStreamResult(stream, "application/pdf");
        }
        public List<DashboardDto> GetEconsentMyTaskList(int ProjectId)
        {
            var projectIdlist = _context.Project.Where(x => x.ParentProjectId == ProjectId).Select(x => x.Id).ToList();
            var rolelist = _context.SiteTeam.Where(x => projectIdlist.Contains(x.ProjectId) && x.DeletedDate == null && x.IsIcfApproval == true).Select(x => x.RoleId).ToList();
            if (_context.ProjectRight.Any(x => rolelist.Contains(x.RoleId) && x.DeletedDate == null && x.RoleId == _jwtTokenAccesser.RoleId && x.UserId == _jwtTokenAccesser.UserId))
            {
                var result = _context.EconsentReviewDetails.Where(x => x.EconsentSetup.ProjectId == ProjectId && x.DeletedDate == null && x.Randomization.DeletedDate == null
                    && x.IsReviewedByPatient == true && x.IsReviewDoneByInvestigator == false
                    && (x.Randomization.PatientStatusId == ScreeningPatientStatus.ConsentInProcess || x.Randomization.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess)).Select(x => new DashboardDto
                    {
                        Id = x.Id,
                        TaskInformation = x.EconsentSetup.DocumentName + " for " + x.Randomization.Initial + " " + x.Randomization.ScreeningNumber + " is Pending approve from your side",
                        ExtraData = x.Id,
                        Module = MyTaskModule.InformConsent.GetDescription(),
                        ControlType = DashboardMyTaskType.EConsentData
                    }).ToList();
                return result;
            }
            return new List<DashboardDto>();
        }

        public CustomParameter downloadpdf(int id)
        {
            // after reviewed document patient can download pdf from dashboard
            var econsentreviewdetails = FindByInclude(x => x.Id == id, x => x.EconsentSetup, x => x.Randomization).FirstOrDefault();
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var docName = Path.Combine(upload.DocumentUrl, econsentreviewdetails.PdfPath);
            CustomParameter param = new CustomParameter();
            param.documentData = docName;
            //param.fileName = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            param.fileName = econsentreviewdetails.EconsentSetup.DocumentName + "_" + econsentreviewdetails.Randomization.ScreeningNumber;
            return param;
        }

        public List<EconsentReviewDetailsDto> GetEconsentReviewDetailsForSubjectManagement(int patientid)
        {
            //var econsentReview = All.Where(x => x.DeletedDate == null && x.RandomizationId == patientid).GroupBy(x => x.EconsentSetupId).ToList();

            var EconsentReviewDetails = All.Where(x => x.DeletedDate == null && x.RandomizationId == patientid && x.IsReviewedByPatient == true).
                                        ProjectTo<EconsentReviewDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            EconsentReviewDetails = EconsentReviewDetails.GroupBy(x => new { x.RandomizationId, x.EconsentSetupId })
                .Select(y => new EconsentReviewDetailsDto
                {
                    Id = y.FirstOrDefault().Id,
                    RandomizationId = y.Key.RandomizationId,
                    EconsentSetupId = y.Key.EconsentSetupId,
                    IsReviewDoneByInvestigator = y.FirstOrDefault().IsReviewDoneByInvestigator,
                    EconsentDocumentName = y.FirstOrDefault().EconsentDocumentName
                }).ToList();


            return EconsentReviewDetails;

        }

        public List<EconsentDocumentDetailsDto> GetEconsentReviewDetailsForPatientDashboard()
        {
            // display ICF details in patient dashboard
            var roleName = _jwtTokenAccesser.RoleName;

            var randomization = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();

            if (roleName == "LAR")
            {
                randomization = _context.Randomization.Where(x => x.LARUserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            }

            if (randomization == null)
                return null;

            var result = All.Where(x => x.RandomizationId == randomization.Id && x.EconsentSetup.DeletedDate == null && x.EconsentSetup.LanguageId == randomization.LanguageId
                         && (roleName == "LAR" ? x.IsLAR == true : x.IsLAR == null || x.IsLAR == false)).Include(x => x.EconsentSetup).ToList();
            if (result.Count <= 0)
                return null;

            var lastRecords = result.Where(q => q.EconsentSetup.CreatedDate.GetValueOrDefault().Ticks < randomization.CreatedDate.GetValueOrDefault().Ticks).OrderByDescending(o => o.EconsentSetupId).FirstOrDefault();
            var afterRecords = result.Where(q => q.EconsentSetup.CreatedDate.GetValueOrDefault().Ticks > randomization.CreatedDate.GetValueOrDefault().Ticks).ToList();
            afterRecords.Add(lastRecords);
            var resultDto = _mapper.Map<List<EconsentDocumentDetailsDto>>(afterRecords);
            return resultDto.OrderByDescending(o => o.Id).ToList();
        }

        public int UpdateDocument(EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            // uptate method calls when patient review document
            var econsentReviewDetail = _mapper.Map<EconsentReviewDetails>(econsentReviewDetailsDto);
            var original = Find(econsentReviewDetail.Id);
            econsentReviewDetail.PatientApprovedDatetime = _jwtTokenAccesser.GetClientDate();
            econsentReviewDetail.RandomizationId = original.RandomizationId;

            if (econsentReviewDetailsDto.PatientdigitalSignBase64?.Length > 0)
            {
                FileModel fileModel = new FileModel();
                fileModel.Base64 = econsentReviewDetailsDto.PatientdigitalSignBase64;
                fileModel.Extension = "png";
                econsentReviewDetail.PatientdigitalSignImagepath = new ImageService().ImageSave(fileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.InformConcent);
            }

            string filePath = string.Empty;

            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var projectId = _context.EconsentSetup.Where(x => x.Id == econsentReviewDetailsDto.EconsentSetupId).Select(x => x.ProjectId).FirstOrDefault();
            var docName = Guid.NewGuid().ToString() + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(projectId), FolderType.InformConcent.ToString(), docName);

            Byte[] byteArray = Convert.FromBase64String(econsentReviewDetailsDto.DocumentData);
            Stream stream = new MemoryStream(byteArray);

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
            document.Save(fileStream, Syncfusion.DocIO.FormatType.Docx);
            document.Close();

            Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(fileStream, Syncfusion.DocIO.FormatType.Automatic);

            stream.Dispose();
            stream.Close();
            fileStream.Dispose();
            fileStream.Close();

            DocIORenderer render = new DocIORenderer();
            render.Settings.PreserveFormFields = true;
            PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);
            render.Dispose();
            wordDocument.Dispose();
            MemoryStream outputStream = new MemoryStream();
            pdfDocument.Save(outputStream);
            pdfDocument.Close();

            int ProjectId = _context.EconsentReviewDetails.Where(x => x.Id == econsentReviewDetail.Id).Select(x => x.EconsentSetup.ProjectId).FirstOrDefault();
            var outputname = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            var pdfpath = Path.Combine(_jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent.ToString(), "ReviewedPDF", outputname);
            string directorypath = Path.Combine(_jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent.ToString(), "ReviewedPDF");
            //string[] paths = { upload.DocumentPath, FolderType.InformConcent.ToString(), "ReviewedPDF" };
            var fullPath = Path.Combine(upload.DocumentPath, directorypath);
            var outputFile = Path.Combine(upload.DocumentPath, directorypath, outputname);
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);
            file.Dispose();
            file.Close();
            outputStream.Dispose();
            outputStream.Close();


            econsentReviewDetail.PdfPath = pdfpath;
            econsentReviewDetail.IsReviewedByPatient = true;
            Update(econsentReviewDetail);

            var existing = _context.EconsentReviewDetailsSections.Where(t => t.EconsentReviewDetailId == econsentReviewDetail.Id).ToList();
            if (existing.Any())
            {
                _context.EconsentReviewDetailsSections.RemoveRange(econsentReviewDetail.EconsentReviewDetailsSections);
                _context.Save();
            }
            _context.EconsentReviewDetailsSections.AddRange(econsentReviewDetail.EconsentReviewDetailsSections);

            System.IO.File.Delete(filePath);
            if (_uow.Save() <= 0) throw new Exception("Creating Econsent review insert failed on save.");
            var Econsentsetup = _context.EconsentSetup.Where(x => x.Id == econsentReviewDetail.EconsentSetupId).FirstOrDefault();
            var project = _projectRepository.Find(Econsentsetup.ProjectId);
            var randomization = _context.Randomization.Where(x => x.Id == econsentReviewDetail.RandomizationId).FirstOrDefault();
            _emailSenderRespository.SendEmailOfPatientReviewedPDFtoPatient(randomization.Email, randomization.Initial + " " + randomization.ScreeningNumber, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
            //var EconsentApprovedroles = _context.EconsentSetupRoles.Where(x => x.EconsentDocumentId == Econsentsetup.Id).Select(x => x.RoleId);
            //var users = _context.ProjectRight.Where(x => x.ProjectId == randomization.ProjectId && EconsentApprovedroles.Contains(x.RoleId) && x.IsReviewDone == true).Select(x => x.UserId).Distinct();
            //var usersdata = _context.Users.Where(x => users.Contains(x.Id) && x.DeletedDate == null).ToList();
            //usersdata.ForEach(x =>
            //{
            //    _emailSenderRespository.SendEmailOfPatientReviewedPDFtoInvestigator(x.Email, x.UserName, Econsentsetup.DocumentName, project.ProjectCode, randomization.Initial + " " + randomization.ScreeningNumber, outputFile);
            //});
            return econsentReviewDetail.Id;
        }

        //public int ApproveRejectEconsentDocument(EconsentReviewDetailsDto econsentReviewDetailsDto)
        //{
        //    var generalSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
        //    generalSettings.TimeFormat = generalSettings.TimeFormat.Replace("a", "tt");
        //    // calls when investigator approve/reject document
        //    var econsentReviewDetails = Find(econsentReviewDetailsDto.Id);
        //    econsentReviewDetails.IsReviewDoneByInvestigator = true;
        //    econsentReviewDetails.InvestigatorReviewedDatetime = _jwtTokenAccesser.GetClientDate();
        //    econsentReviewDetails.ReviewDoneByRoleId = _jwtTokenAccesser.RoleId;
        //    econsentReviewDetails.ReviewDoneByUserId = _jwtTokenAccesser.UserId;
        //    econsentReviewDetails.IsApproved = econsentReviewDetailsDto.IsApproved;
        //    econsentReviewDetails.ApproveRejectReasonId = econsentReviewDetailsDto.ApproveRejectReasonId;
        //    econsentReviewDetails.ApproveRejectReasonOth = econsentReviewDetailsDto.ApproveRejectReasonOth;

        //    string filePath = string.Empty;

        //    var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();

        //    if (econsentReviewDetails.PdfPath != null)
        //    {
        //        //try
        //        //{
        //        string oldpdfpath = System.IO.Path.Combine(upload.DocumentPath, econsentReviewDetails?.PdfPath);
        //        if (File.Exists(oldpdfpath))
        //            System.IO.File.Delete(oldpdfpath);
        //        //}
        //        //catch (Exception ex)
        //        //{

        //        //}
        //    }
        //    int ProjectId = _context.EconsentReviewDetails.Where(x => x.Id == econsentReviewDetailsDto.Id).Select(x => x.EconsentSetup.ProjectId).FirstOrDefault();
        //    var docName = Guid.NewGuid().ToString() + DateTime.Now.Ticks + ".docx";
        //    filePath = System.IO.Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent.ToString(), docName);

        //    Byte[] byteArray = Convert.FromBase64String(econsentReviewDetailsDto.DocumentData);
        //    Stream stream = new MemoryStream(byteArray);

        //    FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
        //    Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
        //    document.Save(fileStream, Syncfusion.DocIO.FormatType.Docx);
        //    document.Close();

        //    Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(fileStream, Syncfusion.DocIO.FormatType.Automatic);

        //    stream.Close();
        //    stream.Dispose();
        //    fileStream.Close();
        //    fileStream.Dispose();

        //    DocIORenderer render = new DocIORenderer();
        //    render.Settings.PreserveFormFields = true;
        //    PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);

        //    int pagecount = pdfDocument.Pages.Count;
        //    PdfGraphics graphics = pdfDocument.Pages[pagecount - 1].Graphics;
        //    PdfFont fontbold = new PdfStandardFont(PdfFontFamily.TimesRoman, 13, PdfFontStyle.Bold);
        //    PdfFont fontnormal = new PdfStandardFont(PdfFontFamily.TimesRoman, 13);

        //    //var userdetails = _context.Users.Where(x => x.Id == econsentReviewDetails.ReviewDoneByUserId && x.DeletedDate==null).Include(x=>x.UserRoles).SingleOrDefault();

        //    if (econsentReviewDetailsDto.IsApproved == true)
        //        graphics.DrawString("Approved By: ", fontbold, PdfBrushes.Black, new PointF(70, 300));
        //    else
        //        graphics.DrawString("Reject By: ", fontbold, PdfBrushes.Black, new PointF(70, 300));

        //    graphics.DrawString(_jwtTokenAccesser.UserName + "(" + _jwtTokenAccesser.RoleName + ")", fontnormal, PdfBrushes.Black, new PointF(70, 320));
        //    graphics.DrawString(Convert.ToDateTime(econsentReviewDetails.InvestigatorReviewedDatetime).ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat), fontnormal, PdfBrushes.Black, new PointF(70, 340));

        //    if (econsentReviewDetailsDto.IsApproved == true)
        //        graphics.DrawString("Approved Reason: ", fontbold, PdfBrushes.Black, new PointF(70, 360));
        //    else
        //        graphics.DrawString("Reject Reason: ", fontbold, PdfBrushes.Black, new PointF(70, 360));

        //    string reasonName = _context.AuditReason.Where(x => x.Id == econsentReviewDetailsDto.ApproveRejectReasonId).FirstOrDefault().ReasonName;
        //    graphics.DrawString(reasonName, fontnormal, PdfBrushes.Black, new PointF(175, 360));
        //    if (econsentReviewDetailsDto.ApproveRejectReasonOth != null && econsentReviewDetailsDto.ApproveRejectReasonOth != "")
        //    {
        //        if (econsentReviewDetailsDto.IsApproved == true)
        //            graphics.DrawString("Approved Comment: ", fontbold, PdfBrushes.Black, new PointF(70, 380));
        //        else
        //            graphics.DrawString("Reject Comment: ", fontbold, PdfBrushes.Black, new PointF(70, 380));

        //        graphics.DrawString(econsentReviewDetailsDto.ApproveRejectReasonOth, fontnormal, PdfBrushes.Black, new PointF(70, 400));
        //    }
        //    render.Dispose();
        //    wordDocument.Dispose();
        //    MemoryStream outputStream = new MemoryStream();
        //    pdfDocument.Save(outputStream);
        //    pdfDocument.Close();


        //    var outputname = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
        //    var pdfpath = Path.Combine(_jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent.ToString(), "ReviewedPDF", outputname);
        //    var outputFile = Path.Combine(upload.DocumentPath, pdfpath);
        //    var fullpath = Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent.ToString(), "ReviewedPDF");
        //    if (!Directory.Exists(fullpath)) Directory.CreateDirectory(fullpath);
        //    FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
        //    outputStream.WriteTo(file);
        //    file.Close();
        //    file.Dispose();
        //    outputStream.Close();
        //    outputStream.Dispose();

        //    econsentReviewDetails.PdfPath = pdfpath;

        //    Update(econsentReviewDetails);
        //    System.IO.File.Delete(filePath);

        //    _uow.Save();
        //    var Econsentsetup = _context.EconsentSetup.Where(x => x.Id == econsentReviewDetails.EconsentSetupId).ToList().FirstOrDefault();
        //    var project = _projectRepository.Find(Econsentsetup.ProjectId);
        //    var randomization = _context.Randomization.Where(x => x.Id == econsentReviewDetails.RandomizationId).ToList().FirstOrDefault();
        //    _emailSenderRespository.SendEmailOfInvestigatorApprovedPDFtoPatient(randomization.Email, randomization.Initial + " " + randomization.ScreeningNumber, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
        //    if (econsentReviewDetailsDto.IsApproved == true)
        //        _randomizationRepository.ChangeStatustoConsentCompleted(econsentReviewDetails.RandomizationId);
        //    else
        //    {
        //        var randomizationdata = _randomizationRepository.Find(econsentReviewDetails.RandomizationId);
        //        randomizationdata.PatientStatusId = ScreeningPatientStatus.Withdrawal;
        //        _randomizationRepository.Update(randomizationdata);
        //    }
        //    _uow.Save();
        //    return econsentReviewDetails.Id;
        //}



        public int ApproveRejectEconsentDocument(EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            var generalSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            generalSettings.TimeFormat = generalSettings.TimeFormat.Replace("a", "tt");
            // calls when investigator approve/reject document
            var econsentReviewDetails = Find(econsentReviewDetailsDto.Id);
            econsentReviewDetails.IsReviewDoneByInvestigator = true;
            econsentReviewDetails.InvestigatorReviewedDatetime = _jwtTokenAccesser.GetClientDate();
            econsentReviewDetails.ReviewDoneByRoleId = _jwtTokenAccesser.RoleId;
            econsentReviewDetails.ReviewDoneByUserId = _jwtTokenAccesser.UserId;
            econsentReviewDetails.IsApproved = econsentReviewDetailsDto.IsApproved;
            econsentReviewDetails.ApproveRejectReasonId = econsentReviewDetailsDto.ApproveRejectReasonId;
            econsentReviewDetails.ApproveRejectReasonOth = econsentReviewDetailsDto.ApproveRejectReasonOth;

            var AllDoc = All.Where(x => x.RandomizationId == econsentReviewDetailsDto.RandomizationId && x.EconsentSetupId == econsentReviewDetailsDto.EconsentSetupId && x.Id != econsentReviewDetailsDto.Id).FirstOrDefault();
            if (AllDoc != null)
            {
                AllDoc.IsReviewDoneByInvestigator = true;
                AllDoc.InvestigatorReviewedDatetime = _jwtTokenAccesser.GetClientDate();
                AllDoc.ReviewDoneByRoleId = _jwtTokenAccesser.RoleId;
                AllDoc.ReviewDoneByUserId = _jwtTokenAccesser.UserId;
                AllDoc.IsApproved = econsentReviewDetailsDto.IsApproved;
                AllDoc.ApproveRejectReasonId = econsentReviewDetailsDto.ApproveRejectReasonId;
                AllDoc.ApproveRejectReasonOth = econsentReviewDetailsDto.ApproveRejectReasonOth;
            }

            string filePath = string.Empty;

            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            int ProjectId = _context.EconsentReviewDetails.Where(x => x.Id == econsentReviewDetailsDto.Id).Select(x => x.EconsentSetup.ProjectId).FirstOrDefault();

            PdfDocument pdfDocument = new PdfDocument();
            string filepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), econsentReviewDetails.PdfPath);
            FileStream docStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
            pdfDocument.ImportPageRange(loadedDocument, 0, loadedDocument.Pages.Count - 1);
            //PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);



            int pagecount = pdfDocument.Pages.Count;
            PdfGraphics graphics = pdfDocument.Pages[pagecount - 1].Graphics;
            PdfFont fontbold = new PdfStandardFont(PdfFontFamily.TimesRoman, 13, PdfFontStyle.Bold);
            PdfFont regular = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Regular);


            PdfPage page = pdfDocument.Pages.Add();
            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            PdfLayoutResult result = new PdfLayoutResult(page, bounds);

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;



            if (econsentReviewDetailsDto.IsApproved == true)
                result = AddString("Approved By: ", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
            else
                result = AddString("Reject By: ", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);

            result = AddString(_jwtTokenAccesser.UserName + "(" + _jwtTokenAccesser.RoleName + ")", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, 500, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);
            result = AddString(Convert.ToDateTime(econsentReviewDetails.InvestigatorReviewedDatetime).ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat), result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, 500, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);

            if (econsentReviewDetailsDto.IsApproved == true)
                result = AddString("Approved Reason: ", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
            else
                result = AddString("Reject Reason: ", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);


            string reasonName = _context.AuditReason.Where(x => x.Id == econsentReviewDetailsDto.ApproveRejectReasonId).FirstOrDefault().ReasonName;
            result = AddString(reasonName, result.Page, new Syncfusion.Drawing.RectangleF(175, result.Bounds.Bottom + 20, 500, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);
            if (econsentReviewDetailsDto.ApproveRejectReasonOth != null && econsentReviewDetailsDto.ApproveRejectReasonOth != "")
            {
                if (econsentReviewDetailsDto.IsApproved == true)
                    result = AddString("Approved Comment: ", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
                else
                    result = AddString("Reject Comment: ", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);

                result = AddString(econsentReviewDetailsDto.ApproveRejectReasonOth, result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, 500, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);
            }
            PdfStringFormat stringFormat = new PdfStringFormat();
            stringFormat.MeasureTrailingSpaces = true;
            stringFormat.WordWrap = PdfWordWrapType.WordOnly;
            string message = "I, hereby understand, that applying my electronic signature in the electronic system is equivalent \n to utilising my hand written signature";
            result = AddString(message, result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);
            //render.Dispose();
            //wordDocument.Dispose();
            MemoryStream outputStream = new MemoryStream();
            pdfDocument.Save(outputStream);
            pdfDocument.Close();


            var outputname = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            var pdfpath = Path.Combine(_jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent.ToString(), "ReviewedPDF", outputname);
            var outputFile = Path.Combine(upload.DocumentPath, pdfpath);
            var fullpath = Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent.ToString(), "ReviewedPDF");
            if (!Directory.Exists(fullpath)) Directory.CreateDirectory(fullpath);
            FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);
            file.Close();
            file.Dispose();
            outputStream.Close();
            outputStream.Dispose();
            docStream.Close();
            docStream.Dispose();

            econsentReviewDetails.PdfPath = pdfpath;

            var details = _mapper.Map<EconsentReviewDetails>(econsentReviewDetails);
            Update(details);

            if (AllDoc != null)
            {
                AllDoc.PdfPath = pdfpath;
                Update(AllDoc);
            }

            //System.IO.File.Delete(filePath);
            _uow.Save();
            var Econsentsetup = _context.EconsentSetup.Where(x => x.Id == econsentReviewDetails.EconsentSetupId).ToList().FirstOrDefault();
            var project = _projectRepository.Find(Econsentsetup.ProjectId);
            var randomization = _context.Randomization.Where(x => x.Id == econsentReviewDetails.RandomizationId).ToList().FirstOrDefault();
            if (econsentReviewDetailsDto.IsApproved == true)
            {
                _emailSenderRespository.SendEmailOfInvestigatorApprovedPDFtoPatient(randomization.Email, randomization.Initial + " " + randomization.ScreeningNumber, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
                if (randomization.LegalFirstName != null)
                {
                    _emailSenderRespository.SendEmailOfInvestigatorApprovedPDFtoPatient(randomization.LegalEmail, randomization.LegalFirstName + " " + randomization.LegalLastName, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
                }
            }
            else
            {
                _emailSenderRespository.SendEmailOfRejectedDocumenttoPatient(randomization.Email, randomization.Initial + " " + randomization.ScreeningNumber, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
                if (randomization.LegalFirstName != null)
                {
                    _emailSenderRespository.SendEmailOfRejectedDocumenttoPatient(randomization.LegalEmail, randomization.LegalFirstName + " " + randomization.LegalLastName, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
                }
            }
            if (econsentReviewDetailsDto.IsApproved == true)
                _randomizationRepository.ChangeStatustoConsentCompleted(econsentReviewDetails.RandomizationId);
            else
            {
                var randomizationdata = _randomizationRepository.Find(econsentReviewDetails.RandomizationId);
                randomizationdata.PatientStatusId = ScreeningPatientStatus.Withdrawal;
                _randomizationRepository.Update(randomizationdata);
            }

            //reviewdetail audit
            EconsentReviewDetailsAudit audit = new EconsentReviewDetailsAudit();
            audit.EconsentReviewDetailsId = details.Id;
            audit.Activity = econsentReviewDetailsDto.IsApproved == true ? ICFAction.Approve : ICFAction.Withdraw;
            audit.PateientStatus = randomization.PatientStatusId;
            _econsentReviewDetailsAuditRepository.Add(audit);
            _uow.Save();
            return econsentReviewDetails.Id;
        }
        public int ApproveWithDrawPatient(EconsentDocumetViwerDto econsentReviewDetailsDto, bool isWithdraw)
        {
            var generalSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            generalSettings.TimeFormat = generalSettings.TimeFormat.Replace("a", "tt");

            var reviewdetails = All.Where(x => x.Id == econsentReviewDetailsDto.EconcentReviewDetailsId).FirstOrDefault();
            var AllDoc = All.Where(x => x.RandomizationId == reviewdetails.RandomizationId && x.EconsentSetupId == reviewdetails.EconsentSetupId && x.Id != reviewdetails.Id).FirstOrDefault();

            var econsentSetupDetails = _context.EconsentSetup.Where(x => x.Id == reviewdetails.EconsentSetupId).FirstOrDefault();
            var randomization = _context.Randomization.Where(x => x.Id == reviewdetails.RandomizationId).FirstOrDefault();
            if (econsentReviewDetailsDto.PatientdigitalSignBase64?.Length > 0)
            {
                FileModel fileModel = new FileModel();
                fileModel.Base64 = econsentReviewDetailsDto.PatientdigitalSignBase64;
                if (string.IsNullOrEmpty(econsentReviewDetailsDto.FileExtension))
                {
                    fileModel.Extension = "png";
                    reviewdetails.PatientdigitalSignImagepath = new ImageService().ImageSave(fileModel,
                   _uploadSettingRepository.GetImagePath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.InformConcent, "");
                }
                else
                {
                    fileModel.Extension = econsentReviewDetailsDto.FileExtension;
                    reviewdetails.PatientdigitalSignImagepath = DocumentService.SaveUploadDocument(fileModel,
                        _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.InformConcent, "EconsentReviewVideo");
                }
            }
            reviewdetails.EconsentReviewDetailsSections = _mapper.Map<List<EconsentReviewDetailsSections>>(econsentReviewDetailsDto.EconsentReviewDetailsSections);
            string filepath = "";
            PdfDocument pdfDocument = new PdfDocument();
            FileStream docStream;
            if (String.IsNullOrEmpty(reviewdetails.PdfPath) && reviewdetails.IsReviewedByPatient == false)
            {
                filepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), econsentSetupDetails.DocumentPath);
                docStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(docStream, Syncfusion.DocIO.FormatType.Automatic);
                DocIORenderer render = new DocIORenderer();
                render.Settings.PreserveFormFields = true;
                pdfDocument = render.ConvertToPDF(wordDocument);
                render.Dispose();
                wordDocument.Dispose();
                //docStream.Close();
                //docStream.Dispose();               
            }
            else
            {
                filepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), reviewdetails.PdfPath);
                docStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
                PdfLoadedDocument loadedDocument = new PdfLoadedDocument(docStream);
                pdfDocument.ImportPageRange(loadedDocument, 0, loadedDocument.Pages.Count - 1);
                //docStream.Close();
                //docStream.Dispose();
            }


            //add signature
            //pdfDocument = CreateSignature(pdfDocument, Id);

            PdfPage page = pdfDocument.Pages.Add();

            RectangleF bounds = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
            PdfLayoutResult result = new PdfLayoutResult(page, bounds);

            PdfLayoutFormat layoutFormat = new PdfLayoutFormat();
            layoutFormat.Layout = PdfLayoutType.Paginate;
            layoutFormat.Break = PdfLayoutBreakType.FitElement;
            PdfGraphics graphics = page.Graphics;
            //Load the image from the disk
            PdfImage image = null;
            if (string.IsNullOrEmpty(econsentReviewDetailsDto.FileExtension))
            {
                FileStream logoinputstream = new FileStream($"{_uploadSettingRepository.GetDocumentPath()}/{reviewdetails.PatientdigitalSignImagepath}", FileMode.Open, FileAccess.Read);
                image = new PdfBitmap(logoinputstream);
                logoinputstream.Close();
                logoinputstream.Dispose();
            }
            //Draw the image   

            PdfFont fontbold = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Bold);
            PdfFont regular = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Regular);
            PdfStringFormat format = new PdfStringFormat();
            //format.Alignment = PdfTextAlignment.Left;
            //format.LineAlignment = PdfVerticalAlignment.Top;
            format.MeasureTrailingSpaces = true;
            format.WordWrap = PdfWordWrapType.Word;

            if (isWithdraw == false)
            {
                //graphics.DrawString("Volunteer Initial:", fontbold, PdfBrushes.Black, new PointF(70, 30), format);
                //graphics.DrawString($"{randomization.ScreeningNumber + " " + randomization.Initial}", regular, PdfBrushes.Black, new PointF(170, 30), format);
                AddString(reviewdetails.IsLAR == true ? "LAR" : "Volunteer Initial:", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
                result = AddString(reviewdetails.IsLAR == true ? $"{randomization.LegalFirstName + " " + randomization.LegalLastName}" : $"{randomization.ScreeningNumber + " " + randomization.Initial}", result.Page, new Syncfusion.Drawing.RectangleF(170, result.Bounds.Bottom + 20, 500, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);

                //graphics.DrawString("Volunteer Signature:", fontbold, PdfBrushes.Black, new PointF(70, 50), format);
                //graphics.DrawImage(image, new PointF(70, 70), new SizeF(400f, 100f));
                result = AddString(reviewdetails.IsLAR == true ? "LAR Signature:" : "Volunteer Signature:", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
                if (image != null)
                {
                    result.Page.Graphics.DrawImage(image, new PointF(70, result.Bounds.Y + 20), new SizeF(400f, 100f));
                }
                //graphics.DrawString("DateTime:", fontbold, PdfBrushes.Black, new PointF(70, 230), format);
                //graphics.DrawString($"{_jwtTokenAccesser.GetClientDate().ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat)}", regular, PdfBrushes.Black, new PointF(140, 230), format);

                AddString("DateTime:", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Y + 180, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
                result = AddString($"{_jwtTokenAccesser.GetClientDate().ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat)}", result.Page, new Syncfusion.Drawing.RectangleF(140, result.Bounds.Y + 180, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);

                //if (AllDoc != null && AllDoc.IsReviewedByPatient)
                //{
                //    //add signature
                //    //pdfDocument = CreateSignature(pdfDocument, Id);

                //    PdfPage page1 = pdfDocument.Pages.Add();

                //    RectangleF bounds1 = new RectangleF(new PointF(0, 10), new SizeF(0, 0));
                //    PdfLayoutResult result1 = new PdfLayoutResult(page1, bounds1);

                //    PdfLayoutFormat layoutFormat1 = new PdfLayoutFormat();
                //    layoutFormat1.Layout = PdfLayoutType.Paginate;
                //    layoutFormat1.Break = PdfLayoutBreakType.FitElement;
                //    PdfGraphics graphics1 = page1.Graphics;
                //    //Load the image from the disk
                //    FileStream logoinputstream1 = new FileStream($"{_uploadSettingRepository.GetDocumentPath()}/{AllDoc.PatientdigitalSignImagepath}", FileMode.Open, FileAccess.Read);
                //    PdfImage image1 = new PdfBitmap(logoinputstream1);
                //    logoinputstream1.Close();
                //    logoinputstream1.Dispose();
                //    //Draw the image   

                //    PdfFont fontbold1 = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Bold);
                //    PdfFont regular1 = new PdfStandardFont(PdfFontFamily.TimesRoman, 12, PdfFontStyle.Regular);
                //    PdfStringFormat format1 = new PdfStringFormat();
                //    //format.Alignment = PdfTextAlignment.Left;
                //    //format.LineAlignment = PdfVerticalAlignment.Top;
                //    format1.MeasureTrailingSpaces = true;
                //    format1.WordWrap = PdfWordWrapType.Word;

                //    //graphics.DrawString("Volunteer Initial:", fontbold, PdfBrushes.Black, new PointF(70, 30), format);
                //    //graphics.DrawString($"{randomization.ScreeningNumber + " " + randomization.Initial}", regular, PdfBrushes.Black, new PointF(170, 30), format);
                //    AddString(AllDoc.IsLAR == true ? "LAR" : "Volunteer Initial:", result1.Page, new Syncfusion.Drawing.RectangleF(70, result1.Bounds.Bottom + 20, result1.Page.GetClientSize().Width, result1.Page.GetClientSize().Height), PdfBrushes.Black, fontbold1, layoutFormat1);
                //    result1 = AddString(AllDoc.IsLAR == true ? $"{randomization.LegalFirstName + " " + randomization.LegalLastName}" : $"{randomization.ScreeningNumber + " " + randomization.Initial}", result1.Page, new Syncfusion.Drawing.RectangleF(170, result1.Bounds.Bottom + 20, 500, result1.Page.GetClientSize().Height), PdfBrushes.Black, regular1, layoutFormat1);

                //    //graphics.DrawString("Volunteer Signature:", fontbold, PdfBrushes.Black, new PointF(70, 50), format);
                //    //graphics.DrawImage(image, new PointF(70, 70), new SizeF(400f, 100f));
                //    result1 = AddString(AllDoc.IsLAR == true ? "LAR Signature:" : "Volunteer Signature:", result1.Page, new Syncfusion.Drawing.RectangleF(70, result1.Bounds.Bottom + 20, result1.Page.GetClientSize().Width, result1.Page.GetClientSize().Height), PdfBrushes.Black, fontbold1, layoutFormat1);
                //    result1.Page.Graphics.DrawImage(image1, new PointF(70, result1.Bounds.Y + 20), new SizeF(400f, 100f));

                //    //graphics.DrawString("DateTime:", fontbold, PdfBrushes.Black, new PointF(70, 230), format);
                //    //graphics.DrawString($"{_jwtTokenAccesser.GetClientDate().ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat)}", regular, PdfBrushes.Black, new PointF(140, 230), format);

                //    AddString("DateTime:", result1.Page, new Syncfusion.Drawing.RectangleF(70, result1.Bounds.Y + 180, result1.Page.GetClientSize().Width, result1.Page.GetClientSize().Height), PdfBrushes.Black, fontbold1, layoutFormat1);
                //    result1 = AddString($"{_jwtTokenAccesser.GetClientDate().ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat)}", result1.Page, new Syncfusion.Drawing.RectangleF(140, result1.Bounds.Y + 180, result1.Page.GetClientSize().Width, result1.Page.GetClientSize().Height), PdfBrushes.Black, regular1, layoutFormat1);

                //}
            }

            if (isWithdraw == true)
            {
                var reason = _jwtTokenAccesser.GetHeader("audit-reason-name");
                var reasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
                //graphics.DrawString("Withdraw By:", fontbold, PdfBrushes.Black, new PointF(70, 250), format);
                //graphics.DrawString($"{_jwtTokenAccesser.UserName + "(" + _jwtTokenAccesser.RoleName + ")"}", regular, PdfBrushes.Black, new PointF(150, 250), format);
                AddString("Withdraw By:", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
                result = AddString($"{_jwtTokenAccesser.UserName + "(" + _jwtTokenAccesser.RoleName + ")"}", result.Page, new Syncfusion.Drawing.RectangleF(150, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);

                //graphics.DrawString("Withdraw Reason:", fontbold, PdfBrushes.Black, new PointF(70, 280), format);
                //graphics.DrawString($"{reason}", regular, PdfBrushes.Black, new PointF(180, 280), format);
                AddString("Withdraw Reason:", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
                result = AddString($"{reason}", result.Page, new Syncfusion.Drawing.RectangleF(180, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);

                //graphics.DrawString("Comment:", fontbold, PdfBrushes.Black, new PointF(70, 300), format);
                //graphics.DrawString($"{reasonOth}", regular, PdfBrushes.Black, new PointF(70, 320), format);
                result = AddString("Comment:", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Y + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, fontbold, layoutFormat);
                result = AddString($"{reasonOth}", result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Y + 20, 500, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);

                reviewdetails.IsWithDraw = true;
                reviewdetails.WithdrawReason = reason;
                reviewdetails.WithdrawComment = reasonOth;
            }
            //graphics.DrawString("I, hereby understand, that applying my electronic signature in the electronic system is equivalent \n to utilising my hand written signature", regular, PdfBrushes.Black, new PointF(70, isWithdraw ? 500 : 400), format);
            string message = "I, hereby understand, that applying my electronic signature in the electronic system is equivalent \n to utilising my hand written signature";
            result = AddString(message, result.Page, new Syncfusion.Drawing.RectangleF(70, result.Bounds.Bottom + 20, result.Page.GetClientSize().Width, result.Page.GetClientSize().Height), PdfBrushes.Black, regular, layoutFormat);
            MemoryStream outputStream = new MemoryStream();
            pdfDocument.Save(outputStream);
            pdfDocument.Close();
            pdfDocument.Dispose();
            docStream.Close();
            docStream.Dispose();

            var filename = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            var pdfpath = Path.Combine(_jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(econsentSetupDetails.ProjectId), FolderType.InformConcent.ToString(), "ReviewedPDF", filename);
            var diractorypath = Path.Combine(_jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(econsentSetupDetails.ProjectId), FolderType.InformConcent.ToString(), "ReviewedPDF");
            var filewritepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), pdfpath);
            var fulldiractorypath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), diractorypath);
            if (!Directory.Exists(fulldiractorypath))
                Directory.CreateDirectory(fulldiractorypath);
            // var filewritepath = Path.Combine(outputFile, filename);
            FileStream file = new FileStream(filewritepath, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);
            file.Close();
            file.Dispose();
            outputStream.Close();
            outputStream.Dispose();

            //var reviewDetails = _context.EconsentReviewDetails.Find(item.Id);
            reviewdetails.PdfPath = pdfpath;
            reviewdetails.IsReviewedByPatient = true;
            reviewdetails.PatientdigitalSignImagepath = reviewdetails.PatientdigitalSignImagepath;
            reviewdetails.PatientApprovedDatetime = DateTime.Now;
            var details = _mapper.Map<EconsentReviewDetails>(reviewdetails);
            _context.EconsentReviewDetails.Update(details);

            if (AllDoc != null)
            {
                AllDoc.PdfPath = pdfpath;
                _context.EconsentReviewDetails.Update(AllDoc);
            }

            //auditlog
            EconsentReviewDetailsAudit audit = new EconsentReviewDetailsAudit();
            audit.EconsentReviewDetailsId = details.Id;
            audit.Activity = isWithdraw ? ICFAction.Withdraw : ICFAction.Approve;
            audit.PateientStatus = randomization.PatientStatusId;
            _econsentReviewDetailsAuditRepository.Add(audit);
            _context.Save();

            var project = _projectRepository.Find(econsentSetupDetails.ProjectId);
            if (!isWithdraw)
                _emailSenderRespository.SendEmailOfPatientReviewedPDFtoPatient(randomization.Email, randomization.Initial + " " + randomization.ScreeningNumber, econsentSetupDetails.DocumentName, project.ProjectCode, filewritepath);

            var siteteam = _context.SiteTeam.Where(x => x.ProjectId == randomization.ProjectId && x.DeletedDate == null && x.IsIcfApproval == true).Select(x => x.RoleId).ToList();
            var users = _context.ProjectRight.Where(x => x.ProjectId == randomization.ProjectId && siteteam.Contains(x.RoleId) && x.IsReviewDone == true).Select(x => x.UserId).Distinct();
            var usersdata = _context.Users.Where(x => users.Contains(x.Id) && x.DeletedDate == null).ToList();
            var roleName = _jwtTokenAccesser.RoleName;

            usersdata.ForEach(x =>
            {
                if (!String.IsNullOrEmpty(x.Email))
                    if (!isWithdraw)
                    {
                        if (roleName == "LAR")
                            _emailSenderRespository.SendEmailOfLARReviewedPDFtoInvestigator(x.Email, x.UserName, econsentSetupDetails.DocumentName, project.ProjectCode, randomization.LegalFirstName + " " + randomization.LegalLastName, filewritepath);
                        else
                            _emailSenderRespository.SendEmailOfPatientReviewedPDFtoInvestigator(x.Email, x.UserName, econsentSetupDetails.DocumentName, project.ProjectCode, randomization.Initial + " " + randomization.ScreeningNumber, filewritepath);
                    }
                    else
                    {
                        if (roleName == "LAR")
                            _emailSenderRespository.SendWithDrawEmailLAR(x.Email, x.FirstName, econsentSetupDetails.DocumentName, project.ProjectCode, $"{randomization.LegalFirstName + " " + randomization.LegalLastName}", filewritepath);
                        else
                            _emailSenderRespository.SendWithDrawEmail(x.Email, x.FirstName, econsentSetupDetails.DocumentName, project.ProjectCode, $"{randomization.ScreeningNumber + " " + randomization.Initial}", filewritepath);
                    }

            });
            return reviewdetails.Id;
        }

        private PdfLayoutResult AddString(string note, PdfPage page, RectangleF position, PdfBrush brush, PdfFont font, PdfLayoutFormat pdfLayoutFormat)
        {
            //font.SetTextEncoding(Encoding.GetEncoding("Windows-1250"))
            //PdfTrueTypeFont fonts = new PdfTrueTypeFont(new Font("Microsoft Sans Serif", 14), true);

            PdfTextElement richTextElement = new PdfTextElement(String.IsNullOrEmpty(note) ? " " : note, font, brush);
            //Draws String       
            PdfStringFormat stringFormat = new PdfStringFormat();
            stringFormat.MeasureTrailingSpaces = true;
            stringFormat.WordWrap = PdfWordWrapType.Word;
            richTextElement.StringFormat = stringFormat;

            PdfLayoutResult result = richTextElement.Draw(page, position, pdfLayoutFormat);
            return result;
        }

    }
}
