using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Etmf;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectArtificateDocumentApproverController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IProjectArtificateDocumentApproverRepository _projectArtificateDocumentApproverRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectWorkplaceArtificatedocumentRepository _projectWorkplaceArtificatedocumentRepository;
        public ProjectArtificateDocumentApproverController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProjectWorkplaceArtificatedocumentRepository projectWorkplaceArtificatedocumentRepository,
            IJwtTokenAccesser jwtTokenAccesser
           )
        {
            _uow = uow;
            _mapper = mapper;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectWorkplaceArtificatedocumentRepository = projectWorkplaceArtificatedocumentRepository;
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectArtificateDocumentApproverDto ProjectArtificateDocumentApproverDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            ProjectArtificateDocumentApproverDto.Id = 0;
            var ProjectArtificateDocumentApprover = _mapper.Map<ProjectArtificateDocumentApprover>(ProjectArtificateDocumentApproverDto);

            _projectArtificateDocumentApproverRepository.Add(ProjectArtificateDocumentApprover);
            if (_uow.Save() <= 0) throw new Exception("Creating Approver failed on save.");

            _projectArtificateDocumentApproverRepository.SendMailForApprover(ProjectArtificateDocumentApproverDto);
            _projectWorkplaceArtificatedocumentRepository.UpdateApproveDocument(ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId, false);
            return Ok(ProjectArtificateDocumentApprover.Id);
        }

        [HttpGet]
        [Route("UserNameForApproval/{Id}")]
        public IActionResult UserNameForApproval(int Id)
        {
            if (Id <= 0) return BadRequest();
            return Ok(_projectArtificateDocumentApproverRepository.UserNameForApproval(Id));
        }

        [HttpPut]
        [Route("ApproveDocument/{DocApprover}/{Id}")]
        public IActionResult ApproveDocument(bool DocApprover, int Id)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var ProjectArtificateDocumentApproverDto = _projectArtificateDocumentApproverRepository.FindByInclude(x => x.UserId == _jwtTokenAccesser.UserId
            && x.ProjectWorkplaceArtificatedDocumentId == Id && x.IsApproved == null).FirstOrDefault();

            var ProjectArtificateDocumentApprover = _mapper.Map<ProjectArtificateDocumentApprover>(ProjectArtificateDocumentApproverDto);
            ProjectArtificateDocumentApprover.IsApproved = DocApprover ?  true : false;
            _projectArtificateDocumentApproverRepository.Update(ProjectArtificateDocumentApprover);
            if (_uow.Save() <= 0) throw new Exception("Updating Approver failed on save.");

            var DocumentApprover = _projectArtificateDocumentApproverRepository.FindByInclude(x => x.ProjectWorkplaceArtificatedDocumentId == Id
            && x.DeletedDate == null && (x.IsApproved == null || x.IsApproved == true)).ToList();

            if (DocumentApprover.All(x => x.IsApproved == true)) {
                _projectWorkplaceArtificatedocumentRepository.UpdateApproveDocument(Id, true);
            }
            //_projectArtificateDocumentApproverRepository.SendMailForApprover(ProjectArtificateDocumentApproverDto);
            return Ok(ProjectArtificateDocumentApprover.Id);
        }

    }
}