﻿using AutoMapper;
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
using System.Text;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using GSC.Data.Dto.Etmf;
using Syncfusion.EJ2.DocumentEditor;
using System.Security.Cryptography.X509Certificates;
using GSC.Respository.UserMgt;
using System.Linq.Expressions;
using Microsoft.EntityFrameworkCore.Metadata.Conventions;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.InformConcent
{
    public class EconsentReviewDetailsRepository : GenericRespository<EconsentReviewDetails>, IEconsentReviewDetailsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        //private readonly Lazy<IRandomizationRepository> _noneRegisterRepository;
        private readonly IInvestigatorContactRepository _investigatorContactRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUserRepository _userRepository;
        public EconsentReviewDetailsRepository(IGSCContext context, 
                                                IJwtTokenAccesser jwtTokenAccesser,
                                                IEconsentSetupRepository econsentSetupRepository,
                                                //Lazy<IRandomizationRepository> noneRegisterRepository,
                                                IInvestigatorContactRepository investigatorContactRepository,
                                                IProjectRepository projectRepository,
                                                IUserRepository userRepository,
                                                IMapper mapper) : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentSetupRepository = econsentSetupRepository;
            _mapper = mapper;
           //_noneRegisterRepository = noneRegisterRepository;
            _investigatorContactRepository = investigatorContactRepository;
            _projectRepository = projectRepository;
            _userRepository = userRepository;
        }

        public string Duplicate(EconsentReviewDetailsDto objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.EconsentSetupId == objSave.EconsentSetupId && x.RandomizationId == objSave.RandomizationId && x.DeletedDate == null))
            {
                return "Already reviewed this document";
            }
            return "";
        }

        

        //public IList<DropDownDto> GetPatientDropdown(int projectid)
        //{
        //    //var econsentsetups = _econsentSetupRepository.All.Where(x => x.ProjectId == projectid).ToList();
        //    var data = (from econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == projectid)
        //                join EconsentReviewDetails in _context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsReviewDoneByInvestigator == false) on econsentsetups.Id equals EconsentReviewDetails.EconsentSetupId
        //                join nonregister in _context.Randomization.Where(x => x.DeletedDate == null && x.Id == 7) on EconsentReviewDetails.RandomizationId equals nonregister.Id //attendance.Id equals nonregister.AttendanceId
        //                //join attendance in _context.Attendance.Where(x => x.DeletedDate == null) on EconsentReviewDetails.AttendanceId equals attendance.Id

        //                select new DropDownDto
        //                {
        //                    Id = nonregister.Id,
        //                    Value = nonregister.Initial + " " + nonregister.ScreeningNumber
        //                }).Distinct().ToList();

        //    return data;
        //}

        //public List<EconsentReviewDetailsDto> GetUnApprovedEconsentDocumentList(int patientid)
        //{
        //    var EconsentReviewDetails = All.Where(x => x.DeletedDate == null && x.RandomizationId == patientid && x.IsReviewDoneByInvestigator == false).
        //           ProjectTo<EconsentReviewDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        //    EconsentReviewDetails.ForEach(b =>
        //    {
        //        b.EconsentDocumentName = _econsentSetupRepository.Find((int)b.EconsentSetupId).DocumentName;
        //    });
        //    return EconsentReviewDetails;
        //}

        //public List<EconsentReviewDetailsDto> GetApprovedEconsentDocumentList(int projectid)
        //{
        //    var data = (from econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == projectid)
        //                join EconsentReviewDetails in _context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsReviewDoneByInvestigator == true) on econsentsetups.Id equals EconsentReviewDetails.EconsentSetupId
        //                join nonregister in _context.Randomization.Where(x => x.DeletedDate == null) on EconsentReviewDetails.RandomizationId equals nonregister.Id//attendance.Id equals nonregister.AttendanceId
        //                //join attendance in _context.Attendance.Where(x => x.DeletedDate == null) on EconsentReviewDetails.AttendanceId equals attendance.Id

        //                select new EconsentReviewDetailsDto
        //                {
        //                    Id = EconsentReviewDetails.Id,
        //                    EconsentSetupId = EconsentReviewDetails.EconsentSetupId,
        //                    EconsentDocumentName = econsentsetups.DocumentName,
        //                    RandomizationName = nonregister.Initial + " " + nonregister.ScreeningNumber,
        //                    RandomizationId = EconsentReviewDetails.RandomizationId,
        //                    patientapproveddatetime = EconsentReviewDetails.patientapproveddatetime,
        //                    investigatorRevieweddatetime = EconsentReviewDetails.investigatorRevieweddatetime
        //                }).ToList();

        //    return data;
        //}

        public List<SectionsHeader> GetEconsentDocumentHeaders()
        {
            //var noneregister = _context.Randomization.Where(x => x.Id == patientId).ToList().FirstOrDefault();//_noneRegisterRepository.Find(patientId);
            var noneregister = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();//_noneRegisterRepository.Find(patientId);
            if (noneregister == null) return new List<SectionsHeader>();
            var studyId = _projectRepository.Find(noneregister.ProjectId).ParentProjectId;
            var econsentReviewDetails = FindBy(x => x.RandomizationId == noneregister.Id).ToList();
            var econsentpatientstatus = _context.EconsentSetupPatientStatus.Where(x => x.PatientStatusId == (int)noneregister.PatientStatusId && x.DeletedDate == null).Select(x => x.EconsentDocumentId);
            var Edocuments = _context.EconsentSetup.Where(x => x.ProjectId == (int)studyId && x.LanguageId == noneregister.LanguageId && x.DeletedDate == null && econsentpatientstatus.Contains(x.Id)).ToList();

            var Econsentdocuments = (from econsentsetups in Edocuments
                                     join doc in econsentReviewDetails on econsentsetups.Id equals doc.EconsentSetupId
                                     select new EConsentDocumentHeader
                                     {
                                         DocumentId = econsentsetups.Id,
                                         DocumentName = econsentsetups.DocumentName,
                                         DocumentPath = econsentsetups.DocumentPath,
                                         ReviewId = doc.Id,
                                         IsReviewed = doc.IsReviewedByPatient
                                     }).ToList();

            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            List<SectionsHeader> sectionsHeaders = new List<SectionsHeader>();
            int seqNo = 0;
            int documentid = 0;
            foreach (var document in Econsentdocuments)
            {
                var FullPath = System.IO.Path.Combine(upload.DocumentPath, document.DocumentPath);
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
                                sectionsHeader.documentId = document.DocumentId;
                                sectionsHeader.documentReviewId = document.ReviewId; 
                                sectionsHeader.documentName = document.DocumentName;
                                if (documentid != document.DocumentId)
                                {
                                    seqNo++;
                                }
                                documentid = document.DocumentId;
                                sectionsHeader.seqNo = seqNo;
                                sectionsHeader.isReadCompelete = document.IsReviewed;
                                sectionsHeader.isReviewed = document.IsReviewed;
                                sectionsHeaders.Add(sectionsHeader);
                                sectioncount++;
                            }
                        }
                    }
                }
            }
            return sectionsHeaders;
        }

        public List<SectionsHeader> GetEconsentDocumentHeadersByDocumentId(int documentId)
        {

            var Econsentdocument = _econsentSetupRepository.FindByInclude(x => x.Id == documentId).ToList().FirstOrDefault();
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
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var Econsentdocument = _econsentSetupRepository.Find(id);
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, Econsentdocument.DocumentPath);
            string path = FullPath;
            //string path = "C:\\Users\\Shree\\Documents\\ICF_English_A.N.Pharamcia-chlor.docx";
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            //string json = ImportWordDocument(stream);
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
            string jsonnew = JsonConvert.SerializeObject(jsonobj,Formatting.None, new JsonSerializerSettings
            {
                NullValueHandling = NullValueHandling.Ignore
            });
            //Syncfusion.DocIO.DLS.WordDocument documentold = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
            //Syncfusion.DocIO.DLS.WordDocument documentnew = new Syncfusion.DocIO.DLS.WordDocument();
            //documentnew.Sections.Clear();
            //documentnew.Sections.Add(documentold.Sections[sectionno].Clone());
            //string filePath = "E:\\Neel Doc";//System.IO.Path.Combine(upload.DocumentPath, document.DocPath);
            //string fileName = id + "_" + sectionno + "_" + DateTime.Now.ToFileTime().ToString() + ".docx"; //+ "_"  + document.DocumentName;
            //DirectoryInfo info = new DirectoryInfo(filePath);
            //if (!info.Exists)
            //{
            //    info.Create();
            //}
            //string pathnew = Path.Combine(filePath, fileName);
            //FileStream streamnew = new FileStream(pathnew, FileMode.Create);
            //documentnew.Save(streamnew, Syncfusion.DocIO.FormatType.Docx);
            //documentold.Close();
            //documentnew.Close();
            //streamnew.Position = 0;
            //streamnew.Close();
            //System.IO.File.Delete(pathnew);
            // return new HttpResponseMessage() { Content = new System.Net.Http.StringContent(json) };
            return jsonnew;
        }

        public string GetEconsentDocument(EconsentReviewDetailsDto econsentreviewdetails)
        {
            
            //var econsentreviewdetails = Find(id);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            //var ecosentdocId = econsentreviewdetails.EconsentDocumentId;
            var Econsentdocument = _econsentSetupRepository.Find(econsentreviewdetails.EconsentSetupId);
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, Econsentdocument.DocumentPath);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            //string json = ImportWordDocument(stream);
            string sfdtText = "";
            EJ2WordDocument wdocument = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(wdocument);
            wdocument.Dispose();
            string json = sfdtText;
            var jsonObj = JObject.Parse(json);
            string sign = File.ReadAllText("Config//signaturefooterblock.json");
            string sign2 = sign;
            int randomizationId;
            if (econsentreviewdetails.RandomizationId == 0)
            {
                randomizationId = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault().Id;
            } else
            {
                 randomizationId = econsentreviewdetails.RandomizationId;
            }
            var randomization = _context.Randomization.Where(x => x.Id == randomizationId).ToList().FirstOrDefault();//_noneRegisterRepository.Find(randomizationId);
            //string randomizationsignaturepath = System.IO.Path.Combine(upload.DocumentPath, randomization.SignaturePath);
            if (econsentreviewdetails.IsReviewedByPatient == true)
            {
                string randomizationsignaturepath = System.IO.Path.Combine(upload.DocumentPath, econsentreviewdetails.patientdigitalSignImagepath);
                string signRandombase64 = DocumentService.ConvertBase64Image(randomizationsignaturepath);
                sign = sign.Replace("_imagepath_", signRandombase64);
            } else
            {
                sign = sign.Replace("_imagepath_", econsentreviewdetails.patientdigitalSignBase64);
            }
            sign = sign.Replace("_volunterlabel_", "Volunteer");
            sign = sign.Replace("_voluntername_", randomization.ScreeningNumber + " " + randomization.Initial);
            sign = sign.Replace("_datetime_", (econsentreviewdetails.patientapproveddatetime == null) ? _jwtTokenAccesser.GetClientDate().ToString() : econsentreviewdetails.patientapproveddatetime.ToString());
            var jsonObj2 = JObject.Parse(sign);
            jsonObj.Merge(jsonObj2, new JsonMergeSettings
            {
                MergeArrayHandling = MergeArrayHandling.Union
            });

            if (econsentreviewdetails.IsReviewedByPatient == true)
            {
               var user = _userRepository.Find(_jwtTokenAccesser.UserId);
                //string investigatorsignaturepath = System.IO.Path.Combine(upload.DocumentPath, user.SignaturePath);
                string signinvestigatorbase64 = user.SignatureBase64String == null ? "" : user.SignatureBase64String;//DocumentService.ConvertBase64Image(investigatorsignaturepath);
                sign2 = sign2.Replace("_volunterlabel_", "Investigator");
                sign2 = sign2.Replace("_imagepath_", signinvestigatorbase64);
                sign2 = sign2.Replace("_voluntername_", user.UserName);
                sign2 = sign2.Replace("_datetime_", econsentreviewdetails.investigatorRevieweddatetime == null ? _jwtTokenAccesser.GetClientDate().ToString() : econsentreviewdetails.investigatorRevieweddatetime.ToString());
                var jsonObj3 = JObject.Parse(sign2);
                jsonObj.Merge(jsonObj3, new JsonMergeSettings
                {
                    MergeArrayHandling = MergeArrayHandling.Union
                });
            }

            json = jsonObj.ToString();
            stream.Close();
            return json;
        }

        public List<DashboardDto> GetEconsentMyTaskList(int ProjectId)
        {
            var result = (//from project in _context.Project.Where(x => x.Id == ProjectId)
                          //join childproject in _context.Project.Where(x => x.ParentProjectId != null) on project.Id equals childproject.ParentProjectId
                          from econsentsetups in _context.EconsentSetup.Where(x => x.ProjectId == ProjectId && x.DeletedDate == null)
                         join EconsentReviewDetails in _context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsReviewedByPatient == true && x.IsReviewDoneByInvestigator == false) on econsentsetups.Id equals EconsentReviewDetails.EconsentSetupId
                         join nonregister in _context.Randomization.Where(x => x.DeletedDate == null) on EconsentReviewDetails.RandomizationId equals nonregister.Id//attendance.Id equals nonregister.AttendanceId
                                                                                                                                                                 //join attendance in _context.Attendance.Where(x => x.DeletedDate == null) on EconsentReviewDetails.AttendanceId equals attendance.Id

             select new DashboardDto
             {
                 Id = EconsentReviewDetails.Id,
                 TaskInformation = econsentsetups.DocumentName + " for " + nonregister.Initial + " " + nonregister.ScreeningNumber + " is Pending approve from your side",
                 ExtraData = EconsentReviewDetails.Id
             }).ToList();

            return result;
        }

        public void downloadpdf(CustomParameter param)
        {
            
            //File(memory, GetMimeTypes()[ext], Path.GetFileName(path));
            //return File(new FileStream(file, FileMode.Open), "text/plain");
            //Response.AppendHeader("content-disposition", "attachment; filename=" + name);
            //return File(memory, GetContentType(path), Path.GetFileName(path));

        }

        public List<EconsentReviewDetailsDto> GetEconsentReviewDetailsForSubjectManagement(int patientid)
        {
            var EconsentReviewDetails = All.Where(x => x.DeletedDate == null && x.RandomizationId == patientid && x.IsReviewedByPatient == true && x.EconsentSetup.Roles.Any(t => t.RoleId == _jwtTokenAccesser.RoleId)).
                   ProjectTo<EconsentReviewDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return EconsentReviewDetails;
        }

        public List<EconsentReviewDetailsDto> GetEconsentReviewDetailsForPatientDashboard()
        {
            var randomization = _context.Randomization.Where(x => x.UserId == _jwtTokenAccesser.UserId).FirstOrDefault();
            if (randomization == null)
                return new List<EconsentReviewDetailsDto>();
            var studyid = _projectRepository.Find(randomization.ProjectId).ParentProjectId;
            var econsentReviewDetails = FindBy(x => x.RandomizationId == randomization.Id).ToList();
            var Edocuments = _context.EconsentSetup.Where(x => x.ProjectId == (int)studyid && x.LanguageId == randomization.LanguageId && x.DeletedDate == null).ToList();
            //var EconsentReviewDetails = All.Where(x => x.DeletedDate == null && x.RandomizationId == randomizationId).
            //       ProjectTo<EconsentReviewDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            var EconsentReviewDetails = (from econsentsetups in Edocuments
                                         join doc in econsentReviewDetails on econsentsetups.Id equals doc.EconsentSetupId into ps
                                     from p in ps.DefaultIfEmpty()
                                     select new EconsentReviewDetailsDto
                                     {
                                         Id = (p == null) ? 0 : p.Id,
                                         EconsentDocumentName = econsentsetups.DocumentName,
                                         IsReviewedByPatient = (p == null) ? false : p.IsReviewedByPatient,
                                         IsReviewDoneByInvestigator = (p == null) ? false : p.IsReviewDoneByInvestigator
                                     }).ToList();
            return EconsentReviewDetails;
        }
    }
}
