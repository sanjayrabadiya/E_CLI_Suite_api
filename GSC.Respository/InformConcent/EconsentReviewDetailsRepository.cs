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

        public EconsentReviewDetailsRepository(IGSCContext context,
                                                IJwtTokenAccesser jwtTokenAccesser,
                                                IProjectRepository projectRepository,
                                                IUserRepository userRepository,
                                                IMapper mapper,
                                                IUploadSettingRepository uploadSettingRepository,
                                                IEmailSenderRespository emailSenderRespository,
                                                IUnitOfWork uow,
                                                IRandomizationRepository randomizationRepository, IAppSettingRepository appSettingRepository) : base(context)
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
        }

        public List<EConsentDocumentHeader> GetEconsentDocumentHeaders()
        {
            // this method calls when patient login and click on menu inform consent, documents with headers displays on left side returned in this API
            var noneregister = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            if (noneregister == null) return new List<EConsentDocumentHeader>();
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var result = _context.EconsentReviewDetails.Where(x => x.RandomizationId == noneregister.Id && x.EconsentSetup.DeletedDate == null && x.EconsentSetup.LanguageId == noneregister.LanguageId).Select(x => new EConsentDocumentHeader
            {
                DocumentId = x.EconsentSetup.Id,
                DocumentName = x.EconsentSetup.DocumentName,
                DocumentPath = x.EconsentSetup.DocumentPath,
                ReviewId = x.Id,
                IsReviewed = x.IsReviewedByPatient,
                TotalReviewTime = x.EconsentReviewDetailsSections.Sum(x => x.TimeInSeconds)
            }).ToList();

            result.ForEach(t => t.DocumentPath = System.IO.Path.Combine(upload.DocumentPath, t.DocumentPath));

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
                            sectionsHeader.sectionNo = sectioncount;
                            sectionsHeader.sectionName = "Section " + sectioncount.ToString();
                            string headerstring = "";
                            foreach (var e3 in e2.inlines)
                            {
                                if (e3.text != null)
                                {
                                    headerstring = headerstring + e3.text;
                                }
                            }
                            sectionsHeader.header = headerstring;
                            sectionsHeader.documentId = econsentreviewdetail.EconsentSetup.Id;
                            sectionsHeader.documentReviewId = econsentreviewdetail.Id;
                            sectionsHeader.documentName = econsentreviewdetail.EconsentSetup.DocumentName;
                            sectionsHeader.isReadCompelete = econsentreviewdetail.IsReviewedByPatient;
                            sectionsHeader.isReviewed = econsentreviewdetail.IsReviewedByPatient;
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
                            sectionsHeader.sectionNo = sectioncount;
                            sectionsHeader.sectionName = "Section " + sectioncount.ToString();
                            string headerstring = "";
                            foreach (var e3 in e2.inlines)
                            {
                                if (e3.text != null)
                                {
                                    headerstring = headerstring + e3.text;
                                }
                            }
                            sectionsHeader.header = headerstring;
                            sectionsHeader.documentId = Econsentdocument.Id;
                            sectionsHeader.documentName = Econsentdocument.DocumentName;
                            sectionsHeaders.Add(sectionsHeader);
                            sectioncount++;
                        }
                    }
                }
            }
            return sectionsHeaders;
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

        public string GetEconsentDocument(EconsentDocumetViwerDto econsentreviewdetails)
        {
            var generalSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            generalSettings.TimeFormat = generalSettings.TimeFormat.Replace("a", "tt");
            // this method is called when patient reviewed document and completes the signature 
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            //var econsentdocument = _context.EconsentSetup.Where(x => x.Id == EconsentSetupId).FirstOrDefault();
            var econcentreview = All.Where(x => x.Id == econsentreviewdetails.EconcentReviewDetailsId).Include(x => x.EconsentSetup).FirstOrDefault();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, econcentreview.EconsentSetup.DocumentPath);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            string sfdtText = "";
            EJ2WordDocument wdocument = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(wdocument);
            wdocument.Dispose();
            string json = sfdtText;
            var jsonObj = JObject.Parse(json);
            string sign = File.ReadAllText("Config//signaturefooterblock.json");
            string sign2 = sign;

            var randomization = _context.Randomization.Where(x => x.Id == econcentreview.RandomizationId).ToList().FirstOrDefault();//_noneRegisterRepository.Find(randomizationId);
            if (econcentreview.IsReviewedByPatient)
            {
                string randomizationsignaturepath = System.IO.Path.Combine(upload.DocumentPath, econcentreview.PatientdigitalSignImagepath);
                string signRandombase64 = DocumentService.ConvertBase64Image(randomizationsignaturepath);
                sign = sign.Replace("_imagepath_", signRandombase64);
            }
            else
            {
                sign = sign.Replace("_imagepath_", econsentreviewdetails.PatientdigitalSignBase64);
            }
            sign = sign.Replace("_volunterlabel_", "Volunteer");
            sign = sign.Replace("Name", "Initial");
            sign = sign.Replace("_voluntername_", randomization.ScreeningNumber + " " + randomization.Initial);
            sign = sign.Replace("_datetime_", (econcentreview.Patientapproveddatetime == null) ? _jwtTokenAccesser.GetClientDate().ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat) : Convert.ToDateTime(econcentreview.Patientapproveddatetime).ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat));
            var jsonObj2 = JObject.Parse(sign);
            jsonObj.Merge(jsonObj2, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            if (econcentreview.IsReviewedByPatient == true)
            {
                var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                string signinvestigatorbase64 = user.SignatureBase64String == null ? "" : user.SignatureBase64String;//DocumentService.ConvertBase64Image(investigatorsignaturepath);
                sign2 = sign2.Replace("_volunterlabel_", "Investigator");
                sign2 = sign2.Replace("_imagepath_", signinvestigatorbase64);
                sign2 = sign2.Replace("_voluntername_", user.UserName);
                sign2 = sign2.Replace("_datetime_", econcentreview.InvestigatorRevieweddatetime == null ? _jwtTokenAccesser.GetClientDate().ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat) : Convert.ToDateTime(econcentreview.InvestigatorRevieweddatetime).ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat));
                var jsonObj3 = JObject.Parse(sign2);
                jsonObj.Merge(jsonObj3, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }

            json = jsonObj.ToString();
            stream.Close();
            stream.Dispose();
            return json;
        }

        public List<DashboardDto> GetEconsentMyTaskList(int ProjectId)
        {
            // method calls in dashboard My Task
            var result = (
                          from econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null)
                          join econsentroles in _context.EconsentSetupRoles.Where(x => x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null) on econsentsetups.Id equals econsentroles.EconsentDocumentId
                          join EconsentReviewDetails in _context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsReviewedByPatient == true && x.IsReviewDoneByInvestigator == false) on econsentsetups.Id equals EconsentReviewDetails.EconsentSetupId
                          join nonregister in _context.Randomization.Where(x => x.DeletedDate == null && (x.PatientStatusId == ScreeningPatientStatus.ConsentInProcess || x.PatientStatusId == ScreeningPatientStatus.ReConsentInProcess)) on EconsentReviewDetails.RandomizationId equals nonregister.Id

                          select new DashboardDto
                          {
                              Id = EconsentReviewDetails.Id,
                              TaskInformation = econsentsetups.DocumentName + " for " + nonregister.Initial + " " + nonregister.ScreeningNumber + " is Pending approve from your side",
                              ExtraData = EconsentReviewDetails.Id,
                              Module = MyTaskModule.InformConsent.GetDescription(),
                          }).ToList();
            return result;
        }

        public CustomParameter downloadpdf(int id)
        {
            // after reviewed document patient can download pdf from dashboard
            var econsentreviewdetails = Find(id);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var docName = Path.Combine(upload.DocumentUrl, econsentreviewdetails.Pdfpath);
            CustomParameter param = new CustomParameter();
            param.documentData = docName;
            param.fileName = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            return param;
        }

        public List<EconsentReviewDetailsDto> GetEconsentReviewDetailsForSubjectManagement(int patientid)
        {
            // use in subject management display documents patient wise
            //var EconsentReviewDetails = All.Where(x => x.DeletedDate == null && x.RandomizationId == patientid && x.IsReviewedByPatient == true && x.EconsentSetup.Roles.Any(t => t.RoleId == _jwtTokenAccesser.RoleId)).
            //       ProjectTo<EconsentReviewDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            var EconsentReviewDetails = All.Where(x => x.DeletedDate == null && x.RandomizationId == patientid && x.EconsentSetup.Roles.Any(t => t.RoleId == _jwtTokenAccesser.RoleId)).
                   ProjectTo<EconsentReviewDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return EconsentReviewDetails;
        }

        public List<EconsentDocumentDetailsDto> GetEconsentReviewDetailsForPatientDashboard()
        {
            // display ICF details in patient dashboard
            var randomization = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            if (randomization == null)
                return null;

            var result = All.Where(x => x.RandomizationId == randomization.Id && x.EconsentSetup.DeletedDate == null && x.EconsentSetup.LanguageId == randomization.LanguageId).
                  ProjectTo<EconsentDocumentDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            return result;
        }

        public int UpdateDocument(EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            // uptate method calls when patient review document
            var econsentReviewDetail = _mapper.Map<EconsentReviewDetails>(econsentReviewDetailsDto);
            var original = Find(econsentReviewDetail.Id);
            econsentReviewDetail.Patientapproveddatetime = _jwtTokenAccesser.GetClientDate();
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
            var docName = Guid.NewGuid().ToString() + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, FolderType.InformConcent.ToString(), docName);

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

            var outputname = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            var pdfpath = Path.Combine(FolderType.InformConcent.ToString(), "ReviewedPDF", outputname);
            string directorypath = Path.Combine(FolderType.InformConcent.ToString(), "ReviewedPDF");
            string[] paths = { upload.DocumentPath, FolderType.InformConcent.ToString(), "ReviewedPDF" };
            var fullPath = Path.Combine(paths);
            var outputFile = Path.Combine(upload.DocumentPath, pdfpath);           
            if (!Directory.Exists(fullPath))
                Directory.CreateDirectory(fullPath);
            FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);
            file.Dispose();
            file.Close();
            outputStream.Dispose();
            outputStream.Close();


            econsentReviewDetail.Pdfpath = pdfpath;
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
            var EconsentApprovedroles = _context.EconsentSetupRoles.Where(x => x.EconsentDocumentId == Econsentsetup.Id).Select(x => x.RoleId);
            var users = _context.ProjectRight.Where(x => x.ProjectId == randomization.ProjectId && EconsentApprovedroles.Contains(x.RoleId) && x.IsReviewDone == true).Select(x => x.UserId).Distinct();
            var usersdata = _context.Users.Where(x => users.Contains(x.Id) && x.DeletedDate == null).ToList();
            usersdata.ForEach(x =>
            {
                _emailSenderRespository.SendEmailOfPatientReviewedPDFtoInvestigator(x.Email, x.UserName, Econsentsetup.DocumentName, project.ProjectCode, randomization.Initial + " " + randomization.ScreeningNumber, outputFile);
            });
            return econsentReviewDetail.Id;
        }

        public int ApproveRejectEconsentDocument(EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            var generalSettings = _appSettingRepository.Get<GeneralSettingsDto>(_jwtTokenAccesser.CompanyId);
            generalSettings.TimeFormat = generalSettings.TimeFormat.Replace("a", "tt");
            // calls when investigator approve/reject document
            var econsentReviewDetails = Find(econsentReviewDetailsDto.Id);
            econsentReviewDetails.IsReviewDoneByInvestigator = true;
            econsentReviewDetails.InvestigatorRevieweddatetime = _jwtTokenAccesser.GetClientDate();
            econsentReviewDetails.ReviewDoneByRoleId = _jwtTokenAccesser.RoleId;
            econsentReviewDetails.ReviewDoneByUserId = _jwtTokenAccesser.UserId;
            econsentReviewDetails.IsApproved = econsentReviewDetailsDto.IsApproved;
            econsentReviewDetails.ApproveRejectReasonId = econsentReviewDetailsDto.ApproveRejectReasonId;
            econsentReviewDetails.ApproveRejectReasonOth = econsentReviewDetailsDto.ApproveRejectReasonOth;

            string filePath = string.Empty;

            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();

            if (econsentReviewDetails.Pdfpath != null)
            {
                //try
                //{
                string oldpdfpath = System.IO.Path.Combine(upload.DocumentPath, econsentReviewDetails?.Pdfpath);
                if (File.Exists(oldpdfpath))
                    System.IO.File.Delete(oldpdfpath);
                //}
                //catch (Exception ex)
                //{

                //}
            }
            var docName = Guid.NewGuid().ToString() + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, FolderType.InformConcent.ToString(), docName);

            Byte[] byteArray = Convert.FromBase64String(econsentReviewDetailsDto.DocumentData);
            Stream stream = new MemoryStream(byteArray);

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
            document.Save(fileStream, Syncfusion.DocIO.FormatType.Docx);
            document.Close();

            Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(fileStream, Syncfusion.DocIO.FormatType.Automatic);

            stream.Close();
            stream.Dispose();
            fileStream.Close();
            fileStream.Dispose();

            DocIORenderer render = new DocIORenderer();
            render.Settings.PreserveFormFields = true;
            PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);

            int pagecount = pdfDocument.Pages.Count;
            PdfGraphics graphics = pdfDocument.Pages[pagecount - 1].Graphics;
            PdfFont fontbold = new PdfStandardFont(PdfFontFamily.TimesRoman, 13, PdfFontStyle.Bold);
            PdfFont fontnormal = new PdfStandardFont(PdfFontFamily.TimesRoman, 13);

            //var userdetails = _context.Users.Where(x => x.Id == econsentReviewDetails.ReviewDoneByUserId && x.DeletedDate==null).Include(x=>x.UserRoles).SingleOrDefault();

            if (econsentReviewDetailsDto.IsApproved == true)
                graphics.DrawString("Approved By: ", fontbold, PdfBrushes.Black, new PointF(70, 300));
            else
                graphics.DrawString("Reject By: ", fontbold, PdfBrushes.Black, new PointF(70, 300));

            graphics.DrawString(_jwtTokenAccesser.UserName + "(" + _jwtTokenAccesser.RoleName + ")", fontnormal, PdfBrushes.Black, new PointF(70, 320));
            graphics.DrawString(Convert.ToDateTime(econsentReviewDetails.InvestigatorRevieweddatetime).ToString(generalSettings.DateFormat + ' ' + generalSettings.TimeFormat), fontnormal, PdfBrushes.Black, new PointF(70, 340));

            if (econsentReviewDetailsDto.IsApproved == true)
                graphics.DrawString("Approved Reason: ", fontbold, PdfBrushes.Black, new PointF(70, 360));
            else
                graphics.DrawString("Reject Reason: ", fontbold, PdfBrushes.Black, new PointF(70, 360));

            string reasonName = _context.AuditReason.Where(x => x.Id == econsentReviewDetailsDto.ApproveRejectReasonId).FirstOrDefault().ReasonName;
            graphics.DrawString(reasonName, fontnormal, PdfBrushes.Black, new PointF(175, 360));
            if (econsentReviewDetailsDto.ApproveRejectReasonOth != null && econsentReviewDetailsDto.ApproveRejectReasonOth != "")
            {
                if (econsentReviewDetailsDto.IsApproved == true)
                    graphics.DrawString("Approved Comment: ", fontbold, PdfBrushes.Black, new PointF(70, 380));
                else
                    graphics.DrawString("Reject Comment: ", fontbold, PdfBrushes.Black, new PointF(70, 380));

                graphics.DrawString(econsentReviewDetailsDto.ApproveRejectReasonOth, fontnormal, PdfBrushes.Black, new PointF(70, 400));
            }
            render.Dispose();
            wordDocument.Dispose();
            MemoryStream outputStream = new MemoryStream();
            pdfDocument.Save(outputStream);
            pdfDocument.Close();

            var outputname = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            var pdfpath = Path.Combine(FolderType.InformConcent.ToString(), "ReviewedPDF", outputname);
            var outputFile = Path.Combine(upload.DocumentPath, pdfpath);
            if (!Directory.Exists(outputFile)) Directory.CreateDirectory(Path.Combine(FolderType.InformConcent.ToString(), "ReviewedPDF"));
            FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);
            file.Close();
            file.Dispose();
            outputStream.Close();
            outputStream.Dispose();

            econsentReviewDetails.Pdfpath = pdfpath;

            Update(econsentReviewDetails);
            System.IO.File.Delete(filePath);

            _uow.Save();
            var Econsentsetup = _context.EconsentSetup.Where(x => x.Id == econsentReviewDetails.EconsentSetupId).ToList().FirstOrDefault();
            var project = _projectRepository.Find(Econsentsetup.ProjectId);
            var randomization = _context.Randomization.Where(x => x.Id == econsentReviewDetails.RandomizationId).ToList().FirstOrDefault();
            _emailSenderRespository.SendEmailOfInvestigatorApprovedPDFtoPatient(randomization.Email, randomization.Initial + " " + randomization.ScreeningNumber, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
            if (econsentReviewDetailsDto.IsApproved == true)
                _randomizationRepository.ChangeStatustoConsentCompleted(econsentReviewDetails.RandomizationId);
            else
            {
                var randomizationdata = _randomizationRepository.Find(econsentReviewDetails.RandomizationId);
                randomizationdata.PatientStatusId = ScreeningPatientStatus.Withdrawal;
                _randomizationRepository.Update(randomizationdata);
            }
            _uow.Save();
            return econsentReviewDetails.Id;
        }



    }
}
