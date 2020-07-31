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
    public class DomainClassController : BaseController
    {
        private readonly IDomainClassRepository _domainClassRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public DomainClassController(IDomainClassRepository domainClassRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _domainClassRepository = domainClassRepository;
            _uow = uow;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var domainClasss = _domainClassRepository.All.Where(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var domainClasssDto = _mapper.Map<IEnumerable<DomainClassDto>>(domainClasss);

            //domainClasssDto.ForEach(b =>
            //{
            //    b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
            //    if (b.ModifiedBy != null)
            //        b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
            //    if (b.DeletedBy != null)
            //        b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
            //    if (b.CompanyId != null)
            //        b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            //});
            return Ok(domainClasssDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var domainClass = _domainClassRepository.Find(id);
            var domainClassDto = _mapper.Map<DomainClassDto>(domainClass);
            return Ok(domainClassDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] DomainClassDto domainClassDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            domainClassDto.Id = 0;
            var domainClass = _mapper.Map<DomainClass>(domainClassDto);

            var validateMessage = _domainClassRepository.ValidateDomainClass(domainClass);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            _domainClassRepository.Add(domainClass);
            if (_uow.Save() <= 0) throw new Exception("Creating Domain Class failed on save.");
            return Ok(domainClass.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] DomainClassDto domainClassDto)
        {
            if (domainClassDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var domainClass = _mapper.Map<DomainClass>(domainClassDto);
            domainClass.Id = domainClassDto.Id;

            var validateMessage = _domainClassRepository.ValidateDomainClass(domainClass);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _domainClassRepository.AddOrUpdate(domainClass);

            if (_uow.Save() <= 0) throw new Exception("Updating Domain Class failed on save.");
            return Ok(domainClass.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _domainClassRepository.Find(id);

            if (record == null)
                return NotFound();

            _domainClassRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _domainClassRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _domainClassRepository.ValidateDomainClass(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _domainClassRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetDomainClassDropDown")]
        public IActionResult GetDomainClassDropDown()
        {
            return Ok(_domainClassRepository.GetDomainClassDropDown());
        }
    }
}