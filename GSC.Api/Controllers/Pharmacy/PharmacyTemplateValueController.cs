using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Pharmacy;
using GSC.Data.Entities.Pharmacy;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.Pharmacy;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Pharmacy
{
    [Route("api/[controller]")]
    public class PharmacyTemplateValueController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPharmacyTemplateValueRepository _pharmacyTemplateValueRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public PharmacyTemplateValueController(IPharmacyTemplateValueRepository pharmacyTemplateValueRepository,
            IUploadSettingRepository uploadSettingRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _pharmacyTemplateValueRepository = pharmacyTemplateValueRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var pharmacyTemplateValue = _pharmacyTemplateValueRepository.Find(id);

            var pharmacyTemplateValueDto = _mapper.Map<PharmacyTemplateValueDto>(pharmacyTemplateValue);
            return Ok(pharmacyTemplateValueDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PharmacyTemplateValueDto pharmacyTemplateValueDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var pharmacyTemplateValue = _mapper.Map<PharmacyTemplateValue>(pharmacyTemplateValueDto);
            pharmacyTemplateValue.Id = 0;


            _pharmacyTemplateValueRepository.Add(pharmacyTemplateValue);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating pharmacy Value failed on save.");
                return BadRequest(ModelState);
            }

            pharmacyTemplateValueDto = _mapper.Map<PharmacyTemplateValueDto>(pharmacyTemplateValue);

            return Ok(pharmacyTemplateValueDto);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PharmacyTemplateValueDto pharmacyTemplateValueDto)
        {
            if (pharmacyTemplateValueDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var pharmacyTemplateValue = _mapper.Map<PharmacyTemplateValue>(pharmacyTemplateValueDto);


            _pharmacyTemplateValueRepository.Update(pharmacyTemplateValue);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating pharmacy Value failed on save.");
                return BadRequest(ModelState);
            }

            pharmacyTemplateValueDto = _mapper.Map<PharmacyTemplateValueDto>(pharmacyTemplateValue);

            return Ok(pharmacyTemplateValueDto);
        }

        [HttpPut("UploadDocument")]
        public IActionResult UploadDocument([FromBody] PharmacyTemplateValueDto pharmacyTemplateValueDto)
        {
            if (pharmacyTemplateValueDto.Id <= 0) return BadRequest();

            var pharmacyTemplateValue = _pharmacyTemplateValueRepository.Find(pharmacyTemplateValueDto.Id);

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Uploading document failed on save.");
                return BadRequest(ModelState);
            }

            pharmacyTemplateValueDto.DocPath = documentUrl + pharmacyTemplateValue.DocPath;

            return Ok(pharmacyTemplateValueDto);
        }

    }
}