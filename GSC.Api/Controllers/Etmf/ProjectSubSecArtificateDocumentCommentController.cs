using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Etmf;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    public class ProjectSubSecArtificateDocumentCommentController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectSubSecArtificateDocumentCommentRepository _projectSubSecArtificateDocumentCommentRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ProjectSubSecArtificateDocumentCommentController(IUnitOfWork uow,
            IMapper mapper,
            IProjectSubSecArtificateDocumentCommentRepository projectSubSecArtificateDocumentCommentRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _mapper = mapper;
            _projectSubSecArtificateDocumentCommentRepository = projectSubSecArtificateDocumentCommentRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{documentId}")]
        public IActionResult Get(int documentId)
        {
            if (documentId <= 0) return BadRequest();
            var commentsDto = _projectSubSecArtificateDocumentCommentRepository.GetComments(documentId);
            return Ok(commentsDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectSubSecArtificateDocumentCommentDto projectSubSecArtificateDocumentCommentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            projectSubSecArtificateDocumentCommentDto.Id = 0;
            var projectSubSecArtificateDocumentComment = _mapper.Map<ProjectSubSecArtificateDocumentComment>(projectSubSecArtificateDocumentCommentDto);

            _projectSubSecArtificateDocumentCommentRepository.Add(projectSubSecArtificateDocumentComment);
            if (_uow.Save() <= 0) throw new Exception("Creating Comment failed on save.");
            return Ok(projectSubSecArtificateDocumentComment.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectSubSecArtificateDocumentCommentDto projectSubSecArtificateDocumentCommentDto)
        {
            if (projectSubSecArtificateDocumentCommentDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (!string.IsNullOrEmpty(projectSubSecArtificateDocumentCommentDto.Response) && projectSubSecArtificateDocumentCommentDto.ResponseDate == null)
            {
                projectSubSecArtificateDocumentCommentDto.ResponseBy = _jwtTokenAccesser.UserId;
                projectSubSecArtificateDocumentCommentDto.ResponseDate = DateTime.Now;
            }

            if (projectSubSecArtificateDocumentCommentDto.IsClose)
            {
                projectSubSecArtificateDocumentCommentDto.CloseBy = _jwtTokenAccesser.UserId;
                projectSubSecArtificateDocumentCommentDto.CloseDate = DateTime.Now;
            }

            var projectSubSecArtificateDocumentComment = _mapper.Map<ProjectSubSecArtificateDocumentComment>(projectSubSecArtificateDocumentCommentDto);
            _projectSubSecArtificateDocumentCommentRepository.Update(projectSubSecArtificateDocumentComment);

            if (_uow.Save() <= 0) throw new Exception("Updating Response failed on save.");
            return Ok(projectSubSecArtificateDocumentComment.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectSubSecArtificateDocumentCommentRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectSubSecArtificateDocumentCommentRepository.Delete(record);
            _uow.Save();
            return Ok();
        }
    }
}