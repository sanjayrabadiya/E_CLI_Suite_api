using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
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
    public class EconsentReviewDetailsRepository : GenericRespository<EconsentReviewDetails, GscContext>, IEconsentReviewDetailsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IMapper _mapper;
        private readonly IRandomizationRepository _noneRegisterRepository;
        private readonly GscContext _context;
        public EconsentReviewDetailsRepository(IUnitOfWork<GscContext> uow, 
                                                IJwtTokenAccesser jwtTokenAccesser,
                                                IEconsentSetupRepository econsentSetupRepository,
                                                IRandomizationRepository noneRegisterRepository,
                                                IMapper mapper) : base(uow, jwtTokenAccesser)
        {
            _uow = uow;
            _context = uow.Context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _econsentSetupRepository = econsentSetupRepository;
            _mapper = mapper;
            _noneRegisterRepository = noneRegisterRepository;
        }

        public string Duplicate(EconsentReviewDetailsDto objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.EconsentDocumentId == objSave.EconsentDocumentId && x.AttendanceId == objSave.AttendanceId && x.DeletedDate == null))
            {
                return "Already reviewed this document";
            }
            return "";
        }

        

        public IList<DropDownDto> GetPatientDropdown(int projectid)
        {
            //var econsentsetups = _econsentSetupRepository.All.Where(x => x.ProjectId == projectid).ToList();
            var data = (from econsentsetups in Context.EconsentSetup.Where(x => x.ProjectId == projectid)
                        join EconsentReviewDetails in Context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsApprovedByInvestigator == false) on econsentsetups.Id equals EconsentReviewDetails.EconsentDocumentId
                        join nonregister in Context.Randomization.Where(x => x.DeletedDate == null && x.Id == 1) on EconsentReviewDetails.AttendanceId equals nonregister.Id //attendance.Id equals nonregister.AttendanceId
                        //join attendance in Context.Attendance.Where(x => x.DeletedDate == null) on EconsentReviewDetails.AttendanceId equals attendance.Id

                        select new DropDownDto
                        {
                            Id = nonregister.Id,
                            Value = nonregister.Initial + " " + nonregister.ScreeningNumber
                        }).Distinct().ToList();

            return data;
        }

        public List<EconsentReviewDetailsDto> GetUnApprovedEconsentDocumentList(int patientid)
        {
            var EconsentReviewDetails = All.Where(x => x.DeletedDate == null && x.AttendanceId == patientid && x.IsApprovedByInvestigator == false).
                   ProjectTo<EconsentReviewDetailsDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            EconsentReviewDetails.ForEach(b =>
            {
                b.EconsentDocumentName = _econsentSetupRepository.Find((int)b.EconsentDocumentId).DocumentName;
            });
            return EconsentReviewDetails;
        }

        public List<EconsentReviewDetailsDto> GetApprovedEconsentDocumentList(int projectid)
        {
            var data = (from econsentsetups in Context.EconsentSetup.Where(x => x.ProjectId == projectid)
                        join EconsentReviewDetails in Context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsApprovedByInvestigator == true) on econsentsetups.Id equals EconsentReviewDetails.EconsentDocumentId
                        join nonregister in Context.Randomization.Where(x => x.DeletedDate == null && x.Id == 1) on EconsentReviewDetails.AttendanceId equals nonregister.Id//attendance.Id equals nonregister.AttendanceId
                        //join attendance in Context.Attendance.Where(x => x.DeletedDate == null) on EconsentReviewDetails.AttendanceId equals attendance.Id

                        select new EconsentReviewDetailsDto
                        {
                            Id = EconsentReviewDetails.Id,
                            EconsentDocumentId = EconsentReviewDetails.EconsentDocumentId,
                            EconsentDocumentName = econsentsetups.DocumentName,
                            AttendanceName = nonregister.Initial + " " + nonregister.ScreeningNumber,
                            AttendanceId = EconsentReviewDetails.AttendanceId,
                            patientapproveddatetime = EconsentReviewDetails.patientapproveddatetime,
                            investigatorapproveddatetime = EconsentReviewDetails.investigatorapproveddatetime
                        }).ToList();

            return data;
        }

        public List<SectionsHeader> GetEconsentDocumentHeaders(int patientId)
        {

            var noneregister = _noneRegisterRepository.Find(patientId);
            var languageId = noneregister.LanguageId;
            var ProjectId = noneregister.ProjectId;
            var Econsentdocuments = _econsentSetupRepository.FindByInclude(x => x.ProjectId == ProjectId && x.LanguageId == languageId && x.DeletedBy == null && x.DeletedDate == null).ToList();
            var econsentReviewDetails = FindByInclude(x => x.AttendanceId == patientId).ToList();
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
                    bool isReadcompelete = false;
                    if (econsentReviewDetails.Where(x => x.EconsentDocumentId == document.Id).ToList().Count > 0)
                    {
                        isReadcompelete = true;
                    }
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
                                sectionsHeader.documentId = document.Id;
                                sectionsHeader.documentName = document.DocumentName;
                                if (documentid != document.Id)
                                {
                                    seqNo++;
                                }
                                documentid = document.Id;
                                sectionsHeader.seqNo = seqNo;
                                sectionsHeader.isReadCompelete = isReadcompelete;
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

            var Econsentdocument = _econsentSetupRepository.FindByInclude(x => x.Id == documentId && x.DeletedBy == null && x.DeletedDate == null).ToList().FirstOrDefault();
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


            string jsonnew = JsonConvert.SerializeObject(jsonobj);
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

        public string GetEconsentDocument(int id)
        {
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var Econsentdocument = _econsentSetupRepository.Find(id);
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
            stream.Close();
            return json;
        }

        public List<DashboardDto> GetEconsentMyTaskList(int ProjectId)
        {
            var result = (from project in Context.Project.Where(x => x.Id == ProjectId)
                          join childproject in Context.Project.Where(x => x.ParentProjectId != null) on project.Id equals childproject.ParentProjectId
                          join econsentsetups in Context.EconsentSetup on childproject.Id equals econsentsetups.ProjectId
                         join EconsentReviewDetails in Context.EconsentReviewDetails.Where(x => x.DeletedDate == null && x.IsApprovedByInvestigator == false) on econsentsetups.Id equals EconsentReviewDetails.EconsentDocumentId
                         join nonregister in Context.Randomization.Where(x => x.DeletedDate == null && x.Id == 1) on EconsentReviewDetails.AttendanceId equals nonregister.Id//attendance.Id equals nonregister.AttendanceId
                                                                                                                                                                 //join attendance in Context.Attendance.Where(x => x.DeletedDate == null) on EconsentReviewDetails.AttendanceId equals attendance.Id

             select new DashboardDto
             {
                 Id = EconsentReviewDetails.Id,
                 TaskInformation = econsentsetups.DocumentName + " for " + nonregister.Initial + " " + nonregister.ScreeningNumber + " is Pending approve from your side",
                 ExtraData = EconsentReviewDetails.Id
             }).ToList();

            return result;
        }
    }
}
