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
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectWorkplaceSubSecArtificatedocumentRepository _projectWorkplaceSubSecArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectSubSecArtificateDocumentHistoryRepository _projectSubSecArtificateDocumentHistoryRepository;
        private readonly IProjectSubSecArtificateDocumentReviewRepository _projectSubSecArtificateDocumentReviewRepository;
        public ProjectSubSecArtificateDocumentReviewController(IProjectRepository projectRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IProjectWorkplaceSubSecArtificatedocumentRepository projectWorkplaceSubSecArtificatedocumentRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
              IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
              IUploadSettingRepository uploadSettingRepository, IJwtTokenAccesser jwtTokenAccesser,
              IProjectSubSecArtificateDocumentHistoryRepository projectSubSecArtificateDocumentHistoryRepository,
              IProjectSubSecArtificateDocumentReviewRepository projectSubSecArtificateDocumentReviewRepository
            )
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _uow = uow;
            _mapper = mapper;
            _eTMFWorkplaceRepository = eTMFWorkplaceRepository;
            _projectWorkplaceSubSecArtificatedocumentRepository = projectWorkplaceSubSecArtificatedocumentRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectSubSecArtificateDocumentHistoryRepository = projectSubSecArtificateDocumentHistoryRepository;
            _projectSubSecArtificateDocumentReviewRepository = projectSubSecArtificateDocumentReviewRepository;
        }

        [HttpGet]
        [Route("UserRoles/{Id}/{ProjectId}")]
        public IActionResult UserRoles(int Id, int ProjectId)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectSubSecArtificateDocumentReviewRepository.UserRoles(Id, ProjectId));
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
        [Route("SendBackDocument/{id}")]
        public IActionResult SendBackDocument(int id)
        {
            var projectArtificateDocumentReviewDto = _projectSubSecArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceSubSecArtificateDocumentId == id
            && x.UserId == _jwtTokenAccesser.UserId && x.SendBackDate == null && x.DeletedDate == null).FirstOrDefault();

            projectArtificateDocumentReviewDto.IsSendBack = true;
            projectArtificateDocumentReviewDto.SendBackDate = _jwtTokenAccesser.GetClientDate();
            var projectArtificateDocumentReview = _mapper.Map<ProjectSubSecArtificateDocumentReview>(projectArtificateDocumentReviewDto);
            _projectSubSecArtificateDocumentReviewRepository.Update(projectArtificateDocumentReview);

            if (_uow.Save() <= 0) throw new Exception("Updating Send Back failed on save.");
            _projectSubSecArtificateDocumentReviewRepository.SendMailToSendBack(projectArtificateDocumentReview);

            var projectWorkplaceArtificatedocument = _projectWorkplaceSubSecArtificatedocumentRepository.Find(projectArtificateDocumentReviewDto.ProjectWorkplaceSubSecArtificateDocumentId);
            _projectSubSecArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, projectArtificateDocumentReviewDto.Id, null);
            return Ok();
        }
    }
}