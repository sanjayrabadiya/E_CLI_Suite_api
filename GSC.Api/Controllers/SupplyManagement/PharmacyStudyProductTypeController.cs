using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.SupplyManagement;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class PharmacyStudyProductTypeController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IPharmacyStudyProductTypeRepository _pharmacyStudyProductTypeRepository;
        private readonly IUnitOfWork _uow;

        public PharmacyStudyProductTypeController(IPharmacyStudyProductTypeRepository pharmacyStudyProductTypeRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _pharmacyStudyProductTypeRepository = pharmacyStudyProductTypeRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var productTypes = _pharmacyStudyProductTypeRepository.GetPharmacyStudyProductTypeList(isDeleted);
            return Ok(productTypes);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PharmacyStudyProductTypeDto pharmacyStudyProductTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            pharmacyStudyProductTypeDto.Id = 0;
            var pharmacyStudyProductType = _mapper.Map<PharmacyStudyProductType>(pharmacyStudyProductTypeDto);
            var validate = _pharmacyStudyProductTypeRepository.Duplicate(pharmacyStudyProductType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _pharmacyStudyProductTypeRepository.Add(pharmacyStudyProductType);
            if (_uow.Save() <= 0) throw new Exception("Creating pharmacy study product type failed on save.");
            return Ok(pharmacyStudyProductType.Id);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var pharmacyStudyProductType = _pharmacyStudyProductTypeRepository.Find(id);
            var pharmacyStudyProductTypeDto = _mapper.Map<PharmacyStudyProductTypeDto>(pharmacyStudyProductType);
            return Ok(pharmacyStudyProductTypeDto);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PharmacyStudyProductTypeDto pharmacyStudyProductTypeDto)
        {
            if (pharmacyStudyProductTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var pharmacyStudyProductType = _mapper.Map<PharmacyStudyProductType>(pharmacyStudyProductTypeDto);
            var validate = _pharmacyStudyProductTypeRepository.Duplicate(pharmacyStudyProductType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _pharmacyStudyProductTypeRepository.AddOrUpdate(pharmacyStudyProductType);

            if (_uow.Save() <= 0) throw new Exception("Updating pharmacy study product type failed on save.");
            return Ok(pharmacyStudyProductType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _pharmacyStudyProductTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _pharmacyStudyProductTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _pharmacyStudyProductTypeRepository.Find(id);

            if (record == null)
                return NotFound();
            var validate = _pharmacyStudyProductTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _pharmacyStudyProductTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetPharmacyStudyProductTypeDropDown/{projectId}")]
        public IActionResult GetPharmacyStudyProductTypeDropDown(int projectId)
        {
            return Ok(_pharmacyStudyProductTypeRepository.GetPharmacyStudyProductTypeDropDown(projectId));
        }
    }
}
