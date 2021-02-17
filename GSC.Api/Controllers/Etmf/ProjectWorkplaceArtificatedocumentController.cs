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
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Mvc;
using Syncfusion.EJ2.DocumentEditor;
using GSC.Api.Helpers;
using Newtonsoft.Json.Linq;
using Newtonsoft.Json;
using Syncfusion.DocIORenderer;
using Syncfusion.Pdf;
using Microsoft.EntityFrameworkCore;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
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
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IProjectWorkplaceArtificateRepository _projectWorkplaceArtificateRepository;
        public ProjectWorkplaceArtificatedocumentController(IUnitOfWork uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IUploadSettingRepository uploadSettingRepository,
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
            _uploadSettingRepository = uploadSettingRepository;
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
            var projectworkplace = _eTMFWorkplaceRepository.GetTreeview(1);
            return Ok(projectworkplace);
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
            if (_uow.Save() <= 0) throw new Exception("Creating Document failed on save.");

            _projectWorkplaceArtificateDocumentReviewRepository.SaveByDocumentIdInReview(projectWorkplaceArtificatedocument.Id);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, null);
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
        [Route("UpdateVersion/{id}")]
        public IActionResult UpdateVersion(int id)
        {
            var projectWorkplaceArtificatedocumentDto = _projectWorkplaceArtificatedocumentRepository.Find(id);
            projectWorkplaceArtificatedocumentDto.Version = (double.Parse(projectWorkplaceArtificatedocumentDto.Version) + 1).ToString("0.0");

            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceArtificatedocument>(projectWorkplaceArtificatedocumentDto);
            _projectWorkplaceArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);

            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
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
            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");

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
            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");

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

            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
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
                var ProjectArtificate = _projectWorkplaceArtificateRepository.All.Where(x => x.Id == workplaceFolderDto[i].ProjectWorkplaceArtificateId).FirstOrDefault();
                ProjectArtificate.ParentArtificateId = document.ProjectWorkplaceArtificateId;

                document.Id = 0;
                document.ProjectWorkplaceArtificateId = workplaceFolderDto[i].ProjectWorkplaceArtificateId;

                document.ProjectArtificateDocumentReview.Select(x => { x.Id = 0; return x; }).ToList();
                document.ProjectArtificateDocumentApprover.Select(x => { x.Id = 0; return x; }).ToList();
                document.ProjectArtificateDocumentComment.Select(x => { x.Id = 0; return x; }).ToList();
                document.ProjectArtificateDocumentHistory = null;

                _projectWorkplaceArtificatedocumentRepository.Add(document);
                _projectWorkplaceArtificateRepository.Update(ProjectArtificate);
                if (i == 0) firstSaved = document;
            }
            if (_uow.Save() <= 0) throw new Exception("Creating move document failed on save.");

            return Ok(firstSaved.Id);
        }

        [HttpPost]
        [Route("GetDocumentForHistory/{id}")]
        public IActionResult GetDocumentForHistory(int id)
        {
            var history = _projectArtificateDocumentHistoryRepository.Find(id);
            var document = _projectWorkplaceArtificatedocumentRepository.Find(history.ProjectWorkplaceArtificateDocumentId);
            var upload = _context.UploadSetting.OrderByDescending(x => x.Id).FirstOrDefault();
            var FullPath = System.IO.Path.Combine(upload.DocumentPath, FolderType.ProjectWorksplace.GetDescription(), document.DocPath, history.DocumentName);
            string path = FullPath;
            if (!System.IO.File.Exists(path))
                return null;
            Stream stream = System.IO.File.OpenRead(path);
            string json = _projectWorkplaceArtificatedocumentRepository.ImportWordDocument(stream, path);
            stream.Close();
            return Ok(json);
        }

        [Route("GetEtmfZoneDropdown")]
        [HttpGet]
        public IActionResult GetEtmfZoneDropdown()
        {
            var data = _projectWorkplaceArtificatedocumentRepository.GetEtmfZoneDropdown();
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

        [HttpGet]
        [Route("GetEtmfAuditLogReport")]
        public IActionResult GetEtmfAuditLogReport([FromQuery] EtmfAuditLogReportSearchDto filters)
        {
            if (filters.projectId <= 0) return BadRequest();

            var auditsDto = _projectWorkplaceArtificatedocumentRepository.GetEtmfAuditLogReport(filters);

            return Ok(auditsDto);
        }

        [HttpPut]
        [Route("UpdateNotRequired/{id}")]
        public IActionResult UpdateNotRequired(int id)
        {
            var projectWorkplaceArtificatedocumentDto = _projectWorkplaceArtificatedocumentRepository.Find(id);
            if (projectWorkplaceArtificatedocumentDto.IsNotRequired)
            {
                projectWorkplaceArtificatedocumentDto.Status = ArtifactDocStatusType.Draft;
                projectWorkplaceArtificatedocumentDto.IsNotRequired = !projectWorkplaceArtificatedocumentDto.IsNotRequired;
            }
            else
            {
                projectWorkplaceArtificatedocumentDto.Status = ArtifactDocStatusType.NotRequired;
                projectWorkplaceArtificatedocumentDto.IsNotRequired = !projectWorkplaceArtificatedocumentDto.IsNotRequired;
            }
            var projectWorkplaceArtificatedocument = _mapper.Map<ProjectWorkplaceArtificatedocument>(projectWorkplaceArtificatedocumentDto);
            _projectWorkplaceArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);

            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
            return Ok(projectWorkplaceArtificatedocument.Id);
        }
    }
}