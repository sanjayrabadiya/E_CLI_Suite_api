using AutoMapper;
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
            if (_uow.Save() <= 0) throw new Exception("Creating Approver failed on save.");

            _projectArtificateDocumentApproverRepository.SendMailForApprover(ProjectArtificateDocumentApproverDto);
            _projectWorkplaceArtificatedocumentRepository.UpdateApproveDocument(ProjectArtificateDocumentApproverDto.ProjectWorkplaceArtificatedDocumentId, false);

            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApprover.ProjectWorkplaceArtificatedDocumentId);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, ProjectArtificateDocumentApprover.Id);

            return Ok(ProjectArtificateDocumentApprover.Id);
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
        [Route("ApproveDocument/{DocApprover}/{Id}")]
        public IActionResult ApproveDocument(bool DocApprover, int Id)
        {
            var ProjectArtificateDocumentApproverDto = _projectArtificateDocumentApproverRepository.FindByInclude(x => x.UserId == _jwtTokenAccesser.UserId
            && x.ProjectWorkplaceArtificatedDocumentId == Id && x.IsApproved == null).FirstOrDefault();

            var ProjectArtificateDocumentApprover = _mapper.Map<ProjectArtificateDocumentApprover>(ProjectArtificateDocumentApproverDto);
            ProjectArtificateDocumentApprover.IsApproved = DocApprover ? true : false;
            _projectArtificateDocumentApproverRepository.Update(ProjectArtificateDocumentApprover);
            if (_uow.Save() <= 0) throw new Exception("Updating Approver failed on save.");

            _projectArtificateDocumentApproverRepository.IsApproveDocument(Id);
            var projectWorkplaceArtificatedocument = _projectWorkplaceArtificatedocumentRepository.Find(ProjectArtificateDocumentApprover.ProjectWorkplaceArtificatedDocumentId);
            _projectArtificateDocumentHistoryRepository.AddHistory(projectWorkplaceArtificatedocument, null, ProjectArtificateDocumentApprover.Id);

            return Ok(ProjectArtificateDocumentApprover.Id);
        }

        /// Delete approve
        /// Created By Swati
        [HttpPost]
        [Route("DeleteDocumentApprover")]
        public IActionResult DeleteDocumentApprover([FromBody] List<int> Data)
        {
            foreach (var item in Data)
            {
                var record = _projectArtificateDocumentApproverRepository.Find(item);

                if (record == null)
                    return NotFound();

                _projectArtificateDocumentApproverRepository.Delete(record);
            }

            _uow.Save();
            return Ok();
        }
    }
}