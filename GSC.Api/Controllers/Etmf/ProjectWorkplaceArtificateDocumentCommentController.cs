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
using GSC.Shared;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectWorkplaceArtificateDocumentCommentController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectArtificateDocumentCommentRepository _projectArtificateDocumentCommentRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectWorkplaceArtificateDocumentCommentController(IProjectRepository projectRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectArtificateDocumentCommentRepository projectArtificateDocumentCommentRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _uow = uow;
            _mapper = mapper;
            _eTMFWorkplaceRepository = eTMFWorkplaceRepository;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectArtificateDocumentCommentRepository = projectArtificateDocumentCommentRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{documentId}")]
        public IActionResult Get(int documentId)
        {
            if (documentId <= 0) return BadRequest();

            var commentsDto = _projectArtificateDocumentCommentRepository.GetComments(documentId);

            return Ok(commentsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectArtificateDocumentCommentDto projectArtificateDocumentCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            projectArtificateDocumentCommentDto.Id = 0;
            var projectArtificateDocumentComment = _mapper.Map<ProjectArtificateDocumentComment>(projectArtificateDocumentCommentDto);

            _projectArtificateDocumentCommentRepository.Add(projectArtificateDocumentComment);

            if (_uow.Save() <= 0) throw new Exception("Creating Comment failed on save.");
            return Ok(projectArtificateDocumentComment.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectArtificateDocumentCommentDto projectArtificateDocumentCommentDto)
        {
            if (projectArtificateDocumentCommentDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (!string.IsNullOrEmpty(projectArtificateDocumentCommentDto.Response) && projectArtificateDocumentCommentDto.ResponseDate == null)
            {
                projectArtificateDocumentCommentDto.ResponseBy = _jwtTokenAccesser.UserId;
                projectArtificateDocumentCommentDto.ResponseDate = DateTime.Now;
            }

            if (projectArtificateDocumentCommentDto.IsClose) {
                projectArtificateDocumentCommentDto.CloseBy = _jwtTokenAccesser.UserId;
                projectArtificateDocumentCommentDto.CloseDate = DateTime.Now;
            }

            var projectArtificateDocumentComment = _mapper.Map<ProjectArtificateDocumentComment>(projectArtificateDocumentCommentDto);
            _projectArtificateDocumentCommentRepository.Update(projectArtificateDocumentComment);

            if (_uow.Save() <= 0) throw new Exception("Updating Response failed on save.");
            return Ok(projectArtificateDocumentComment.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectArtificateDocumentCommentRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectArtificateDocumentCommentRepository.Delete(record);
            _uow.Save();

            return Ok();
        }
    }
}