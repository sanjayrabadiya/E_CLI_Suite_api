using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.InformConcent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentReviewDetailsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IRandomizationRepository _noneRegisterRepository;
        private readonly GscContext _context;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        public EconsentReviewDetailsController(IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IEconsentSetupRepository econsentSetupRepository,
            IRandomizationRepository noneRegisterRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _econsentSetupRepository = econsentSetupRepository;
            _noneRegisterRepository = noneRegisterRepository;
            _context = uow.Context;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
        }

        [HttpGet]
        [Route("GetEconsentDocumentHeaders/{patientId}")]
        public IActionResult GetEconsentDocumentHeaders(int patientId)
        {
            var noneregister = _noneRegisterRepository.Find(patientId);
            var languageId = 7;//noneregister.LanguageId;
            var ProjectId = 4;//noneregister.ProjectId;
            var Econsentdocuments = _econsentSetupRepository.FindByInclude(x => x.ProjectId == ProjectId && x.LanguageId == languageId && x.DeletedBy == null && x.DeletedDate == null).ToList();
            var econsentReviewDetails = _econsentReviewDetailsRepository.FindByInclude(x => x.AttendanceId == patientId).ToList();
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
            return Ok(sectionsHeaders);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ImportSectionData/{id}/{sectionno}")]
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

        [HttpPost]
        public IActionResult Post([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var econsentReviewDetail = _mapper.Map<EconsentReviewDetails>(econsentReviewDetailsDto);
            econsentReviewDetail.patientapproveddatetime = DateTime.Now;
            var validate = _econsentReviewDetailsRepository.Duplicate(econsentReviewDetailsDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _econsentReviewDetailsRepository.Add(econsentReviewDetail);
            
            if (_uow.Save() <= 0) throw new Exception("Creating Econsent review insert failed on save.");

            return Ok(econsentReviewDetail.Id);
        }

        [HttpGet]
        [Route("GetPatientDropdown/{projectid}")]
        public IActionResult GetPatientDropdown(int projectid)
        {
            return Ok(_econsentReviewDetailsRepository.GetPatientDropdown(projectid));
        }

        [HttpGet]
        [Route("GetUnApprovedEconsentDocumentList/{patientid}")]
        public IActionResult GetUnApprovedEconsentDocumentList(int patientid)
        {
            return Ok(_econsentReviewDetailsRepository.GetUnApprovedEconsentDocumentList(patientid));
        }

        [HttpGet]
        [Route("GetApprovedEconsentDocumentList/{projectid}")]
        public IActionResult GetApprovedEconsentDocumentList(int projectid)
        {
            return Ok(_econsentReviewDetailsRepository.GetApprovedEconsentDocumentList(projectid));
        }

        [HttpPost]
        [Route("GetEconsentDocument/{id}")]
        public IActionResult GetEconsentDocument(int id)
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
            return Ok(json);
        }

        [HttpPut]
        [Route("ApproveEconsentDocument/{id}")]
        public IActionResult ApproveEconsentDocument(int id)
        {
            if (id <= 0) return BadRequest();

            var econsentReviewDetails = _econsentReviewDetailsRepository.Find(id);
            econsentReviewDetails.IsApprovedByInvestigator = true;
            econsentReviewDetails.investigatorapproveddatetime = DateTime.Now;
            
            _econsentReviewDetailsRepository.Update(econsentReviewDetails);

            if (_uow.Save() <= 0) throw new Exception("Approving failed");
            return Ok(econsentReviewDetails.Id);
        }


    }
}
