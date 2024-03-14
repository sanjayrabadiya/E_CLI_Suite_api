using AutoMapper;
using DocumentFormat.OpenXml.Office2010.Excel;
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
    public class ProjectArtificateDocumentApproverController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;

        public ProjectArtificateDocumentApproverController(IProjectRepository projectRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository
           )
        {
            _uow = uow;
            _mapper = mapper;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;

        }

        /// Add approver
        /// Created By Swati
        [HttpPost]
        public IActionResult Post([FromBody] ProjectArtificateDocumentApproverDto ProjectArtificateDocumentApproverDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            ProjectArtificateDocumentApproverDto.Id = 0;
            var ProjectArtificateDocumentApprover = _mapper.Map<ProjectArtificateDocumentApprover>(ProjectArtificateDocumentApproverDto);

            _projectArtificateDocumentApproverRepository.Add(ProjectArtificateDocumentApprover);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Approver failed on save.");
                return BadRequest(ModelState);
            }

            _projectArtificateDocumentApproverRepository.SendMailForApprover(ProjectArtificateDocumentApproverDto);
            _projectWorkplaceArtificatedocumentRepository.UpdateApproveDocument(ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId, false);

            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApprover.ProjectWorkplaceArtificatedDocumentId);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, ProjectArtificateDocumentApprover.Id);

            return Ok(ProjectArtificateDocumentApprover.Id);
        }

        [HttpPost]
        [Route("SaveDocumentApprove")]
        public IActionResult SaveDocumentApprove([FromBody] List<ProjectArtificateDocumentApproverDto> projectArtificateDocumentApproveDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            foreach (var ProjectArtificateDocumentApproverDto in projectArtificateDocumentApproveDto)
            {

                ProjectArtificateDocumentApproverDto.Id = 0;
                var ProjectArtificateDocumentApprover = _mapper.Map<ProjectArtificateDocumentApprover>(ProjectArtificateDocumentApproverDto);

                _projectArtificateDocumentApproverRepository.Add(ProjectArtificateDocumentApprover);

                if (_uow.Save() <= 0)
                {
                    ModelState.AddModelError("Message", "Creating Approver failed on save.");
                    return BadRequest(ModelState);
                }

                _projectWorkplaceArtificatedocumentRepository.UpdateApproveDocument(ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId, false);

                var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApprover.ProjectWorkplaceArtificatedDocumentId);
                _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, ProjectArtificateDocumentApprover.Id);

            }



            if (projectArtificateDocumentApproveDto.Count(x => x.SequenceNo == null) == projectArtificateDocumentApproveDto.Count)
            {
                foreach (var ReviewDto in projectArtificateDocumentApproveDto)
                {
                    _projectArtificateDocumentApproverRepository.SendMailForApprover(ReviewDto);
                }
            }
            else
            {
                var firstRecord = projectArtificateDocumentApproveDto.OrderBy(x => x.SequenceNo).FirstOrDefault();
                if (firstRecord?.IsApproved == null)
                    _projectArtificateDocumentApproverRepository.SendMailForApprover(firstRecord);
            }

            _projectArtificateDocumentApproverRepository.SkipDocumentApproval(projectArtificateDocumentApproveDto[0].ProjectWorkplaceArtificatedDocumentId, false);

            return Ok(1);
        }

        /// Get UserName For Approval list
        /// Created By Swati
        [HttpGet]
        [Route("UserNameForApproval/{Id}/{ProjectId}/{ProjectDetailsId}")]
        public IActionResult UserNameForApproval(int Id, int ProjectId, int ProjectDetailsId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectArtificateDocumentApproverRepository.UserNameForApproval(Id, ProjectId, ProjectDetailsId));
        }

        /// update approve doc or not
        /// Created By Swati
        [HttpPut]
        [Route("ApproveDocument/{DocApprover}/{seqNo}/{Id}")]
        public IActionResult ApproveDocument(bool DocApprover, int? seqNo, int Id)
        {
            var ProjectArtificateDocumentApproverDto = _projectArtificateDocumentApproverRepository.FindByInclude(x => x.UserId == _jwtTokenAccesser.UserId
            && x.ProjectWorkplaceArtificatedDocumentId == Id && x.IsApproved == null && x.SequenceNo == (seqNo == 0 ? null : seqNo)).FirstOrDefault();

            var ProjectArtificateDocumentApprover = _mapper.Map<ProjectArtificateDocumentApprover>(ProjectArtificateDocumentApproverDto);
            ProjectArtificateDocumentApprover.IsApproved = DocApprover;
            _projectArtificateDocumentApproverRepository.Update(ProjectArtificateDocumentApprover);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Approver failed on save.");
                return BadRequest(ModelState);
            }
            _projectArtificateDocumentApproverRepository.SendMailForApprovedRejected(ProjectArtificateDocumentApprover);

            _projectWorkplaceArtificatedocumentRepository.IsApproveDocument(Id);
            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApprover.ProjectWorkplaceArtificatedDocumentId);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, ProjectArtificateDocumentApprover.Id);


            if (seqNo > 0 && DocApprover)
            {
                var projectArtificateDocumentReviewDtos = _projectArtificateDocumentApproverRepository.All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == Id && x.SequenceNo > seqNo && x.DeletedDate == null)
                    .OrderBy(x => x.SequenceNo).FirstOrDefault();
                if (projectArtificateDocumentReviewDtos != null)
                {
                    var reviewDto = _mapper.Map<ProjectArtificateDocumentApproverDto>(projectArtificateDocumentReviewDtos);
                    _projectArtificateDocumentApproverRepository.SendMailForApprover(reviewDto);
                }
            }

            return Ok(ProjectArtificateDocumentApprover.Id);
        }

        /// Delete approve
        /// Created By Swati
        [HttpPost]
        [Route("DeleteDocumentApprover")]
        public IActionResult DeleteDocumentApprover([FromBody] List<int> Data)
        {
            List<int> documentList = new List<int>();
            foreach (var item in Data)
            {
                var record = _projectArtificateDocumentApproverRepository.Find(item);

                var allRecords = _projectArtificateDocumentApproverRepository.All.Where(q => q.UserId == record.UserId && q.DeletedDate == null && q.ProjectWorkplaceArtificatedDocumentId == record.ProjectWorkplaceArtificatedDocumentId && q.IsApproved != true && q.DeletedDate == null).ToList();

                if (!allRecords.Any())
                    return NotFound();

                foreach (var resultRecord in allRecords)
                {
                    _projectArtificateDocumentApproverRepository.Delete(resultRecord);
                }

                documentList.Add(record.ProjectWorkplaceArtificatedDocumentId);
            }

            _uow.Save();

            //Logic add by Tinku Mahato (09/06/2023)

            foreach (var item in documentList.Distinct())
            {
                var document = _projectWorkplaceArtificatedocumentRepository.Find(item);
                var allRecords = _projectArtificateDocumentApproverRepository.All.Where(q => q.ProjectWorkplaceArtificatedDocumentId == item && q.DeletedDate == null).ToList();
                if (allRecords.TrueForAll(x => x.IsApproved == true) && allRecords.Count > 0)
                {
                    document.IsAccepted = true;
                }

                _projectWorkplaceArtificatedocumentRepository.Update(document);
            }
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetPendingApprove/{documentId}")]
        public IActionResult GetPendingApprove(int documentId)
        {
            var result = _projectArtificateDocumentApproverRepository.GetApprovePending(documentId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetUsers/{Id}/{ProjectId}")]
        public IActionResult GetUsers(int Id, int ProjectId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectArtificateDocumentApproverRepository.GetUsers(Id, ProjectId));
        }

        [HttpPost]
        [Route("ReplaceUser")]
        public IActionResult ReplaceUser([FromBody] ReplaceUserDto replaceUserDto)
        {
            if (replaceUserDto.DocumentId <= 0) return BadRequest();
            return Ok(_projectArtificateDocumentApproverRepository.ReplaceUser(replaceUserDto.DocumentId, replaceUserDto.UserId, replaceUserDto.ReplaceUserId));
        }

        [HttpGet]
        [Route("GetMaxDueDate/{documentId}")]
        public IActionResult GetMaxDueDate(int documentId)
        {
            if (documentId <= 0) return BadRequest();
            return Ok(_projectArtificateDocumentApproverRepository.GetMaxDueDate(documentId));
        }

        [HttpGet]
        [Route("SkipDocumentApproval/{documentId}")]
        public IActionResult SkipDocumentApproval(int documentId)
        {
            if (documentId <= 0) return BadRequest();
            return Ok(_projectArtificateDocumentApproverRepository.SkipDocumentApproval(documentId, true));
        }
    }
}