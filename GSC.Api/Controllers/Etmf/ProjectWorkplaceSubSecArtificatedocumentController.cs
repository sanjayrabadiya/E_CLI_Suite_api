using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Etmf;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceSubSecArtificatedocumentController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        private readonly IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        private readonly IProjectSubSecArtificateDocumentApproverRepository _projectSubSecArtificateDocumentApproverRepository;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;
        public ProjectWorkplaceSubSecArtificatedocumentController(IUnitOfWork uow,
            IMapper mapper,
            IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
            IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository,
            IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository,
            IGSCContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
        }


        [Route("Get/{id}")]
        [HttpGet]
        public IActionResult Get(int id)
        {
            var result = _projectWorkplaceSubSecArtificatedocumentRepository.GetSubSecDocumentList(id);
            return Ok(result);
        }

        [Route("GetDocument/{id}")]
        [HttpGet]
        public IActionResult GetDocument(int id)
        {
            var result = _projectWorkplaceSubSecArtificatedocumentRepository.GetDocument(id);
            return Ok(result);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectWorkplaceSubSecArtificatedocumentDto projectWorkplaceArtificatedocumentDto)
        {
            var projectWorkplaceArtificatedocument = _projectWorkplaceSubSecArtificatedocumentRepository.AddDocument(projectWorkplaceArtificatedocumentDto);
            _projectWorkplaceSubSecArtificatedocumentRepository.Add(projectWorkplaceArtificatedocument);
            if (_uow.Save() <= 0) throw new Exception("Creating Document failed on save.");

            _projectSubSecArtificateDocumentReviewRepository.SaveByDocumentIdInReview(projectWorkplaceArtificatedocument.Id);
            _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, null);
            return Ok(projectWorkplaceArtificatedocument.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var subArtifactdoc = _projectWorkplaceSubSecArtificatedocumentRepository.FindByInclude(x => x.Id == id).FirstOrDefault();

            if (subArtifactdoc == null)
                return NotFound();
            _projectWorkplaceSubSecArtificatedocumentRepository.Delete(subArtifactdoc);
            _uow.Save();
            var aa = _projectWorkplaceSubSecArtificatedocumentRepository.deleteSubsectionArtifactfile(id);
            return Ok(aa);
        }

        [HttpPut]
        [Route("UpdateVersion/{id}")]
        public IActionResult UpdateVersion(int id)
        {
            var projectSubSecArtificatedocumentDto = _projectWorkplaceSubSecArtificatedocumentRepository.Find(id);
            projectSubSecArtificatedocumentDto.Version = (double.Parse(projectSubSecArtificatedocumentDto.Version) + 1).ToString("0.0");

            var ProjectSubSecArtificatedocument = _mapper.Map<ProjectWorkplaceSubSecArtificatedocument>(projectSubSecArtificatedocumentDto);
            _projectWorkplaceSubSecArtificatedocumentRepository.Update(ProjectSubSecArtificatedocument);

            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
            return Ok(ProjectSubSecArtificatedocument.Id);
        }

        [HttpPost]
        [Route("ImportData/{id}")]
        public IActionResult ImportData(int id)
        {
            var result = _projectWorkplaceSubSecArtificatedocumentRepository.ImportData(id);
            return Ok(result);
        }

        [Route("GetArtificateDocumentApproverHistory/{Id}")]
        [HttpGet]
        public IActionResult GetArtificateDocumentApproverHistory(int Id)
        {
            var History = _projectSubSecArtificateDocumentApproverRepository.GetArtificateDocumentApproverHistory(Id);
            return Ok(History);
        }

        [Route("GetArtificateDocumentHistory/{Id}")]
        [HttpGet]
        public IActionResult GetArtificateDocumentHistory(int Id)
        {
            var History = _projectSubSecArtificateDocumentReviewRepository.GetArtificateDocumentHistory(Id);
            return Ok(History);
        }

        [HttpPost]
        [Route("Save")]
        public IActionResult Save([FromBody] CustomParameter param)
        {
            var projectWorkplaceArtificatedocument = _projectWorkplaceSubSecArtificatedocumentRepository.Find(param.id);
            var docName = _projectWorkplaceSubSecArtificatedocumentRepository.SaveDocumentInFolder(projectWorkplaceArtificatedocument, param);

            projectWorkplaceArtificatedocument.DocumentName = docName;
            _projectWorkplaceSubSecArtificatedocumentRepository.Update(projectWorkplaceArtificatedocument);
            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");

            if (!param.AddHistory)
                _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, null);

            return Ok();
        }

        [HttpPost]
        [Route("WordToPdf/{id}")]
        public IActionResult WordToPdf(int id)
        {
            var document = _projectWorkplaceSubSecArtificatedocumentRepository.WordToPdf(id);

            _projectWorkplaceSubSecArtificatedocumentRepository.Update(document);
            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
            return Ok();
        }

        [HttpPut]
        [Route("UpdateSupersede/{id}")]
        public IActionResult UpdateSupersede(int id)
        {
            var projectWorkplaceSubSecArtificatedocumentDto = _projectWorkplaceSubSecArtificatedocumentRepository.Find(id);
            projectWorkplaceSubSecArtificatedocumentDto.Status = ArtifactDocStatusType.Supersede;

            var projectWorkplaceSubSecArtificatedocument = _mapper.Map<ProjectWorkplaceSubSecArtificatedocument>(projectWorkplaceSubSecArtificatedocumentDto);
            _projectWorkplaceSubSecArtificatedocumentRepository.Update(projectWorkplaceSubSecArtificatedocument);

            var childDoc = _context.ProjectWorkplaceSubSecArtificatedocument.Where(x => x.ParentDocumentId == id && x.DeletedDate == null).ToList();
            foreach (var obj in childDoc)
            {
                obj.Version = (double.Parse(projectWorkplaceSubSecArtificatedocumentDto.Version) + 1).ToString("0.0");
                _projectWorkplaceSubSecArtificatedocumentRepository.Update(obj);
            }

            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
            return Ok(projectWorkplaceSubSecArtificatedocument.Id);
        }

        [HttpPost]
        [Route("GetDocumentForHistory/{id}")]
        public IActionResult GetDocumentForHistory(int id)
        {
            var result = _projectWorkplaceSubSecArtificatedocumentRepository.GetDocumentHistory(id);
            return Ok(result);
        }

        [HttpPost]
        [Route("GetDocumentForPdfHistory/{id}")]
        public IActionResult GetDocumentForPdfHistory(int id)
        {
            var result = _projectWorkplaceSubSecArtificatedocumentRepository.GetDocumentForPdfHistory(id);
            return Ok(result);
        }
    }
}