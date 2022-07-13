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
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class DomainClassController : BaseController
    {
        private readonly IDomainClassRepository _domainClassRepository;
        private readonly IDomainRepository _domainRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IVariableRepository _variableRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;

        public DomainClassController(IDomainClassRepository domainClassRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IDomainRepository domainRepository,
            IVariableRepository variableRepository,
            IVariableTemplateRepository variableTemplateRepository)
        {
            _domainClassRepository = domainClassRepository;
            _uow = uow;
            _domainRepository = domainRepository;
            _variableRepository = variableRepository;
            _companyRepository = companyRepository;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _variableTemplateRepository = variableTemplateRepository;
        }


        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var domainClasss = _domainClassRepository.GetDomainClassList(isDeleted);
            return Ok(domainClasss);
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

            var lastDomainClass = _domainClassRepository.Find(domainClassDto.Id);
            if (lastDomainClass.DomainClassCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't edit record!");
                return BadRequest(ModelState);
            }

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

            if (record.DomainClassCode == ScreeningFitnessFit.FitnessFit.GetDescription())
            {
                ModelState.AddModelError("Message", "Can't delete record!");
                return BadRequest(ModelState);
            }

            _domainClassRepository.Delete(record);

            var domain = _domainRepository.FindByInclude(x => x.DomainClassId == id).ToList();
            domain.ForEach(z =>
            {
                _domainRepository.Delete(z);

                var variable = _variableRepository.FindByInclude(x => x.DomainId == z.Id).ToList();

                variable.ForEach(variable =>
                {
                    _variableRepository.Delete(variable);
                });

                var variableTemplate = _variableTemplateRepository.FindByInclude(x => x.DomainId == z.Id).ToList();

                variableTemplate.ForEach(temp =>
                {
                    _variableTemplateRepository.Delete(temp);
                });
            });


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