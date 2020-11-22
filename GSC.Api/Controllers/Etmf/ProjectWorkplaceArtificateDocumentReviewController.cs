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
    public class ProjectWorkplaceArtificateDocumentReviewController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;

        public ProjectWorkplaceArtificateDocumentReviewController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
              IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
              IUploadSettingRepository uploadSettingRepository, IJwtTokenAccesser jwtTokenAccesser,
              IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository
            )
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
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
        }

        [HttpGet]
        [Route("UserRoles/{Id}")]
        public IActionResult UserRoles(int Id)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectWorkplaceArtificateDocumentReviewRepository.UserRoles(Id));
        }

        [HttpPost]
        [Route("SaveDocumentReview")]
        public IActionResult SaveDocumentReview([FromBody] List<ProjectArtificateDocumentReviewDto> projectArtificateDocumentReviewDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _projectWorkplaceArtificateDocumentReviewRepository.SaveDocumentReview(projectArtificateDocumentReviewDto);

            return Ok();
        }

        [HttpPut]
        [Route("SendBackDocument/{id}")]
        public IActionResult SendBackDocument(int id)
        {
            var projectArtificateDocumentReviewDto = _projectWorkplaceArtificateDocumentReviewRepository.FindByInclude(x => x.ProjectWorkplaceArtificatedDocumentId == id
            && x.UserId == _jwtTokenAccesser.UserId && x.SendBackDate == null && x.DeletedDate == null).FirstOrDefault();

            projectArtificateDocumentReviewDto.IsSendBack = true;
            projectArtificateDocumentReviewDto.SendBackDate = DateTime.UtcNow;
            var projectArtificateDocumentReview = _mapper.Map<ProjectArtificateDocumentReview>(projectArtificateDocumentReviewDto);
            _projectWorkplaceArtificateDocumentReviewRepository.Update(projectArtificateDocumentReview);

            if (_uow.Save() <= 0) throw new Exception("Updating Send Back failed on save.");
            _projectWorkplaceArtificateDocumentReviewRepository.SendMailToSendBack(projectArtificateDocumentReview);

            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(projectArtificateDocumentReviewDto.ProjectWorkplaceArtificatedDocumentId);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, projectArtificateDocumentReviewDto.Id, null);
            return Ok();
        }
    }
}