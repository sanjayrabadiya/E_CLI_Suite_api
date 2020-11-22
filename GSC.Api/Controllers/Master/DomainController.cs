using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.UserMgt;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DomainController : BaseController
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IUnitOfWork _uow;

        public DomainController(IDomainRepository domainRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IProjectDesignRepository projectDesignRepository)
        {
            _domainRepository = domainRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectDesignRepository = projectDesignRepository;
        }


        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var domains = _domainRepository.GetDomainList(isDeleted);

            return Ok(domains);

        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var domain = _domainRepository.Find(id);
            var domainDto = _mapper.Map<DomainDto>(domain);
            return Ok(domainDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] DomainDto domainDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            domainDto.Id = 0;
            var domain = _mapper.Map<Data.Entities.Master.Domain>(domainDto);

            var validateMessage = _domainRepository.ValidateDomain(domain);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            _domainRepository.Add(domain);
            if (_uow.Save() <= 0) throw new Exception("Creating Domain failed on save.");

            return Ok(domain.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] DomainDto domainDto)
        {
            if (domainDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var domain = _mapper.Map<Data.Entities.Master.Domain>(domainDto);
            domain.Id = domainDto.Id;

            var validateMessage = _domainRepository.ValidateDomain(domain);
            if (!string.IsNullOrEmpty(validateMessage))
            {
                ModelState.AddModelError("Message", validateMessage);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */          
            _domainRepository.AddOrUpdate(domain);

            if (_uow.Save() <= 0) throw new Exception("Updating Domain failed on save.");

            return Ok(domain.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _domainRepository.Find(id);

            if (record == null)
                return NotFound();

            _domainRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _domainRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _domainRepository.ValidateDomain(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _domainRepository.Active(record);
            _uow.Save();

            return Ok();
        }


        [HttpGet]
        [Route("GetDomainDropDown")]
        public IActionResult GetDomainDropDown()
        {
            return Ok(_domainRepository.GetDomainDropDown());
        }

        [HttpGet]
        [Route("GetDomainByProjectDesignDropDown/{projectDesignId}")]
        public IActionResult GetDomainByProjectDesignDropDown(int projectDesignId)
        {
            return Ok(_domainRepository.GetDomainByProjectDesignDropDown(projectDesignId));
        }

        [HttpGet]
        [Route("GetDomainByProjectDropDown/{projectId}")]
        public IActionResult GetDomainByProjectDropDown(int projectId)
        {
            var projectDesignId = _projectDesignRepository
                .FindBy(x => x.ProjectId == projectId && x.DeletedDate == null).FirstOrDefault();
            return Ok(_domainRepository.GetDomainByProjectDesignDropDown(projectDesignId.Id));
        }
    }
}