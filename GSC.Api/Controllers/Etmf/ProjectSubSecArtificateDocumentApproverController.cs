﻿using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectSubSecArtificateDocumentApproverController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectSubSecArtificateDocumentApproverRepository _projectSubSecArtificateDocumentApproverRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;

        public ProjectSubSecArtificateDocumentApproverController(IUnitOfWork uow,
            IMapper mapper,
            IProjectSubSecArtificateDocumentApproverRepository projectSubSecArtificateDocumentApproverRepository,
            IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository
           )
        {
            _uow = uow;
            _mapper = mapper;
            _projectSubSecArtificateDocumentApproverRepository = projectSubSecArtificateDocumentApproverRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;

        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectSubSecArtificateDocumentApproverDto ProjectSubSecArtificateDocumentApproverDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            ProjectSubSecArtificateDocumentApproverDto.Id = 0;
            var ProjectSubSecArtificateDocumentApprover = _mapper.Map<ProjectSubSecArtificateDocumentApprover>(ProjectSubSecArtificateDocumentApproverDto);

            _projectSubSecArtificateDocumentApproverRepository.Add(ProjectSubSecArtificateDocumentApprover);
            if (_uow.Save() <= 0) throw new Exception("Creating Approver failed on save.");

            _projectSubSecArtificateDocumentApproverRepository.SendMailForApprover(ProjectSubSecArtificateDocumentApproverDto);
            _projectWorkplaceSubSecArtificatedocumentRepository.UpdateApproveDocument(ProjectSubSecArtificateDocumentApproverDto.ProjectWorkplaceSubSecArtificateDocumentId, false);

            var projectWorkplaceArtificatedocument = _projectWorkplaceSubSecArtificatedocumentRepository.Find(ProjectSubSecArtificateDocumentApprover.ProjectWorkplaceSubSecArtificateDocumentId);
            _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, ProjectSubSecArtificateDocumentApprover.Id);

            return Ok(ProjectSubSecArtificateDocumentApprover.Id);
        }

        [HttpPost]
        [Route("SaveDocumentApprove")]
        public IActionResult SaveDocumentApprove([FromBody] List<ProjectSubSecArtificateDocumentApproverDto> projectArtificateDocumentApproveDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            foreach (var ProjectSubSecArtificateDocumentApproverDto in projectArtificateDocumentApproveDto)
            {
                ProjectSubSecArtificateDocumentApproverDto.Id = 0;
                var ProjectSubSecArtificateDocumentApprover = _mapper.Map<ProjectSubSecArtificateDocumentApprover>(ProjectSubSecArtificateDocumentApproverDto);

                _projectSubSecArtificateDocumentApproverRepository.Add(ProjectSubSecArtificateDocumentApprover);


                if (_uow.Save() <= 0) throw new Exception("Creating Approver failed on save.");
                // _projectSubSecArtificateDocumentApproverRepository.SendMailForApprover(ProjectSubSecArtificateDocumentApproverDto);
                _projectWorkplaceSubSecArtificatedocumentRepository.UpdateApproveDocument(ProjectSubSecArtificateDocumentApproverDto.ProjectWorkplaceSubSecArtificateDocumentId, false);

                var projectWorkplaceArtificatedocument = _projectWorkplaceSubSecArtificatedocumentRepository.Find(ProjectSubSecArtificateDocumentApprover.ProjectWorkplaceSubSecArtificateDocumentId);
                _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, ProjectSubSecArtificateDocumentApprover.Id);

            }

            if (projectArtificateDocumentApproveDto.Where(x => x.SequenceNo == null).Count() == projectArtificateDocumentApproveDto.Count())
            {
                foreach (var ReviewDto in projectArtificateDocumentApproveDto)
                {
                    _projectSubSecArtificateDocumentApproverRepository.SendMailForApprover(ReviewDto);
                }
            }
            else
            {
                var firstRecord = projectArtificateDocumentApproveDto.OrderBy(x => x.SequenceNo).FirstOrDefault();
                if (firstRecord.IsApproved == null)
                    _projectSubSecArtificateDocumentApproverRepository.SendMailForApprover(firstRecord);
            }

            return Ok(1);
        }

        [HttpGet]
        [Route("UserNameForApproval/{Id}/{ProjectId}/{ProjectDetailsId}")]
        public IActionResult UserNameForApproval(int Id, int ProjectId, int ProjectDetailsId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectSubSecArtificateDocumentApproverRepository.UserNameForApproval(Id, ProjectId, ProjectDetailsId));
        }

        [HttpPut]
        [Route("ApproveDocument/{DocApprover}/{seqNo}/{Id}")]
        public IActionResult ApproveDocument(bool DocApprover, int? seqNo, int Id)
        {
            var ProjectSubSecArtificateDocumentApproverDto = _projectSubSecArtificateDocumentApproverRepository.FindByInclude(x => x.UserId == _jwtTokenAccesser.UserId
            && x.ProjectWorkplaceSubSecArtificateDocumentId == Id && x.IsApproved == null && x.SequenceNo == (seqNo == 0 ? null : seqNo)).FirstOrDefault();

            var ProjectSubSecArtificateDocumentApprover = _mapper.Map<ProjectSubSecArtificateDocumentApprover>(ProjectSubSecArtificateDocumentApproverDto);
            ProjectSubSecArtificateDocumentApprover.IsApproved = DocApprover ? true : false;
            _projectSubSecArtificateDocumentApproverRepository.Update(ProjectSubSecArtificateDocumentApprover);
            if (_uow.Save() <= 0) throw new Exception("Updating Approver failed on save.");

            _projectSubSecArtificateDocumentApproverRepository.IsApproveDocument(Id);
            var projectWorkplaceSubSecArtificatedocument = _projectWorkplaceSubSecArtificatedocumentRepository.Find(ProjectSubSecArtificateDocumentApprover.ProjectWorkplaceSubSecArtificateDocumentId);
            _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceSubSecArtificatedocument, null, ProjectSubSecArtificateDocumentApprover.Id);

            if (seqNo > 0 && DocApprover)
            {
                var projectArtificateDocumentReviewDtos = _projectSubSecArtificateDocumentApproverRepository.All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == Id && x.SequenceNo > seqNo && x.DeletedDate == null)
                    .OrderBy(x => x.SequenceNo).FirstOrDefault();
                if (projectArtificateDocumentReviewDtos != null)
                {
                    var reviewDto = _mapper.Map<ProjectSubSecArtificateDocumentApproverDto>(projectArtificateDocumentReviewDtos);
                    _projectSubSecArtificateDocumentApproverRepository.SendMailForApprover(reviewDto);
                }
            }

            return Ok(ProjectSubSecArtificateDocumentApprover.Id);
        }

        /// Delete approve
        /// Created By Swati
        [HttpPost]
        [Route("DeleteDocumentApprover")]
        public IActionResult DeleteDocumentApprover([FromBody] List<int> Data)
        {
            foreach (var item in Data)
            {
                var record = _projectSubSecArtificateDocumentApproverRepository.Find(item);

                if (record == null)
                    return NotFound();

                _projectSubSecArtificateDocumentApproverRepository.Delete(record);
            }

            _uow.Save();
            return Ok();
        }


        [HttpGet]
        [Route("GetPendingSubSectionApprove/{documentId}")]
        public IActionResult GetPendingSubSectionApprove(int documentId)
        {
            var result = _projectSubSecArtificateDocumentApproverRepository.GetApprovePending(documentId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetUsers/{Id}/{ProjectId}")]
        public IActionResult GetUsers(int Id, int ProjectId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectSubSecArtificateDocumentApproverRepository.GetUsers(Id, ProjectId));
        }

        [HttpPost]
        [Route("ReplaceUser")]
        public IActionResult ReplaceUser([FromBody] ReplaceUserDto replaceUserDto)
        {
            if (replaceUserDto.DocumentId <= 0) return BadRequest();
            return Ok(_projectSubSecArtificateDocumentApproverRepository.ReplaceUser(replaceUserDto.DocumentId, replaceUserDto.UserId, replaceUserDto.ReplaceUserId));
        }
    }
}