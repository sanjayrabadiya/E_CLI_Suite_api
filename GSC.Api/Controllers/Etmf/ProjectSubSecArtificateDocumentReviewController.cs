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
    public class ProjectSubSecArtificateDocumentReviewController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;
        private readonly IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        public ProjectSubSecArtificateDocumentReviewController(IUnitOfWork uow,
        IMapper mapper,
        IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
        IJwtTokenAccesser jwtTokenAccesser,
        IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository,
        IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
        }

        [HttpGet]
        [Route("UserRoles/{Id}/{ProjectId}/{ProjectDetailsId}")]
        public IActionResult UserRoles(int Id, int ProjectId, int ProjectDetailsId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectSubSecArtificateDocumentReviewRepository.UserRoles(Id, ProjectId, ProjectDetailsId));
        }

        [HttpPost]
        [Route("SaveDocumentReview")]
        public IActionResult SaveDocumentReview([FromBody] List<ProjectSubSecArtificateDocumentReviewDto> projectSubSecArtificateDocumentReviewDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _projectSubSecArtificateDocumentReviewRepository.SaveDocumentReview(projectSubSecArtificateDocumentReviewDto);

            return Ok();
        }

        [HttpPut]
        [Route("SendBackDocument/{id}/{isReview}/{seqNo}")]
        public IActionResult SendBackDocument(int id, bool isReview, int? seqNo)
        {
            var projectArtificateDocumentReviewDto = _projectSubSecArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceSubSecArtificateDocumentId == id
            && x.UserId == _jwtTokenAccesser.UserId && x.SendBackDate == null && x.IsReviewed == false && x.DeletedDate == null && x.SequenceNo == (seqNo == 0 ? null : seqNo)).FirstOrDefault();

            projectArtificateDocumentReviewDto.IsSendBack = true;
            projectArtificateDocumentReviewDto.IsReviewed = isReview;
            projectArtificateDocumentReviewDto.SendBackDate = _jwtTokenAccesser.GetClientDate();
            var projectArtificateDocumentReview = _mapper.Map<ProjectSubSecArtificateDocumentReview>(projectArtificateDocumentReviewDto);
            _projectSubSecArtificateDocumentReviewRepository.Update(projectArtificateDocumentReview);


            //if (isReview)
            //{
            //    var projectArtificateDocumentReviewDtos = _projectSubSecArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceSubSecArtificateDocumentId == id
            //    && x.UserId == _jwtTokenAccesser.UserId && x.IsSendBack == true && x.IsReviewed == false && x.DeletedDate == null);
            //    foreach (var item in projectArtificateDocumentReviewDtos)
            //    {
            //        item.IsReviewed = true;
            //        _projectSubSecArtificateDocumentReviewRepository.Update(item);
            //    }
            //}

            if (_uow.Save() <= 0) throw new Exception("Updating Send Back failed on save.");
            _projectSubSecArtificateDocumentReviewRepository.SendMailToSendBack(projectArtificateDocumentReview);



            if (seqNo > 0 && isReview)
            {
                var projectArtificateDocumentReviewDtos = _projectSubSecArtificateDocumentReviewRepository.All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == id && x.SequenceNo > seqNo && x.DeletedDate == null)
                    .OrderBy(x => x.SequenceNo).FirstOrDefault();
                if (projectArtificateDocumentReviewDtos != null)
                {
                    var reviewDto = _mapper.Map<ProjectSubSecArtificateDocumentReviewDto>(projectArtificateDocumentReviewDtos);
                    _projectSubSecArtificateDocumentReviewRepository.SendMailToReviewer(reviewDto);
                }
            }

            var projectWorkplaceArtificatedocument = _projectWorkplaceSubSecArtificatedocumentRepository.Find(projectArtificateDocumentReviewDto.ProjectWorkplaceSubSecArtificateDocumentId);
            _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, projectArtificateDocumentReviewDto.Id, null);
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
                var record = _projectSubSecArtificateDocumentReviewRepository.Find(item);

                var allRecords = _projectSubSecArtificateDocumentReviewRepository.All.Where(q => q.UserId == record.UserId && q.DeletedDate == null && q.ProjectWorkplaceSubSecArtificateDocumentId == record.ProjectWorkplaceSubSecArtificateDocumentId && q.IsReviewed == false);

                if (allRecords == null)
                    return NotFound();

                foreach (var resultRecord in allRecords)
                {
                    _projectSubSecArtificateDocumentReviewRepository.Delete(resultRecord);
                }
            }

            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetPendingReviewer/{documentId}")]
        public IActionResult GetPendingReviewer(int documentId)
        {
            var result = _projectSubSecArtificateDocumentReviewRepository.GetReviewPending(documentId);
            return Ok(result);
        }

        [HttpGet]
        [Route("GetUsers/{Id}/{ProjectId}")]
        public IActionResult GetUsers(int Id, int ProjectId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectSubSecArtificateDocumentReviewRepository.GetUsers(Id, ProjectId));
        }

        [HttpPost]
        [Route("ReplaceUser")]
        public IActionResult ReplaceUser([FromBody] ReplaceUserDto replaceUserDto)
        {
            if (replaceUserDto.DocumentId <= 0) return BadRequest();
            return Ok(_projectSubSecArtificateDocumentReviewRepository.ReplaceUser(replaceUserDto.DocumentId, replaceUserDto.UserId, replaceUserDto.ReplaceUserId));
        }
    }
}