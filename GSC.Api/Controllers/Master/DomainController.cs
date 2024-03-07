using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DomainController : BaseController
    {
        private readonly IDomainRepository _domainRepository;
        private readonly IMapper _mapper;
        private readonly IProjectDesignRepository _projectDesignRepository;
        private readonly IVariableRepository _variableRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public DomainController(IDomainRepository domainRepository,
            IVariableTemplateRepository variableTemplateRepository,
            IUnitOfWork uow, IMapper mapper, IGSCContext context,
            IProjectDesignRepository projectDesignRepository,
            IVariableRepository variableRepository)
        {
            _domainRepository = domainRepository;
            _uow = uow;
            _mapper = mapper;
            _projectDesignRepository = projectDesignRepository;
            _variableRepository = variableRepository;
            _variableTemplateRepository = variableTemplateRepository;
            _context = context;
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
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Domain failed on save.");
                return BadRequest(ModelState);
            }

            return Ok(domain.Id);
        }

        // PUT api/<controller>/5
        [HttpPut]
        public IActionResult Put([FromBody] DomainDto domainDto)
        {
            if (domainDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var lastDomain = _domainRepository.Find(domainDto.Id);
            if (lastDomain.DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't edit record!");
                return BadRequest(ModelState);
            }

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

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Domain failed on save.");
                return BadRequest(ModelState);
            }

            return Ok(domain.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _domainRepository.Find(id);

            if (record == null)
                return NotFound();

            if (record.DomainCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }

            _domainRepository.Delete(record);

            var variable = _variableRepository.FindByInclude(x => x.DomainId == id).ToList();
            variable.ForEach(variable =>
            {
                _variableRepository.Delete(variable);
            });

            var variableTemplate = _variableTemplateRepository.FindByInclude(x => x.DomainId == id).ToList();
            variableTemplate.ForEach(temp =>
            {
                _variableTemplateRepository.Delete(temp);
            });

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

            var domainClass = _context.DomainClass.Where(x => x.Id == record.DomainClassId).FirstOrDefault();
            if (domainClass?.DeletedDate != null)
            {
                var message = "Domain Class Is Deacitvated.";
                ModelState.AddModelError("Message", message);
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
            return Ok(_domainRepository.GetDomainByProjectDesignDropDown(projectDesignId?.Id ?? 0));
        }

        [HttpGet]
        [Route("GetDomainByCRFDropDown/{isNonCRF:bool?}")]
        public IActionResult GetDomainByCRFDropDown(bool isNonCRF)
        {
            return Ok(_domainRepository.GetDomainByCRFDropDown(isNonCRF));
        }
    }
}