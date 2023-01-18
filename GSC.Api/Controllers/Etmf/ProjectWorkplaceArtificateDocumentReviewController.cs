using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;
using GSC.Shared.JWTAuth;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceArtificateDocumentReviewController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;

        public ProjectWorkplaceArtificateDocumentReviewController(IUnitOfWork uow,
            IMapper mapper,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
        }

        /// Get user for send for review
        /// Created By Swati
        [HttpGet]
        [Route("UserRoles/{Id}/{ProjectId}/{ProjectDetailsId}")]
        public IActionResult UserRoles(int Id, int ProjectId, int ProjectDetailsId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectWorkplaceArtificateDocumentReviewRepository.UserRoles(Id, ProjectId, ProjectDetailsId));
        }

        /// Save user for send for review
        /// Created By Swati
        [HttpPost]
        [Route("SaveDocumentReview")]
        public IActionResult SaveDocumentReview([FromBody] List<ProjectArtificateDocumentReviewDto> projectArtificateDocumentReviewDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _projectWorkplaceArtificateDocumentReviewRepository.SaveDocumentReview(projectArtificateDocumentReviewDto);

            return Ok();
        }

        /// Save user for send back
        /// Created By Swati
        [HttpPut]
        [Route("SendBackDocument/{id}/{isReview}/{seqNo}")]
        public IActionResult SendBackDocument(int id, bool isReview, int? seqNo)
        {
            var projectArtificateDocumentReviewDto = _projectWorkplaceArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceArtificatedDocumentId == id
            && x.UserId == _jwtTokenAccesser.UserId && x.SendBackDate == null && x.IsReviewed == false && x.DeletedDate == null && x.SequenceNo == (seqNo == 0 ? null : seqNo)).FirstOrDefault();

            projectArtificateDocumentReviewDto.IsSendBack = true;
            projectArtificateDocumentReviewDto.IsReviewed = isReview;
            projectArtificateDocumentReviewDto.SendBackDate = _jwtTokenAccesser.GetClientDate();
            var projectArtificateDocumentReview = _mapper.Map<ProjectArtificateDocumentReview>(projectArtificateDocumentReviewDto);
            _projectWorkplaceArtificateDocumentReviewRepository.Update(projectArtificateDocumentReview);

            //if (isReview)
            //{
            //    var projectArtificateDocumentReviewDtos = _projectWorkplaceArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceArtificatedDocumentId == id
            //    && x.UserId == _jwtTokenAccesser.UserId && x.IsSendBack == true && x.IsReviewed == false && x.DeletedDate == null);
            //    foreach (var item in projectArtificateDocumentReviewDtos)
            //    {
            //        item.IsReviewed = true;
            //        _projectWorkplaceArtificateDocumentReviewRepository.Update(item);
            //    }
            //}

            if (_uow.Save() <= 0) throw new Exception("Updating Send Back failed on save.");
            _projectWorkplaceArtificateDocumentReviewRepository.SendMailToSendBack(projectArtificateDocumentReview);

            if (seqNo > 0 && isReview)
            {
                var projectArtificateDocumentReviewDtos = _projectWorkplaceArtificateDocumentReviewRepository.All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == id && x.SequenceNo > seqNo && x.DeletedDate == null)
                    .OrderBy(x => x.SequenceNo).FirstOrDefault();
                if (projectArtificateDocumentReviewDtos != null)
                {
                    var reviewDto = _mapper.Map<ProjectArtificateDocumentReviewDto>(projectArtificateDocumentReviewDtos);
                    _projectWorkplaceArtificateDocumentReviewRepository.SendMailToReviewer(reviewDto);
                }
            }

            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(projectArtificateDocumentReviewDto.ProjectWorkplaceArtificatedDocumentId);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, projectArtificateDocumentReviewDto.Id, null);
            return Ok();
        }


        /// Delete review
        /// Created By Swati
        [HttpPost]
        [Route("DeleteDocumentReview")]
        public IActionResult DeleteDocumentReview([FromBody] List<int> Data)
        {
            foreach (var item in Data)
            {
                var record = _projectWorkplaceArtificateDocumentReviewRepository.Find(item);

                if (record == null)
                    return NotFound();

                _projectWorkplaceArtificateDocumentReviewRepository.Delete(record);
            }

            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetPendingReviewer/{documentId}")]
        public IActionResult GetPendingReviewer(int documentId)
        {
            var result = _projectWorkplaceArtificateDocumentReviewRepository.GetReviewPending(documentId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetUsers/{Id}/{ProjectId}")]
        public IActionResult GetUsers(int Id, int ProjectId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectWorkplaceArtificateDocumentReviewRepository.GetUsers(Id, ProjectId));
        }

        [HttpPost]
        [Route("ReplaceUser")]
        public IActionResult ReplaceUser([FromBody] ReplaceUserDto replaceUserDto)
        {
            if (replaceUserDto.DocumentId <= 0) return BadRequest();
            return Ok(_projectWorkplaceArtificateDocumentReviewRepository.ReplaceUser(replaceUserDto.DocumentId, replaceUserDto.UserId, replaceUserDto.ReplaceUserId));
        }
    }
}