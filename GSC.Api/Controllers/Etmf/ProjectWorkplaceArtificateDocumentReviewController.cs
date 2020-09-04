using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

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
        public ProjectWorkplaceArtificateDocumentReviewController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IETMFWorkplaceRepository eTMFWorkplaceRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IProjectWorkplaceArtificateDocumentReviewRepository projectWorkplaceArtificateDocumentReviewRepository,
              IEtmfArtificateMasterLbraryRepository etmfArtificateMasterLbraryRepository,
              IUploadSettingRepository uploadSettingRepository, IJwtTokenAccesser jwtTokenAccesser
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
        public IActionResult SaveDocumentReview([FromBody]List<ProjectArtificateDocumentReviewDto> projectArtificateDocumentReviewDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            _projectWorkplaceArtificateDocumentReviewRepository.SaveDocumentReview(projectArtificateDocumentReviewDto);

            if (_uow.Save() < 0) throw new Exception("Artificate Send failed on save.");

            return Ok();
        }

        [HttpPut]
        [Route("SendBackDocument/{id}")]
        public IActionResult SendBackDocument(int id)
        {
            var projectArtificateDocumentReviewList = _projectWorkplaceArtificateDocumentReviewRepository.FindByInclude(x=>x.ProjectWorkplaceArtificatedDocumentId == id 
            && x.UserId == _jwtTokenAccesser.UserId && x.SendBackDate == null && x.DeletedDate == null);

            foreach (var item in projectArtificateDocumentReviewList)
            {
                item.IsSendBack = true;
                item.SendBackDate = DateTime.UtcNow;
                var projectArtificateDocumentReview = _mapper.Map<ProjectArtificateDocumentReview>(item);
                _projectWorkplaceArtificateDocumentReviewRepository.Update(projectArtificateDocumentReview);
            }

            if (_uow.Save() <= 0) throw new Exception("Updating Send Back failed on save.");
            return Ok();
        }
    }
}