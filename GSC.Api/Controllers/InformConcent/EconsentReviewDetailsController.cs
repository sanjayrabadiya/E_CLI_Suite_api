using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentReviewDetailsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly GscContext _context;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IProjectRepository _projectRepository;
        private readonly IInvestigatorContactRepository _investigatorContactRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public EconsentReviewDetailsController(IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IUploadSettingRepository uploadSettingRepository,
            IEmailSenderRespository emailSenderRespository,
            IProjectRepository projectRepository,
            IInvestigatorContactRepository investigatorContactRepository,
            IRandomizationRepository randomizationRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _mapper = mapper;
            _context = uow.Context;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _emailSenderRespository = emailSenderRespository;
            _projectRepository = projectRepository;
            _investigatorContactRepository = investigatorContactRepository;
            _randomizationRepository = randomizationRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            
        }

        [HttpGet]
        [Route("GetEconsentDocumentHeaders")]
        public IActionResult GetEconsentDocumentHeaders()
        {
            var sectionsHeaders = _econsentReviewDetailsRepository.GetEconsentDocumentHeaders();
            return Ok(sectionsHeaders);
        }

        [HttpGet]
        [Route("GetEconsentDocumentHeadersByDocumentId/{documentId}")]
        public IActionResult GetEconsentDocumentHeadersByDocumentId(int documentId)
        {
            var sectionsHeaders = _econsentReviewDetailsRepository.GetEconsentDocumentHeadersByDocumentId(documentId);
            return Ok(sectionsHeaders);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ImportSectionData/{id}/{sectionno}")]
        public string ImportSectionData(int id, int sectionno)
        {
            var jsonnew = _econsentReviewDetailsRepository.ImportSectionData(id, sectionno);
            return jsonnew;
        }

        [HttpPost]
        public IActionResult Post([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var randomizationId = _randomizationRepository.FindBy(x => x.UserId == _jwtTokenAccesser.UserId).ToList().FirstOrDefault().Id;
            econsentReviewDetailsDto.AttendanceId = randomizationId;
            var econsentReviewDetail = _mapper.Map<EconsentReviewDetails>(econsentReviewDetailsDto);
            econsentReviewDetail.patientapproveddatetime = DateTime.Now;
            var validate = _econsentReviewDetailsRepository.Duplicate(econsentReviewDetailsDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            if (econsentReviewDetailsDto.patientdigitalSignBase64?.Length > 0)
            {
                FileModel fileModel = new FileModel();
                fileModel.Base64 = econsentReviewDetailsDto.patientdigitalSignBase64;
                fileModel.Extension = "png";
                econsentReviewDetail.patientdigitalSignImagepath = new ImageService().ImageSave(fileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.InformConcent);
            }

            string filePath = string.Empty;

            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var docName = Guid.NewGuid().ToString() + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, FolderType.InformConcent.ToString(), docName);

            Byte[] byteArray = Convert.FromBase64String(econsentReviewDetailsDto.documentData);
            Stream stream = new MemoryStream(byteArray);

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
            document.Save(fileStream, Syncfusion.DocIO.FormatType.Docx);
            document.Close();

            Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(fileStream, Syncfusion.DocIO.FormatType.Automatic);

            stream.Dispose();
            fileStream.Dispose();

            DocIORenderer render = new DocIORenderer();
            render.Settings.PreserveFormFields = true;
            PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);
            render.Dispose();
            wordDocument.Dispose();
            MemoryStream outputStream = new MemoryStream();
            pdfDocument.Save(outputStream);
            pdfDocument.Close();

            var outputname = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            var pdfpath = Path.Combine(FolderType.InformConcent.ToString(),"ReviewedPDF", outputname);
            var outputFile = Path.Combine(upload.DocumentPath, pdfpath);
            if (!Directory.Exists(outputFile)) Directory.CreateDirectory(Path.Combine(FolderType.InformConcent.ToString(), "ReviewedPDF"));
            FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);
            file.Dispose();
            outputStream.Dispose();

            econsentReviewDetail.pdfpath = pdfpath;
            econsentReviewDetail.IsReviewedByPatient = true;
            _econsentReviewDetailsRepository.Add(econsentReviewDetail);

            System.IO.File.Delete(filePath);
            if (_uow.Save() <= 0) throw new Exception("Creating Econsent review insert failed on save.");
            var Econsentsetup = _context.EconsentSetup.Where(x => x.Id == econsentReviewDetail.EconsentDocumentId).ToList().FirstOrDefault();
            var project = _projectRepository.Find(Econsentsetup.ProjectId);
            var investigator = _investigatorContactRepository.Find((int)project.InvestigatorContactId);
            var randomization = _context.Randomization.Where(x => x.Id == econsentReviewDetail.AttendanceId).ToList().FirstOrDefault();
            _emailSenderRespository.SendEmailOfPatientReviewedPDFtoPatient(randomization.Email,randomization.Initial + " " + randomization.ScreeningNumber, Econsentsetup.DocumentName,project.ProjectCode,outputFile);
            _emailSenderRespository.SendEmailOfPatientReviewedPDFtoInvestigator(investigator.EmailOfInvestigator,investigator.NameOfInvestigator,Econsentsetup.DocumentName,project.ProjectCode, randomization.Initial + " " + randomization.ScreeningNumber,outputFile);
            return Ok(econsentReviewDetail.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
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

            if (econsentReviewDetailsDto.patientdigitalSignBase64?.Length > 0)
            {
                FileModel fileModel = new FileModel();
                fileModel.Base64 = econsentReviewDetailsDto.patientdigitalSignBase64;
                fileModel.Extension = "png";
                econsentReviewDetail.patientdigitalSignImagepath = new ImageService().ImageSave(fileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.InformConcent);
            }

            string filePath = string.Empty;

            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var docName = Guid.NewGuid().ToString() + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, FolderType.InformConcent.ToString(), docName);

            Byte[] byteArray = Convert.FromBase64String(econsentReviewDetailsDto.documentData);
            Stream stream = new MemoryStream(byteArray);

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
            document.Save(fileStream, Syncfusion.DocIO.FormatType.Docx);
            document.Close();

            Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(fileStream, Syncfusion.DocIO.FormatType.Automatic);

            stream.Dispose();
            fileStream.Dispose();

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
            var outputFile = Path.Combine(upload.DocumentPath, pdfpath);
            if (!Directory.Exists(outputFile)) Directory.CreateDirectory(Path.Combine(FolderType.InformConcent.ToString(), "ReviewedPDF"));
            FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);
            file.Dispose();
            outputStream.Dispose();

            econsentReviewDetail.pdfpath = pdfpath;
            econsentReviewDetail.IsReviewedByPatient = true;
            _econsentReviewDetailsRepository.Update(econsentReviewDetail);

            System.IO.File.Delete(filePath);
            if (_uow.Save() <= 0) throw new Exception("Creating Econsent review insert failed on save.");
            var Econsentsetup = _context.EconsentSetup.Where(x => x.Id == econsentReviewDetail.EconsentDocumentId).ToList().FirstOrDefault();
            var project = _projectRepository.Find(Econsentsetup.ProjectId);
            var investigator = _investigatorContactRepository.Find((int)project.InvestigatorContactId);
            var randomization = _context.Randomization.Where(x => x.Id == econsentReviewDetail.AttendanceId).ToList().FirstOrDefault();
            _emailSenderRespository.SendEmailOfPatientReviewedPDFtoPatient(randomization.Email, randomization.Initial + " " + randomization.ScreeningNumber, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
            _emailSenderRespository.SendEmailOfPatientReviewedPDFtoInvestigator(investigator.EmailOfInvestigator, investigator.NameOfInvestigator, Econsentsetup.DocumentName, project.ProjectCode, randomization.Initial + " " + randomization.ScreeningNumber, outputFile);

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
        [Route("GetEconsentDocument")]
        public IActionResult GetEconsentDocument([FromBody]EconsentReviewDetailsDto econsentreviewdetails)
        {
            var json = _econsentReviewDetailsRepository.GetEconsentDocument(econsentreviewdetails);
            return Ok(json);
        }

        [HttpPut]
        [Route("ApproveEconsentDocument")]
        public IActionResult ApproveEconsentDocument([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            if (econsentReviewDetailsDto.Id <= 0) return BadRequest();

            var econsentReviewDetails = _econsentReviewDetailsRepository.Find(econsentReviewDetailsDto.Id);
            econsentReviewDetails.IsApprovedByInvestigator = true;
            econsentReviewDetails.investigatorapproveddatetime = DateTime.Now;
            
            string filePath = string.Empty;

            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();

            try
            {
                if (econsentReviewDetails.pdfpath != null)
                {
                    string oldpdfpath = System.IO.Path.Combine(upload.DocumentPath, econsentReviewDetails?.pdfpath);
                    System.IO.File.Delete(oldpdfpath);
                }
            }
            catch(Exception ex)
            {
            }
            
            

            var docName = Guid.NewGuid().ToString() + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, FolderType.InformConcent.ToString(), docName);

            Byte[] byteArray = Convert.FromBase64String(econsentReviewDetailsDto.documentData);
            Stream stream = new MemoryStream(byteArray);

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);
            Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
            document.Save(fileStream, Syncfusion.DocIO.FormatType.Docx);
            document.Close();

            Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(fileStream, Syncfusion.DocIO.FormatType.Automatic);

            stream.Dispose();
            fileStream.Dispose();

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
            var outputFile = Path.Combine(upload.DocumentPath, pdfpath);
            if (!Directory.Exists(outputFile)) Directory.CreateDirectory(Path.Combine(FolderType.InformConcent.ToString(), "ReviewedPDF"));
            FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);
            file.Dispose();
            outputStream.Dispose();

            econsentReviewDetails.pdfpath = pdfpath;

            _econsentReviewDetailsRepository.Update(econsentReviewDetails);
            System.IO.File.Delete(filePath);
            if (_uow.Save() <= 0) throw new Exception("Approving failed");

            var Econsentsetup = _context.EconsentSetup.Where(x => x.Id == econsentReviewDetails.EconsentDocumentId).ToList().FirstOrDefault();
            var project = _projectRepository.Find(Econsentsetup.ProjectId);
            var randomization = _context.Randomization.Where(x => x.Id == econsentReviewDetails.AttendanceId).ToList().FirstOrDefault();
            _emailSenderRespository.SendEmailOfInvestigatorApprovedPDFtoPatient(randomization.Email, randomization.Initial + " " + randomization.ScreeningNumber, Econsentsetup.DocumentName, project.ProjectCode, outputFile);
            _randomizationRepository.ChangeStatustoConsentCompleted(econsentReviewDetails.AttendanceId);
            return Ok(econsentReviewDetails.Id);
        }

        

        [HttpPost]
        [Route("downloadpdf/{id}")]
        public  IActionResult downloadpdf(int id)
        {
            var econsentreviewdetails = _econsentReviewDetailsRepository.Find(id);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var docName = Path.Combine(upload.DocumentUrl, econsentreviewdetails.pdfpath);
            CustomParameter param = new CustomParameter();
            param.documentData = docName;//"https://dev2.clinvigilant.com/Documents/Project/6d79f9fb-92e6-49c1-9837-2811d2b8e52f.pdf";//docName
            param.fileName = Guid.NewGuid().ToString() + "_" + DateTime.Now.Ticks + ".pdf";
            return Ok(param);
            //return Ok();

        }

    }
}
