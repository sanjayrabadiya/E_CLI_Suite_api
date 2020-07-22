using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class VariableTemplateNoteController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVariableTemplateNoteRepository _variableTemplateNoteRepository;

        public VariableTemplateNoteController(IVariableTemplateNoteRepository variableTemplateNoteRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _variableTemplateNoteRepository = variableTemplateNoteRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpPost]
        public IActionResult Post([FromBody] VariableTemplateNoteDto variableTemplateNoteDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            variableTemplateNoteDto.Id = 0;

            var variableTemplateNote = _mapper.Map<VariableTemplateNote>(variableTemplateNoteDto);

            _variableTemplateNoteRepository.Add(variableTemplateNote);

            if (_uow.Save() <= 0) throw new Exception("Creating Variable Template Note failed on save.");
            return Ok(variableTemplateNote.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VariableTemplateNoteDto variableTemplateNoteDto)
        {
            if (variableTemplateNoteDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var variableTemplateNote = _mapper.Map<VariableTemplateNote>(variableTemplateNoteDto);

            _variableTemplateNoteRepository.Update(variableTemplateNote);
            if (_uow.Save() <= 0) throw new Exception("Updating Variable Template Note failed on save.");
            return Ok(variableTemplateNote.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _variableTemplateNoteRepository.Find(id);

            if (record == null)
                return NotFound();

            _variableTemplateNoteRepository.Delete(record);
            _uow.Save();

            return Ok();
        }
    }
}