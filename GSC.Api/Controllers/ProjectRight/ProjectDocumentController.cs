﻿using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.ProjectRight;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.ProjectRight;
using Microsoft.AspNetCore.Mvc;
using GSC.Shared.JWTAuth;

namespace GSC.Api.Controllers.ProjectRight
{
    [Route("api/[controller]")]
    public class ProjectDocumentController : BaseController
    {
        private readonly IProjectDocumentReviewRepository _documentReviewRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDocumentRepository _projectDocumentRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectRightRepository _projectRightRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;


        public ProjectDocumentController(IProjectDocumentRepository projectDocumentRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IUploadSettingRepository uploadSettingRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectDocumentReviewRepository documentReviewRepository,
            IProjectRightRepository projectRightRepository,
            IProjectRepository projectRepository
        )
        {
            _projectDocumentRepository = projectDocumentRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _documentReviewRepository = documentReviewRepository;
            _projectRightRepository = projectRightRepository;
            _projectRepository = projectRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var projectDocuments = _projectDocumentRepository.GetDocument(id);
            projectDocuments.ForEach(t => t.PathName = documentUrl + t.PathName);

            if (projectDocuments.Count > 0)
                foreach (var item in projectDocuments)
                {
                    var projectCreatedBy = _projectRepository.FindByInclude(project => project.Id == item.ProjectId).FirstOrDefault();
                    var isExists = _documentReviewRepository.FindByInclude(t => t.ProjectDocumentId == item.Id && t.IsReview && t.UserId != projectCreatedBy.CreatedBy);
                    if (isExists.Any()) item.IsReview = true; else item.IsReview = false;

                    //Add study code in access training grid *Create Date : 14092020 *Create By: Vipul
                    item.SiteCode = projectCreatedBy.ParentProjectId != null ? projectCreatedBy.ProjectCode : "";
                    item.StudyCode = projectCreatedBy.ParentProjectId != null ? _projectRepository.Find((int)projectCreatedBy.ParentProjectId).ProjectCode : projectCreatedBy.ProjectCode;
                }
            return Ok(projectDocuments);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ProjectDocumentDto projectDocumentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

              
            projectDocumentDto.Id = 0;
            //set file path and extension
            if (projectDocumentDto.FileModel?.Base64?.Length > 0)
            {
                var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit(projectDocumentDto.ProjectId);
                if (!string.IsNullOrEmpty(validateuploadlimit))
                {
                    ModelState.AddModelError("Message", validateuploadlimit);
                    return BadRequest(ModelState);
                }
                projectDocumentDto.PathName = DocumentService.SaveUploadDocument(projectDocumentDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(projectDocumentDto.ProjectId), FolderType.TraningDocument, "");
                projectDocumentDto.MimeType = projectDocumentDto.FileModel.Extension;
            }

            var projectDocument = _mapper.Map<ProjectDocument>(projectDocumentDto);

            /* Check for duplicate document name */
            var validate = _projectDocumentRepository.Duplicate(projectDocument);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            projectDocument.ModifiedBy = _jwtTokenAccesser.UserId;
            projectDocument.ModifiedDate = _jwtTokenAccesser.GetClientDate();
            _projectDocumentRepository.Add(projectDocument);

            if (_uow.Save() <= 0) return Ok(new Exception("Creating project document failed on save."));

            _documentReviewRepository.SaveByDocumentId(projectDocument.Id, projectDocument.ProjectId);
            _projectRightRepository.UpdateIsReviewDone(projectDocumentDto.ProjectId);
            return Ok(projectDocument.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDocumentDto projectDocumentDto)
        {
            if (projectDocumentDto.Id <= 0) return BadRequest();


            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //added by bhargav kheni for if document send empty if they cant want to change docuemnt
            var document = _projectDocumentRepository.Find(projectDocumentDto.Id);
            document.FileName = projectDocumentDto.FileName;
            if (projectDocumentDto.FileModel?.Base64?.Length > 0)
            {
                var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit(projectDocumentDto.ProjectId);
                if (!string.IsNullOrEmpty(validateuploadlimit))
                {
                    ModelState.AddModelError("Message", validateuploadlimit);
                    return BadRequest(ModelState);
                }
                DocumentService.RemoveFile(_uploadSettingRepository.GetDocumentPath(), document.PathName);
                document.PathName = DocumentService.SaveUploadDocument(projectDocumentDto.FileModel,
                    _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(projectDocumentDto.ProjectId), FolderType.TraningDocument, "");
                document.MimeType = projectDocumentDto.FileModel.Extension;
                document.FileName = projectDocumentDto.FileName;
            }

            /* Added by Vipul for effective Date on 16-10-2019 */
            var validate = _projectDocumentRepository.Duplicate(document);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectDocumentRepository.Update(document);

            _uow.Save();
            _projectRightRepository.UpdateIsReviewDone(projectDocumentDto.ProjectId);
            return Ok(document.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDocumentRepository.Find(id);

            if (record == null)
                return NotFound();
            DocumentService.RemoveFile(_uploadSettingRepository.GetDocumentPath(), record.PathName);
            _projectDocumentRepository.Delete(record);
            _documentReviewRepository.DeleteByDocumentId(id, record.ProjectId);

            _uow.Save();

            _projectRightRepository.UpdateIsReviewDone(record.ProjectId);

            return Ok();
        }
    }
}