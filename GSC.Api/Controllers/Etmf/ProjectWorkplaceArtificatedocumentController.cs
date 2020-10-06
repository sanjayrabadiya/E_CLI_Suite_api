using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.DocumentEditor;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;
using GSC.Api.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceArtificatedocumentController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly GscContext _context;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;
        public ProjectWorkplaceArtificatedocumentController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
              IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
              IUploadSettingRepository uploadSettingRepository,
              IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
               IJwtTokenAccesser jwtTokenAccesser,
               IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _uow = uow;
            _mapper = mapper;
            _eTMFWorkplaceRepository = eTMFWorkplaceRepository;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _context = uow.Context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
        }

        [Route("GetTreeview")]
        [HttpGet]
        public IActionResult GetTreeview()
        {
            var projectworkplace = _eTMFWorkplaceRepository.GetTreeview(1);
            return Ok(projectworkplace);
        }

        [Route("Get/{id}")]
        [HttpGet]
        public IActionResult Get(int id)
        {
            var reviewDocumentList = _projectWorkplaceArtificateDocumentReviewRepository.GetProjectArtificateDocumentReviewList();
            if (reviewDocumentList == null || reviewDocumentList.Count == 0) return Ok();

            var documentList = _projectWorkplaceArtificatedocumentRepository.FindByInclude(x => x.ProjectWorkplaceArtificateId == id && x.DeletedDate == null && reviewDocumentList.Any(c =>
                                                                                           c == x.Id), x => x.ProjectWorkplaceArtificate).ToList();

            List<CommonArtifactDocumentDto> dataList = new List<CommonArtifactDocumentDto>();
            foreach (var item in documentList)
            {
                var reviewerList = _projectWorkplaceArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId != item.CreatedBy).Select(z => z.UserId).Distinct().ToList();
                var users = new List<string>();
                reviewerList.ForEach(r =>
                {
                    var username = _userRepository.FindByInclude(x => x.Id == r).Select(y => y.UserName);
                    users.AddRange(username);
                });
                var Review = _projectWorkplaceArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId != _jwtTokenAccesser.UserId && x.DeletedDate == null).ToList();

                CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
                obj.Id = item.Id;
                obj.ProjectWorkplaceSubSectionArtifactId = item.ProjectWorkplaceArtificateId;
                obj.ProjectWorkplaceArtificateId = item.ProjectWorkplaceArtificateId;
                obj.Artificatename = _etmfArtificateMasterLbraryRepository.Find(item.ProjectWorkplaceArtificate.EtmfArtificateMasterLbraryId).ArtificateName;
                obj.DocumentName = item.DocumentName;
                obj.ExtendedName = item.DocumentName.Contains('_') ? item.DocumentName.Substring(0, item.DocumentName.LastIndexOf('_')) : item.DocumentName;
                obj.DocPath = System.IO.Path.Combine(_uploadSettingRepository.GetWebDocumentUrl(), FolderType.ProjectWorksplace.GetDescription(), item.DocPath, item.DocumentName);
                obj.CreatedByUser = _userRepository.Find((int)item.CreatedBy).UserName;
                obj.Reviewer = string.Join(", ", users);
                obj.CreatedDate = item.CreatedDate;
                obj.Version = item.Version;
                obj.StatusName = item.Status.GetDescription();
                obj.Status = item.Status;
                obj.Level = 6;
                obj.SendBy = !(item.CreatedBy == _jwtTokenAccesser.UserId);
                obj.ReviewStatus = Review.Count() == 0 ? "" : Review.All(z => z.IsSendBack) ? "Send Back" : "Send";
                obj.IsSendBack = _projectWorkplaceArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceArtificatedDocumentId == item.Id && x.UserId == _jwtTokenAccesser.UserId).Select(z => z.IsSendBack).LastOrDefault();
                dataList.Add(obj);

            }
            return Ok(dataList);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ProjectWorkplaceArtificatedocumentDto projectWorkplaceArtificatedocumentDto)
        {
            var Project = _projectRepository.Find(projectWorkplaceArtificatedocumentDto.ProjectId);
            var Projectname = Project.ProjectName + "-" + Project.ProjectCode;

            string filePath = string.Empty;
            string path = string.Empty;

            if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Country)

                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Country.GetDescription(),
                  projectWorkplaceArtificatedocumentDto.Countryname.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Site)
                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Site.GetDescription(),
                 projectWorkplaceArtificatedocumentDto.Sitename.Trim(), projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());
            else if (projectWorkplaceArtificatedocumentDto.FolderType == (int)WorkPlaceFolder.Trial)
                path = System.IO.Path.Combine(Projectname, WorkPlaceFolder.Trial.GetDescription(),
                   projectWorkplaceArtificatedocumentDto.Zonename.Trim(), projectWorkplaceArtificatedocumentDto.Sectionname.Trim(), projectWorkplaceArtificatedocumentDto.Artificatename.Trim());

            filePath = System.IO.Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), path);
            string FileName = DocumentService.SaveWorkplaceDocument(projectWorkplaceArtificatedocumentDto.FileModel, filePath, projectWorkplaceArtificatedocumentDto.FileName);

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectWorkplaceArtificatedocumentDto.Id = 0;
            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceArtificatedocument>(projectWorkplaceArtificatedocumentDto);
            projectWorkplaceArtificatedocument.DocumentName = FileName;
            projectWorkplaceArtificatedocument.DocPath = path;
            projectWorkplaceArtificatedocument.Status = ArtifactDocStatusType.Draft;
            projectWorkplaceArtificatedocument.Version = "1.0";

            _projectWorkplaceArtificatedocumentRepository.Add(projectWorkplaceArtificatedocument);
            if (_uow.Save() <= 0) throw new Exception("Creating Document failed on save.");

            _projectWorkplaceArtificateDocumentReviewRepository.SaveByDocumentIdInReview(projectWorkplaceArtificatedocument.Id);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument);
            return Ok(projectWorkplaceArtificatedocument.Id);

        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var subArtifactdoc = _projectWorkplaceArtificatedocumentRepository.FindByInclude(x => x.Id == id).FirstOrDefault();

            if (subArtifactdoc == null)
                return NotFound();
            _projectWorkplaceArtificatedocumentRepository.Delete(subArtifactdoc);
            _uow.Save();
            var aa = _projectWorkplaceArtificatedocumentRepository.deleteFile(id);
            return Ok(aa);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectWorkplaceArtificatedocumentDto projectWorkplaceArtificatedocumentDto)
        {
            if (projectWorkplaceArtificatedocumentDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            // projectWorkplaceArtificatedocument.Version = (double.Parse(projectWorkplaceArtificatedocumentDto.Version) + 1).ToString("0.0");
            // projectWorkplaceArtificatedocumentDto.Version = "a";
            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceArtificatedocument>(projectWorkplaceArtificatedocumentDto);

            _projectWorkplaceArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);

            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
            return Ok(projectWorkplaceArtificatedocument.Id);
        }

        [HttpPost]
        [Route("ImportData/{id}")]
        public IActionResult ImportData(int id)
        {
            var document = _projectWorkplaceArtificatedocumentRepository.Find(id);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, FolderType.ProjectWorksplace.GetDescription(), document.DocPath, document.DocumentName);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            string json = ImportWordDocument(stream);
            stream.Close();
            // return new HttpResponseMessage() { Content = new System.Net.Http.StringContent(json) };
            return Ok(json);
        }


        public string ImportWordDocument(Stream stream)
        {
            string sfdtText = "";
            EJ2WordDocument document = EJ2WordDocument.Load(stream, Syncfusion.EJ2.DocumentEditor.FormatType.Docx);
            sfdtText = Newtonsoft.Json.JsonConvert.SerializeObject(document);
            document.Dispose();
            return sfdtText;
        }

        [HttpPost]
        [Route("Save")]
        public IActionResult Save([FromBody] CustomParameter param)
        {
            string filePath = string.Empty;
            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(param.id);

            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var fileName = projectWorkplaceArtificatedocument.DocumentName.Contains('_') ? projectWorkplaceArtificatedocument.DocumentName.Substring(0, projectWorkplaceArtificatedocument.DocumentName.LastIndexOf('_')) : projectWorkplaceArtificatedocument.DocumentName;
            var docName = fileName + "_" + DateTime.Now.Ticks + ".docx";
            filePath = System.IO.Path.Combine(upload.DocumentPath, FolderType.ProjectWorksplace.GetDescription(), projectWorkplaceArtificatedocument.DocPath, docName);

            Byte[] byteArray = Convert.FromBase64String(param.documentData);
            Stream stream = new MemoryStream(byteArray);
            FormatType type = GetFormatTypeExport(filePath);

            FileStream fileStream = new FileStream(filePath, FileMode.OpenOrCreate, FileAccess.ReadWrite);

            if (type != FormatType.Docx)
            {
                Syncfusion.DocIO.DLS.WordDocument document = new Syncfusion.DocIO.DLS.WordDocument(stream, Syncfusion.DocIO.FormatType.Docx);
                document.Save(fileStream, GetDocIOFomatType(type));
                document.Close();
            }
            else
            {
                stream.Position = 0;
                stream.CopyTo(fileStream);
            }
            stream.Dispose();
            fileStream.Dispose();

            projectWorkplaceArtificatedocument.DocumentName = docName;
            _projectWorkplaceArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);
            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");

            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument);

            return Ok();
        }

        internal static Syncfusion.DocIO.FormatType GetDocIOFomatType(FormatType type)
        {
            switch (type)
            {
                case FormatType.Docx:
                    return (Syncfusion.DocIO.FormatType)FormatType.Docx;
                case FormatType.Doc:
                    return (Syncfusion.DocIO.FormatType)FormatType.Doc;
                case FormatType.Rtf:
                    return (Syncfusion.DocIO.FormatType)FormatType.Rtf;
                case FormatType.Txt:
                    return (Syncfusion.DocIO.FormatType)FormatType.Txt;
                case FormatType.WordML:
                    return (Syncfusion.DocIO.FormatType)FormatType.WordML;
                default:
                    throw new NotSupportedException("DocIO does not support this file format.");
            }
        }

        internal static FormatType GetFormatTypeExport(string fileName)
        {
            int index = fileName.LastIndexOf('.');
            string format = index > -1 && index < fileName.Length - 1 ? fileName.Substring(index + 1) : "";

            if (string.IsNullOrEmpty(format))
                throw new NotSupportedException("EJ2 Document editor does not support this file format.");
            switch (format.ToLower())
            {
                case "dotx":
                case "docx":
                case "docm":
                case "dotm":
                    return FormatType.Docx;
                case "dot":
                case "doc":
                    return FormatType.Doc;
                case "rtf":
                    return FormatType.Rtf;
                case "txt":
                    return FormatType.Txt;
                case "xml":
                    return FormatType.WordML;
                default:
                    throw new NotSupportedException("EJ2 Document editor does not support this file format.");
            }
        }

        [HttpPost]
        [Route("WordToPdf/{id}")]
        public IActionResult WordToPdf(int id)
        {
            var document = _projectWorkplaceArtificatedocumentRepository.Find(id);
            var filepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), document.DocPath, document.DocumentName);
            FileStream docStream = new FileStream(filepath, FileMode.Open, FileAccess.Read);
            Syncfusion.DocIO.DLS.WordDocument wordDocument = new Syncfusion.DocIO.DLS.WordDocument(docStream, Syncfusion.DocIO.FormatType.Automatic);
            DocIORenderer render = new DocIORenderer();
            render.Settings.PreserveFormFields = true;
            PdfDocument pdfDocument = render.ConvertToPDF(wordDocument);
            render.Dispose();
            wordDocument.Dispose();
            MemoryStream outputStream = new MemoryStream();
            pdfDocument.Save(outputStream);
            pdfDocument.Close();

            var outputname = document.DocumentName.Substring(0, document.DocumentName.LastIndexOf('_')) + "_" + DateTime.Now.Ticks + ".pdf";
            var outputFile = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.ProjectWorksplace.GetDescription(), document.DocPath, outputname);
            FileStream file = new FileStream(outputFile, FileMode.Create, FileAccess.Write);
            outputStream.WriteTo(file);

            document.DocumentName = outputname;
            _projectWorkplaceArtificatedocumentRepository.Update(document);
            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");

            return Ok();
        }
    }
}