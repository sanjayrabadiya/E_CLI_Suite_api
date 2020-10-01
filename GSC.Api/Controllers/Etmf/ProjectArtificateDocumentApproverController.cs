using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;
using System;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectArtificateDocumentApproverController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IETMFWorkplaceRepository _eTMFWorkplaceRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IProjectArtificateDocumentHistoryRepository _projectArtificateDocumentHistoryRepository;
        private readonly IProjectWorkplaceArtificateDocumentReviewRepository _projectWorkplaceArtificateDocumentReviewRepository;
        private readonly IEtmfArtificateMasterLbraryRepository _etmfArtificateMasterLbraryRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectArtificateDocumentApproverController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
            IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectArtificateDocumentHistoryRepository projectArtificateDocumentHistoryRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _projectRepository = projectRepository;
            _uow = uow;
            _mapper = mapper;
            _eTMFWorkplaceRepository = eTMFWorkplaceRepository;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _projectWorkplaceArtificateDocumentReviewRepository = projectWorkplaceArtificateDocumentReviewRepository;
            _etmfArtificateMasterLbraryRepository = etmfArtificateMasterLbraryRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _projectArtificateDocumentHistoryRepository = projectArtificateDocumentHistoryRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectArtificateDocumentApproverDto ProjectArtificateDocumentApproverDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            ProjectArtificateDocumentApproverDto.Id = 0;
            var ProjectArtificateDocumentApprover = _mapper.Map<ProjectArtificateDocumentApprover>(ProjectArtificateDocumentApproverDto);

            _projectArtificateDocumentApproverRepository.Add(ProjectArtificateDocumentApprover);

            if (_uow.Save() <= 0) throw new Exception("Creating Approver failed on save.");
            return Ok(ProjectArtificateDocumentApprover.Id);
        }

        [HttpGet]
        [Route("UserNameForApproval/{Id}")]
        public IActionResult UserNameForApproval(int Id)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectArtificateDocumentApproverRepository.UserNameForApproval(Id));
        }

    }
}