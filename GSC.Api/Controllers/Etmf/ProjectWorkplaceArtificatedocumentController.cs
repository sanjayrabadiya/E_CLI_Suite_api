using AutoMapper;
using DocumentFormat.OpenXml.Spreadsheet;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.Generic;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceArtificatedocumentController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        public ProjectWorkplaceArtificatedocumentController(IUnitOfWork uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectWorkplaceArtificateRepository projectWorkplaceArtificateRepository,
            IGSCContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _eTMFWorkplaceRepository = eTMFWorkplaceRepository;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _projectWorkplaceArtificateRepository = projectWorkplaceArtificateRepository;
        }

        [Route("GetTreeview")]
        [HttpGet]
        public IActionResult GetTreeview()
        {
            var EtmfProjectWorkPlace = _eTMFWorkplaceRepository.GetTreeview(1, null);
            return Ok(EtmfProjectWorkPlace);
        }

        [Route("GetDocumentList/{id}")]
        [HttpGet]
        public IActionResult GetDocumentList(int id)
        {
            var result = _projectWorkplaceArtificatedocumentRepository.GetDocumentList(id);
            return Ok(result);
        }

        [Route("GetDocument/{id}")]
        [HttpGet]
        public IActionResult GetDocument(int id)
        {
            var result = _projectWorkplaceArtificatedocumentRepository.GetDocument(id);
            return Ok(result);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ProjectWorkplaceArtificatedocumentDto projectWorkplaceArtificatedocumentDto)
        {
            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.AddDocument(projectWorkplaceArtificatedocumentDto);

            _projectWorkplaceArtificatedocumentRepository.Add(projectWorkplaceArtificatedocument);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Document failed on save.");
                return BadRequest(ModelState);
            }

            _projectWorkplaceArtificateDocumentReviewRepository.SaveByDocumentIdInReview(projectWorkplaceArtificatedocument.Id);
            _projectArtificateDocumentApproverRepository.SaveByDocumentIdInApprove(projectWorkplaceArtificatedocument.Id);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, null);
            return Ok(projectWorkplaceArtificatedocument.Id);
        }

        [HttpPost, DisableRequestSizeLimit]
        [Route("SaveBulkDocument")]
        [TransactionRequired]
        public async Task<ActionResult> SaveBulkDocument([FromBody] List<BulkDocumentUploadModel> bulkDocuments)
        {
            var t = System.Threading.Tasks.Task.Run(() =>
            {
                foreach (var document in bulkDocuments)
                {
                    var etmfProjectArtifactList = _projectWorkplaceArtificateRepository.All
                        .Where(x => x.ProjectId == document.ProjectId && x.ArtifactCodeName == document.ArtifactCodeName && x.DeletedDate == null && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate)
                        .Include(x => x.EtmfArtificateMasterLbrary)
                        .Include(x => x.ProjectWorkPlace.EtmfMasterLibrary)
                        .Include(x => x.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary)
                        .Include(x => x.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfUserPermission)
                        .Where(x => x.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.EtmfUserPermission.Any(x => x.UserId == _jwtTokenAccesser.UserId && x.IsAdd)).ToList();

                    foreach (var etmfProjectArtifact in etmfProjectArtifactList)
                    {
                        if (etmfProjectArtifact != null)
                        {
                            var fileModel = new FileModel()
                            {
                                Base64 = document.Base64,
                                Extension = document.Extension
                            };

                            var index1 = document.FileName.LastIndexOf('-');
                            string fileName = "";
                            if (index1 >= 0)
                            {
                                fileName = document.FileName.Substring(0, index1);
                            }
                            var projectWorkplaceArtificatedocumentDto = new ProjectWorkplaceArtificatedocumentDto()
                            {
                                ProjectWorkplaceArtificateId = etmfProjectArtifact.Id,
                                FileModel = fileModel,
                                FileName = $"{fileName}.{document.Extension.Trim()}",
                                ProjectId = document.ProjectId,
                                SuperSede = false,
                                Countryname = etmfProjectArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName,
                                Zonename = etmfProjectArtifact.ProjectWorkPlace.ProjectWorkPlace.EtmfMasterLibrary.ZonName,
                                Sectionname = etmfProjectArtifact.ProjectWorkPlace.EtmfMasterLibrary.SectionName,
                                Artificatename = etmfProjectArtifact.EtmfArtificateMasterLbrary.ArtificateName,
                                FolderType = etmfProjectArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.WorkPlaceFolderId,
                                Sitename = etmfProjectArtifact.ProjectWorkPlace.ProjectWorkPlace.ProjectWorkPlace.ItemName
                            };

                            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.AddDocument(projectWorkplaceArtificatedocumentDto);
                            _projectWorkplaceArtificatedocumentRepository.Add(projectWorkplaceArtificatedocument);

                            if (_uow.Save() <= 0) throw new Exception("Creating Document failed on save.");

                            _projectWorkplaceArtificateDocumentReviewRepository.SaveByDocumentIdInReview(projectWorkplaceArtificatedocument.Id);
                            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, null);
                        }
                    }
                }
            });

            await t;

            return Ok(1);
        }

        [HttpGet]
        [Route("CheckDocumentName/{projectId}")]
        public ActionResult CheckDocumentName(int projectId)
        {
            var etmfProjectArtifact = _projectWorkplaceArtificateRepository.All
                   .Where(x => x.ProjectId == projectId && x.ArtifactCodeName != null && x.DeletedDate == null && x.TableTag == (int)EtmfTableNameTag.ProjectWorkPlaceArtificate)
                   .Select(s => s.ArtifactCodeName).ToList();

            return Ok(etmfProjectArtifact);

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
        [Route("UpdateVersion/{id}")]
        public IActionResult UpdateVersion(int id)
        {
            var projectWorkplaceArtificatedocumentDto = _projectWorkplaceArtificatedocumentRepository.Find(id);
            projectWorkplaceArtificatedocumentDto.Version = (double.Parse(projectWorkplaceArtificatedocumentDto.Version) + 1).ToString("0.0");

            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceArtificatedocument>(projectWorkplaceArtificatedocumentDto);
            _projectWorkplaceArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Document failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(projectWorkplaceArtificatedocument.Id);
        }

        [HttpPost]
        [Route("ImportData/{id}")]
        public IActionResult ImportData(int id)
        {
            var result = _projectWorkplaceArtificatedocumentRepository.ImportData(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("Save")]
        public IActionResult Save([FromBody] CustomParameter param)
        {
            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(param.id);
            var docName = _projectWorkplaceArtificatedocumentRepository.SaveDocumentInFolder(projectWorkplaceArtificatedocument, param);

            projectWorkplaceArtificatedocument.DocumentName = docName;
            _projectWorkplaceArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Document failed on save.");
                return BadRequest(ModelState);
            }

            if (!param.AddHistory)
                _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, null);

            return Ok();
        }

        [HttpPost]
        [Route("WordToPdf/{id}")]
        public IActionResult WordToPdf(int id)
        {
            var document = _projectWorkplaceArtificatedocumentRepository.WordToPdf(id);

            _projectWorkplaceArtificatedocumentRepository.Update(document);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Document failed on save.");
                return BadRequest(ModelState);
            }

            return Ok();
        }

        [Route("GetArtificateDocumentHistory/{Id}")]
        [HttpGet]
        public IActionResult GetArtificateDocumentHistory(int Id)
        {
            var History = _projectWorkplaceArtificateDocumentReviewRepository.GetArtificateDocumentHistory(Id);
            return Ok(History);
        }

        [HttpPut]
        [Route("UpdateSupersede/{id}")]
        public IActionResult UpdateSupersede(int id)
        {
            var projectWorkplaceArtificatedocumentDto = _projectWorkplaceArtificatedocumentRepository.Find(id);
            projectWorkplaceArtificatedocumentDto.Status = ArtifactDocStatusType.Supersede;

            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceArtificatedocument>(projectWorkplaceArtificatedocumentDto);
            _projectWorkplaceArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);

            var childDoc = _context.ProjectWorkplaceArtificatedocument.Where(x => x.ParentDocumentId == id && x.DeletedDate == null).ToList();
            foreach (var obj in childDoc)
            {
                obj.Version = (double.Parse(projectWorkplaceArtificatedocumentDto.Version) + 1).ToString("0.0");
                _projectWorkplaceArtificatedocumentRepository.Update(obj);
            }

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Document failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(projectWorkplaceArtificatedocument.Id);
        }

        [Route("GetArtificateDocumentApproverHistory/{Id}")]
        [HttpGet]
        public IActionResult GetArtificateDocumentApproverHistory(int Id)
        {
            var History = _projectArtificateDocumentApproverRepository.GetArtificateDocumentApproverHistory(Id);
            return Ok(History);
        }

        [HttpPost]
        [Route("DocumentMove")]
        public IActionResult DocumentMove([FromBody] List<WorkplaceFolderDto> workplaceFolderDto)
        {
            ProjectWorkplaceArtificatedocument firstSaved = null;

            for (var i = 0; i <= (workplaceFolderDto.Count - 1); i++)
            {
                var document = _projectWorkplaceArtificatedocumentRepository.AddMovedDocument(workplaceFolderDto[i]);
                var ProjectArtificate = _projectWorkplaceArtificateRepository.All.First(x => x.Id == workplaceFolderDto[i].ProjectWorkplaceArtificateId);
                ProjectArtificate.ParentArtificateId = document.ProjectWorkplaceArtificateId;

                document.Id = 0;
                document.ProjectWorkplaceArtificateId = workplaceFolderDto[i].ProjectWorkplaceArtificateId;

                document.ProjectArtificateDocumentReview.Select(x => { x.Id = 0; return x; }).ToList();
                document.ProjectArtificateDocumentApprover.Select(x => { x.Id = 0; return x; }).ToList();
                document.ProjectArtificateDocumentComment.Select(x => { x.Id = 0; return x; }).ToList();
                document.ProjectArtificateDocumentHistory = null;

                document.IsMoved = true;
                _projectWorkplaceArtificatedocumentRepository.Add(document);
                _context.ProjectArtificateDocumentReview.AddRange(document.ProjectArtificateDocumentReview);
                _context.ProjectArtificateDocumentApprover.AddRange(document.ProjectArtificateDocumentApprover);
                _context.ProjectArtificateDocumentComment.AddRange(document.ProjectArtificateDocumentComment);
                _projectWorkplaceArtificateRepository.Update(ProjectArtificate);
                if (i == 0) firstSaved = document;
            }
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating move document failed on save.");
                return BadRequest(ModelState);
            }

            return Ok(firstSaved?.Id);
        }

        [HttpPost]
        [Route("GetDocumentForHistory/{id}")]
        public IActionResult GetDocumentForHistory(int id)
        {
            var history = _projectArtificateDocumentHistoryRepository.Find(id);
            var document = _projectWorkplaceArtificatedocumentRepository.Find(history.ProjectWorkplaceArtificateDocumentId);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).First();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, history.DocumentName);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            string json = _projectWorkplaceArtificatedocumentRepository.ImportWordDocument(stream, path);
            stream.Close();
            return Ok(json);
        }

        [Route("GetEtmfZoneDropdown/{projectId}")]
        [HttpGet]
        public IActionResult GetEtmfZoneDropdown(int projectId)
        {
            var data = _projectWorkplaceArtificatedocumentRepository.GetEtmfZoneDropdown(projectId);
            return Ok(data);
        }

        [Route("GetEtmfSectionDropdown/{zoneId}")]
        [HttpGet]
        public IActionResult GetEtmfSectionDropdown(int zoneId)
        {
            var data = _projectWorkplaceArtificatedocumentRepository.GetEtmfSectionDropdown(zoneId);
            return Ok(data);
        }

        [Route("GetEtmfArtificateDropdown/{sectionId}")]
        [HttpGet]
        public IActionResult GetEtmfArtificateDropdown(int sectionId)
        {
            var data = _projectWorkplaceArtificatedocumentRepository.GetEtmfArtificateDropdown(sectionId);
            return Ok(data);
        }

        [Route("GetEtmfSubSectionDropdown/{sectionId}")]
        [HttpGet]
        public IActionResult GetEtmfSubSectionDropdown(int sectionId)
        {
            var data = _projectWorkplaceArtificatedocumentRepository.GetEtmfSubSectionDropdown(sectionId);
            return Ok(data);
        }

        [Route("GetEtmfSubSectionArtifactDropdown/{subSectionArtifactId}")]
        [HttpGet]
        public IActionResult GetEtmfSubSectionArtifactDropdown(int subSectionArtifactId)
        {
            var data = _projectWorkplaceArtificatedocumentRepository.GetEtmfSubSectionArtificateDropdown(subSectionArtifactId);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetEtmfAuditLogReport")]
        public IActionResult GetEtmfAuditLogReport([FromQuery] EtmfAuditLogReportSearchDto filters)
        {
            if (filters.projectId <= 0) return BadRequest();

            var auditsDto = _projectWorkplaceArtificatedocumentRepository.GetEtmfAuditLogReport(filters);

            return Ok(auditsDto);
        }

        [HttpPost]
        [Route("GetDocumentForPdfHistory/{id}")]
        public IActionResult GetDocumentForPdfHistory(int id)
        {
            CommonArtifactDocumentDto obj = new CommonArtifactDocumentDto();
            var history = _projectArtificateDocumentHistoryRepository.Find(id);
            var document = _projectWorkplaceArtificatedocumentRepository.Find(history.ProjectWorkplaceArtificateDocumentId);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).First();
            var FullPath = Path.Combine(upload.DocumentUrl, _jwtTokenAccesser.CompanyId.ToString(), document.DocPath, history.DocumentName);
            obj.FullDocPath = FullPath;
            return Ok(obj);
        }

        [Route("GetEtmfCountrySiteDropdown/{projectId}/{folderId}")]
        [HttpGet]
        public IActionResult GetEtmfCountrySiteDropdown(int projectId, int folderId)
        {
            var data = _projectWorkplaceArtificatedocumentRepository.GetEtmfCountrySiteDropdown(projectId, folderId);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetEtmfStudyReport")]
        public IActionResult GetEtmfStudyReport([FromQuery] StudyReportSearchDto filters)
        {
            if (filters.projectId <= 0) return BadRequest();

            var auditsDto = _projectWorkplaceArtificatedocumentRepository.GetEtmfStudyReport(filters);

            return Ok(auditsDto);
        }

        [HttpGet]
        [Route("UpdateDocumentComment/{documentId}/{isComment}")]
        public IActionResult UpdateDocumentComment(int documentId, bool? isComment)
        {
            _projectWorkplaceArtificatedocumentRepository.UpdateDocumentComment(documentId, isComment);
            return Ok("");
        }

        [HttpPost]
        [Route("AddDocumentExpiryDate")]
        public IActionResult AddDocumentExpiryDate([FromBody] DocumentExpiryModel expiryDate)
        {
            var document = _projectWorkplaceArtificatedocumentRepository.Find(expiryDate.id);
            document.ExpiryDate = expiryDate.ExpiryDate;
            _projectWorkplaceArtificatedocumentRepository.Update(document);
            _projectArtificateDocumentHistoryRepository.AddHistory(document, null, null);
            _context.Save();
            return Ok(1);
        }


        [HttpGet]
        [Route("GetDcoumentHistory/{documentId}")]
        public IActionResult GetDcoumentHistory(int documentId)
        {
            var docHistory = _projectWorkplaceArtificatedocumentRepository.GetDocumentHistory(documentId).OrderByDescending(q => q.ExpiryDate);
            return Ok(docHistory);
        }

        [HttpGet]
        [Route("DownloadDocument/{id}")]
        public IActionResult DownloadDocument(int id)
        {
            var file = _projectWorkplaceArtificatedocumentRepository.DownloadDocument(id);
            if (file.FileBytes == null)
            {
                return BadRequest();
            }
            else
            {
                return File(file.FileBytes, file.MIMEType, file.DocumentName);
            }
        }
    }
}