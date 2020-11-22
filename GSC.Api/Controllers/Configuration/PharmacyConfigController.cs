using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Configuration
{
    [Route("api/[controller]")]
    public class PharmacyConfigController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IPharmacyConfigRepository _pharmacyConfigRepository;
        private readonly IUnitOfWork _uow;

        public PharmacyConfigController(IPharmacyConfigRepository pharmacyConfigRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _pharmacyConfigRepository = pharmacyConfigRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var pharmacyConfig = _pharmacyConfigRepository.FindByInclude(
                    x => (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) &&
                         isDeleted ? x.DeletedDate != null : x.DeletedDate == null, x => x.VariableTemplate)
                .Select(x => new PharmacyConfigDto
                {
                    Id = x.Id,
                    FormId = x.FormId,
                    FormName = x.FormName,
                    IsDeleted = x.DeletedDate != null,
                    VariableTemplateId = x.VariableTemplateId,
                    TemplateName = x.VariableTemplate.TemplateName
                }).OrderByDescending(x => x.Id).ToList();
            return Ok(pharmacyConfig);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var pharmacyConfig = _pharmacyConfigRepository.Find(id);
            var pharmacyConfigDto = _mapper.Map<PharmacyConfigDto>(pharmacyConfig);
            return Ok(pharmacyConfigDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PharmacyConfigDto pharmacyConfigDto)
        {
            pharmacyConfigDto.FormName = Enum.GetValues(typeof(FormType))
                .Cast<FormType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).Where(x => x.Id == pharmacyConfigDto.FormId).FirstOrDefault().Value;

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            pharmacyConfigDto.Id = 0;
            var pharmacyConfig = _mapper.Map<PharmacyConfig>(pharmacyConfigDto);
            if (pharmacyConfig.FormId >= 0)
            {
                var pharmacyConfigForm = _pharmacyConfigRepository.All
                    .Where(x => x.FormId == pharmacyConfig.FormId && x.DeletedBy == null).FirstOrDefault();
                if (pharmacyConfigForm == null)
                {
                    _pharmacyConfigRepository.Add(pharmacyConfig);
                }
                else
                {
                    pharmacyConfigForm.VariableTemplateId = pharmacyConfigDto.VariableTemplateId;
                    var pharmacyConfigUpdate = _mapper.Map<PharmacyConfig>(pharmacyConfigForm);
                    _pharmacyConfigRepository.Update(pharmacyConfigUpdate);
                }

                if (_uow.Save() <= 0) throw new Exception("Creating Pharmacy Config failed on save.");
            }

            return Ok(pharmacyConfig.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PharmacyConfigDto pharmacyConfigDto)
        {
            pharmacyConfigDto.FormName = Enum.GetValues(typeof(FormType))
                .Cast<FormType>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).Where(x => x.Id == pharmacyConfigDto.FormId).FirstOrDefault().Value;
            if (pharmacyConfigDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var pharmacyConfig = _mapper.Map<PharmacyConfig>(pharmacyConfigDto);

            _pharmacyConfigRepository.Update(pharmacyConfig);
            if (_uow.Save() <= 0) throw new Exception("Updating Pharmacy Config failed on save.");
            return Ok(pharmacyConfig.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _pharmacyConfigRepository.Find(id);

            if (record == null)
                return NotFound();

            _pharmacyConfigRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _pharmacyConfigRepository.Find(id);

            if (record == null)
                return NotFound();
            _pharmacyConfigRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetTemplateByForm/{id}")]
        public IActionResult GetTemplateByForm(int id)
        {
            return Ok(_pharmacyConfigRepository.GetVariableTemplateByFormId(id));
        }
    }
}