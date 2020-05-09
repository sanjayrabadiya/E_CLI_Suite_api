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
    public class DocumentTypeController : BaseController
    {
        private readonly IDocumentTypeRepository _documentRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public DocumentTypeController(IDocumentTypeRepository documentRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _documentRepository = documentRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var documents = _documentRepository.FindBy(x =>x.IsDeleted == isDeleted).OrderByDescending(x => x.Id).ToList();
            var documentsDto = _mapper.Map<IEnumerable<DocumentTypeDto>>(documents).ToList();
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
            var document = _documentRepository.Find(id);
            var documentDto = _mapper.Map<DocumentTypeDto>(document);
            return Ok(documentDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] DocumentTypeDto documentDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            documentDto.Id = 0;
            var document = _mapper.Map<DocumentType>(documentDto);
            var validate = _documentRepository.Duplicate(document);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _documentRepository.Add(document);
            if (_uow.Save() <= 0) throw new Exception("Creating Document failed on save.");
            return Ok(document.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] DocumentTypeDto documentDto)
        {
            if (documentDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var document = _mapper.Map<DocumentType>(documentDto);
            var validate = _documentRepository.Duplicate(document);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(document.Id);
            document.Id = 0;
            _documentRepository.Add(document);

            if (_uow.Save() <= 0) throw new Exception("Updating Document failed on save.");
            return Ok(document.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _documentRepository.Find(id);

            if (record == null)
                return NotFound();

            _documentRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _documentRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _documentRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _documentRepository.Active(record);
            _uow.Save();

            return Ok();
        }


        [HttpGet]
        [Route("GetDocumentDropDown")]
        public IActionResult GetDocumentDropDown()
        {
            return Ok(_documentRepository.GetDocumentDropDown());
        }
    }
}