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
    public class ClientTypeController : BaseController
    {
        private readonly IClientTypeRepository _clientTypeRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;

        public ClientTypeController(IClientTypeRepository clientTypeRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork<GscContext> uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _clientTypeRepository = clientTypeRepository;
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
          
            var clientTypess = _clientTypeRepository.FindByInclude(x =>  x.IsDeleted == isDeleted).OrderByDescending(x => x.Id).ToList();


            var clientTypessDto = _mapper.Map<IEnumerable<ClientTypeDto>>(clientTypess).ToList();

            clientTypessDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(clientTypessDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var clientTypes = _clientTypeRepository.Find(id);
            var clientTypesDto = _mapper.Map<ClientTypeDto>(clientTypes);
            return Ok(clientTypesDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ClientTypeDto clientTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            clientTypeDto.Id = 0;
            var clientType = _mapper.Map<ClientType>(clientTypeDto);
            var validate = _clientTypeRepository.Duplicate(clientType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _clientTypeRepository.Add(clientType);
            if (_uow.Save() <= 0) throw new Exception("Creating Client Type failed on save.");
            return Ok(clientType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ClientTypeDto clientTypeDto)
        {
            if (clientTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var clientType = _mapper.Map<ClientType>(clientTypeDto);
            var validate = _clientTypeRepository.Duplicate(clientType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _clientTypeRepository.AddOrUpdate(clientType);

            if (_uow.Save() <= 0) throw new Exception("Updating Client Type failed on save.");
            return Ok(clientType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _clientTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _clientTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _clientTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _clientTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _clientTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetClientTypeDropDown")]
        public IActionResult GetClientTypeDropDown()
        {
            return Ok(_clientTypeRepository.GetClientTypeDropDown());
        }
    }
}