using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Respository.Project.Design;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProjectDesignTemplateNoteController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProjectDesignTemplateNoteRepository _projectDesignTemplateNoteRepository;

        public ProjectDesignTemplateNoteController(IProjectDesignTemplateNoteRepository projectDesignTemplateNoteRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _projectDesignTemplateNoteRepository = projectDesignTemplateNoteRepository;
            _uow = uow;
            _mapper = mapper;
        }


        [HttpGet]
        [Route("GetTemplateNoteList/{templateId}")]
        public IActionResult GetTemplateNoteList(int templateId)
        {
            var projectDesignTemplateNotes = _projectDesignTemplateNoteRepository.GetProjectDesignTemplateNoteList(templateId);
            return Ok(projectDesignTemplateNotes);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var projectDesignTemplateNote = _projectDesignTemplateNoteRepository.Find(id);
            var projectDesignTemplateNoteDto = _mapper.Map<ProjectDesignTemplateNoteDto>(projectDesignTemplateNote);
            return Ok(projectDesignTemplateNoteDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProjectDesignTemplateNoteDto projectDesignTemplateNoteDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            projectDesignTemplateNoteDto.Id = 0;
            var projectDesignTemplateNote = _mapper.Map<ProjectDesignTemplateNote>(projectDesignTemplateNoteDto);
           
            _projectDesignTemplateNoteRepository.Add(projectDesignTemplateNote);
            if (_uow.Save() <= 0) throw new Exception("Creating Test Group failed on save.");
            return Ok(projectDesignTemplateNote.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProjectDesignTemplateNoteDto projectDesignTemplateNoteDto)
        {
            if (projectDesignTemplateNoteDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectDesignTemplateNote = _mapper.Map<ProjectDesignTemplateNote>(projectDesignTemplateNoteDto);

            _projectDesignTemplateNoteRepository.Update(projectDesignTemplateNote);

            if (_uow.Save() <= 0) throw new Exception("Updating Test Group failed on save.");
            return Ok(projectDesignTemplateNote.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _projectDesignTemplateNoteRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectDesignTemplateNoteRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _projectDesignTemplateNoteRepository.Find(id);

            if (record == null)
                return NotFound();

            _projectDesignTemplateNoteRepository.Active(record);
            _uow.Save();

            return Ok();
        }



    }
}
