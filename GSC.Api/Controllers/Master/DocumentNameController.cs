using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DocumentNameController : BaseController
    {
        private readonly IDocumentNameRepository _documentNameRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public DocumentNameController(IDocumentNameRepository documentNameRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _documentNameRepository = documentNameRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var documents = _documentNameRepository.FindByInclude(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null
                , t => t.DocumentType).OrderByDescending(x => x.Id).ToList();
            var documentsDto = _mapper.Map<IEnumerable<DocumentNameDto>>(documents);
            documentsDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(documentsDto);
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
            if (_uow.Save() <= 0) throw new Exception("Creating Document failed on save.");
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

            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
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