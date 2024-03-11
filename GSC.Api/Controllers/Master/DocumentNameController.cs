using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DocumentNameController : BaseController
    {
        private readonly IDocumentNameRepository _documentNameRepository;
        private readonly IDocumentTypeRepository _documentTypeRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public DocumentNameController(IDocumentNameRepository documentNameRepository,
            IDocumentTypeRepository documentTypeRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _documentNameRepository = documentNameRepository;
            _documentTypeRepository = documentTypeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var documents = _documentNameRepository.GetDocumentNameList(isDeleted);
            documents.ForEach(b =>
            {
                b.DocumentType = _documentTypeRepository.Find(b.DocumentTypeId);
            });
            return Ok(documents);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var document = _documentNameRepository.Find(id);
            var documentDto = _mapper.Map<DocumentNameDto>(document);
            return Ok(documentDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] DocumentNameDto documentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            documentDto.Id = 0;
            var document = _mapper.Map<DocumentName>(documentDto);
            var validate = _documentNameRepository.Duplicate(document);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _documentNameRepository.Add(document);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Document failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(document.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] DocumentNameDto documentDto)
        {
            if (documentDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var document = _mapper.Map<DocumentName>(documentDto);
            var validate = _documentNameRepository.Duplicate(document);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _documentNameRepository.AddOrUpdate(document);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Document failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(document.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _documentNameRepository.Find(id);

            if (record == null)
                return NotFound();

            _documentNameRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _documentNameRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _documentNameRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _documentNameRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetDocumentDropDown/{documentId}")]
        public IActionResult GetDocumentDropDown(int documentId)
        {
            return Ok(_documentNameRepository.GetDocumentDropDown(documentId));
        }
    }
}