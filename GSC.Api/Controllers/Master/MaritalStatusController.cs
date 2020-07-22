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
    public class MaritalStatusController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMaritalStatusRepository _maritalStatusRepository;
        private readonly IUnitOfWork _uow;

        public MaritalStatusController(IMaritalStatusRepository maritalStatusRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _maritalStatusRepository = maritalStatusRepository;
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
            var maritalStatuss = _maritalStatusRepository.All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var maritalStatussDto = _mapper.Map<IEnumerable<MaritalStatusDto>>(maritalStatuss);
            maritalStatussDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(maritalStatussDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var maritalStatus = _maritalStatusRepository.Find(id);
            var maritalStatusDto = _mapper.Map<MaritalStatusDto>(maritalStatus);
            return Ok(maritalStatusDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] MaritalStatusDto maritalStatusDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            maritalStatusDto.Id = 0;
            var maritalStatus = _mapper.Map<MaritalStatus>(maritalStatusDto);
            var validate = _maritalStatusRepository.Duplicate(maritalStatus);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _maritalStatusRepository.Add(maritalStatus);
            if (_uow.Save() <= 0) throw new Exception("Creating Marital Status failed on save.");
            return Ok(maritalStatus.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] MaritalStatusDto maritalStatusDto)
        {
            if (maritalStatusDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var maritalStatus = _mapper.Map<MaritalStatus>(maritalStatusDto);
            var validate = _maritalStatusRepository.Duplicate(maritalStatus);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _maritalStatusRepository.AddOrUpdate(maritalStatus);

            if (_uow.Save() <= 0) throw new Exception("Updating Marital Status failed on save.");
            return Ok(maritalStatus.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _maritalStatusRepository.Find(id);

            if (record == null)
                return NotFound();

            _maritalStatusRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _maritalStatusRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _maritalStatusRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _maritalStatusRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetMaritalStatusDropDown")]
        public IActionResult GetMaritalStatusDropDown()
        {
            return Ok(_maritalStatusRepository.GetMaritalStatusDropDown());
        }
    }
}