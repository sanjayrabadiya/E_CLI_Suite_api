﻿using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.ProjectRight;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.ProjectRight;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.ProjectRight
{
    [Route("api/[controller]")]
    public class ProjectDocumentController : BaseController
    {
        private readonly IProjectDocumentReviewRepository _documentReviewRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDesignRepository _projectDesignRepository;
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
            IProjectRepository projectRepository,
            IProjectDesignRepository projectDesignRepository
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
            _projectDesignRepository = projectDesignRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            //var childProjectIds = _projectRepository.GetChildProjectDropDown(id);

          //  List<ProjectDocumentDto> termsList = new List<ProjectDocumentDto>();

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var projectDocuments = _projectDocumentRepository.GetDocument(id);
            projectDocuments.ForEach(t => t.PathName = documentUrl + t.PathName);
            //projectDocuments.ForEach(t => t.IsReview = false);

            if (projectDocuments.Count() > 0)
                foreach (var item in projectDocuments)
                {
                    var projectCreatedBy = _projectRepository.FindByInclude(project => project.Id == item.ProjectId).FirstOrDefault();
                    var isExists = _documentReviewRepository.FindByInclude(t => t.ProjectDocumentId == item.Id && t.IsReview && t.UserId != projectCreatedBy.CreatedBy);
                    if (isExists.Count() > 0) item.IsReview = true; else item.IsReview = false;
                }

            return Ok(projectDocuments);

            #region Comment
            //if (childProjectIds.Count != 0)
            //{
            //    for (var i = 0; i < childProjectIds.Count; i++)
            //    {
            //        var i1 = i;

            //        projectDocuments = _projectDocumentRepository.GetDocument(childProjectIds[i1].Id);

            //        //if (i1 < 1)
            //        {

            //            projectDocuments.ForEach(t => t.PathName = documentUrl + t.PathName);
            //            projectDocuments.ForEach(t => t.IsReview = false);


            //            if (projectDocuments.Count() > 0)
            //                foreach (var item in projectDocuments)
            //                {
            //                    var projectCreatedBy = _projectRepository.FindByInclude(project => project.Id == item.ProjectId)
            //                        .FirstOrDefault();
            //                    var isExists = _documentReviewRepository.FindByInclude(t =>
            //                        t.ProjectDocumentId == item.Id && t.IsReview && t.UserId != projectCreatedBy.CreatedBy);
            //                    if (isExists.Count() > 0) item.IsReview = true;
            //                }
            //        }
            //        termsList.AddRange(projectDocuments);


            //    }
            //    return Ok(termsList);
            //}
            //else
            //{
            //    var projectDocument = _projectDocumentRepository.GetDocument(id);

            //    projectDocument.ForEach(t => t.PathName = documentUrl + t.PathName);
            //    projectDocument.ForEach(t => t.IsReview = false);

            //    if (projectDocument.Count() > 0)
            //        foreach (var item in projectDocument)
            //        {
            //            var projectCreatedBy = _projectRepository.FindByInclude(project => project.Id == item.ProjectId)
            //                .FirstOrDefault();
            //            var isExists = _documentReviewRepository.FindByInclude(t =>
            //                t.ProjectDocumentId == item.Id && t.IsReview && t.UserId != projectCreatedBy.CreatedBy);
            //            if (isExists.Count() > 0) item.IsReview = true;
            //        }

            //    return Ok(projectDocument);
            //}
            //return Ok(id);
            #endregion
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectDocumentDto projectDocumentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //Check for project design complete or not
            var projectDesign = new GSC.Data.Entities.Project.Design.ProjectDesign();
            var parentProject = _projectRepository.GetParentProjectId(projectDocumentDto.ProjectId);
            if (parentProject != null)
                projectDesign = _projectDesignRepository.FindBy(t => t.ProjectId == parentProject && t.DeletedDate == null).FirstOrDefault();
            else
                projectDesign = _projectDesignRepository.FindBy(t => t.ProjectId == projectDocumentDto.ProjectId && t.DeletedDate == null).FirstOrDefault();
            if (projectDesign == null || !projectDesign.IsCompleteDesign)
            {
                ModelState.AddModelError("Message", "Design not completed for this project");
                return BadRequest(ModelState);
            }


            projectDocumentDto.Id = 0;
            //set file path and extension
            if (projectDocumentDto.FileModel?.Base64?.Length > 0)
            {
                projectDocumentDto.PathName = DocumentService.SaveProjectDocument(projectDocumentDto.FileModel,_uploadSettingRepository.GetDocumentPath(), FolderType.Project);
                projectDocumentDto.MimeType = projectDocumentDto.FileModel.Extension;
            }

            projectDocumentDto.ModifiedBy = _jwtTokenAccesser.UserId;
            projectDocumentDto.ModifiedDate = DateTime.Now.ToUniversalTime();

            var projectDocument = _mapper.Map<ProjectDocument>(projectDocumentDto);

            /* Check for duplicate document name */
            var validate = _projectDocumentRepository.Duplicate(projectDocument);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectDocumentRepository.Add(projectDocument);

            if (_uow.Save() <= 0) throw new Exception("Creating project document failed on save.");

            _documentReviewRepository.SaveByDocumentId(projectDocument.Id, projectDocument.ProjectId);
            _projectRightRepository.UpdateIsReviewDone(projectDocumentDto.ProjectId);
            return Ok(projectDocument.Id);


            #region Comment
            //if (parentProject != null)
            //{
            //    var projectDesign = _projectDesignRepository
            //    .FindBy(t => t.ProjectId == parentProject && t.DeletedDate == null).FirstOrDefault();

            //    if (projectDesign == null || !projectDesign.IsCompleteDesign)
            //    {
            //        ModelState.AddModelError("Message", "Design not completed for this project");
            //        return BadRequest(ModelState);
            //    }

            //    projectDocumentDto.Id = 0;
            //    if (projectDocumentDto.FileModel?.Base64?.Length > 0)
            //    {
            //        projectDocumentDto.PathName = DocumentService.SaveProjectDocument(projectDocumentDto.FileModel,
            //            _uploadSettingRepository.GetDocumentPath(), FolderType.Project);
            //        projectDocumentDto.MimeType = projectDocumentDto.FileModel.Extension;
            //    }

            //    projectDocumentDto.ModifiedBy = _jwtTokenAccesser.UserId;
            //    projectDocumentDto.ModifiedDate = DateTime.Now.ToUniversalTime();
            //    var projectDocument = _mapper.Map<ProjectDocument>(projectDocumentDto);
            //    /* Added by Vipul for effective Date on 16-10-2019 */
            //    var validate = _projectDocumentRepository.Duplicate(projectDocument);
            //    if (!string.IsNullOrEmpty(validate))
            //    {
            //        ModelState.AddModelError("Message", validate);
            //        return BadRequest(ModelState);
            //    }
            //    _projectDocumentRepository.Add(projectDocument);
            //    if (_uow.Save() <= 0) throw new Exception("Creating project document failed on save.");
            //    _documentReviewRepository.SaveByDocumentId(projectDocument.Id, projectDocument.ProjectId);
            //}
            //else
            //{
            //    var projectDesign = _projectDesignRepository
            //    .FindBy(t => t.ProjectId == projectDocumentDto.ProjectId && t.DeletedDate == null).FirstOrDefault();
            //    if (projectDesign == null || !projectDesign.IsCompleteDesign)
            //    {
            //        ModelState.AddModelError("Message", "Design not completed for this project");
            //        return BadRequest(ModelState);
            //    }
            //    var childProjectIds = _projectRepository.GetChildProjectDropDown(projectDocumentDto.ProjectId);
            //    for (var i = 0; i < childProjectIds.Count; i++)
            //    {
            //        var i1 = i;
            //        projectDocumentDto.ProjectId = childProjectIds[i1].Id;
            //        var projectDocuments = _mapper.Map<ProjectDocument>(projectDocumentDto);
            //        projectDocumentDto.Id = 0;
            //        if (i1 < 1)
            //        {
            //            if (projectDocumentDto.FileModel?.Base64?.Length > 0)
            //            {
            //                projectDocumentDto.PathName = DocumentService.SaveProjectDocument(projectDocumentDto.FileModel,
            //                    _uploadSettingRepository.GetDocumentPath(), FolderType.Project);
            //                projectDocumentDto.MimeType = projectDocumentDto.FileModel.Extension;
            //            }
            //            projectDocumentDto.ModifiedBy = _jwtTokenAccesser.UserId;
            //            projectDocumentDto.ModifiedDate = DateTime.Now.ToUniversalTime();
            //            projectDocuments = _mapper.Map<ProjectDocument>(projectDocumentDto);
            //            var validates = _projectDocumentRepository.Duplicate(projectDocuments);
            //            if (!string.IsNullOrEmpty(validates))
            //            {
            //                ModelState.AddModelError("Message", validates);
            //                return BadRequest(ModelState);
            //            }
            //        }
            //        _projectDocumentRepository.Add(projectDocuments);
            //        if (_uow.Save() <= 0) throw new Exception("Creating project document failed on save.");

            //        _documentReviewRepository.SaveByDocumentId(projectDocuments.Id, projectDocuments.ProjectId);
            //    }
            //}
            //_projectRightRepository.UpdateIsReviewDone(projectDocumentDto.ProjectId);
            //return Ok(projectDocumentDto.Id);
            #endregion Comment
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
                //document = new ProjectDocument();
                // var documentCategory = _documentTypeRepository.Find(1).TypeName;
                var documentCategory = "";
                document.PathName = DocumentService.SaveDocument(projectDocumentDto.FileModel,
                    _uploadSettingRepository.GetDocumentPath(), FolderType.Project, documentCategory);
                document.MimeType = projectDocumentDto.FileModel.Extension;
                document.FileName = projectDocumentDto.FileName;
            }
            //var projectDocument = _mapper.Map<ProjectDocument>(projectDocumentDto);

            /* Added by Vipul for effective Date on 16-10-2019 */
            var validate = _projectDocumentRepository.Duplicate(document);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _projectDocumentRepository.Update(document);

            if (_uow.Save() <= 0) throw new Exception("Updating project document failed on save.");
            _projectRightRepository.UpdateIsReviewDone(projectDocumentDto.ProjectId);
            return Ok(document.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDocumentRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectDocumentRepository.Delete(record);
            _documentReviewRepository.DeleteByDocumentId(id, record.ProjectId);

            _uow.Save();

            _projectRightRepository.UpdateIsReviewDone(record.ProjectId);

            return Ok();
        }
    }
}