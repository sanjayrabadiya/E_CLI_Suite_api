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

        public ProjectArtificateDocumentApproverController(IProjectRepository projectRepository,
            IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IProjectArtificateDocumentApproverRepository projectArtificateDocumentApproverRepository,
            IUploadSettingRepository uploadSettingRepository,
            IJwtTokenAccesser jwtTokenAccesser
           )
        {
            _uow = uow;
            _mapper = mapper;
            _projectArtificateDocumentApproverRepository = projectArtificateDocumentApproverRepository;
            _uploadSettingRepository = uploadSettingRepository;
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

            _projectArtificateDocumentApproverRepository.SendMailForApprover(ProjectArtificateDocumentApproverDto);
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