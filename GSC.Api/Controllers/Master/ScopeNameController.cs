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
    public class ScopeNameController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IScopeNameRepository _scopeNameRepository;
        private readonly IUnitOfWork _uow;

        public ScopeNameController(IScopeNameRepository scopeNameRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _scopeNameRepository = scopeNameRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var scopeName = _scopeNameRepository.All.Where(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).Select(x => _mapper.Map<ScopeNameDto>(x)).OrderByDescending(x => x.Id);

            var scopeNamesDto = _mapper.Map<IEnumerable<ScopeNameDto>>(scopeName);
            scopeNamesDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(scopeNamesDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get([FromRoute] int id)
        {
            var scopeName = _scopeNameRepository.Find(id);

            return Ok(_mapper.Map<ScopeNameDto>(scopeName));
        }

        [HttpPost]
        public IActionResult Post([FromBody] ScopeNameDto dto)
        {
            dto.Id = 0;
            var scopeName = _mapper.Map<ScopeName>(dto);
            var validate = _scopeNameRepository.Duplicate(scopeName);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _scopeNameRepository.Add(scopeName);
            _uow.Save();
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] ScopeNameDto dto)
        {
            var scopeName = _mapper.Map<ScopeName>(dto);
            var validate = _scopeNameRepository.Duplicate(scopeName);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _scopeNameRepository.AddOrUpdate(scopeName);

            if (_uow.Save() <= 0) throw new Exception("Updating scope failed on save.");

            return Ok(scopeName.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _scopeNameRepository.Find(id);

            if (record == null)
                return NotFound();

            _scopeNameRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _scopeNameRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _scopeNameRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _scopeNameRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetScopeNameDropDown")]
        public IActionResult GetScopeNameDropDown()
        {
            return Ok(_scopeNameRepository.GetScopeNameDropDown());
        }
    }
}