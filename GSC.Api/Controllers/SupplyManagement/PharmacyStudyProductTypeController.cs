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
    public class PharmacyStudyProductTypeController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IPharmacyStudyProductTypeRepository _pharmacyStudyProductTypeRepository;
        private readonly IProductReceiptRepository _productReceiptRepository;
        private readonly IUnitOfWork _uow;

        public PharmacyStudyProductTypeController(IPharmacyStudyProductTypeRepository pharmacyStudyProductTypeRepository,
            IProductReceiptRepository productReceiptRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _pharmacyStudyProductTypeRepository = pharmacyStudyProductTypeRepository;
            _productReceiptRepository = productReceiptRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("GetPharmacyStudyProductTypeList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetPharmacyStudyProductTypeList(int projectId, bool isDeleted)
        {
            var productType = _pharmacyStudyProductTypeRepository.GetPharmacyStudyProductTypeList(projectId, isDeleted);
            return Ok(productType);
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

            var result = _productReceiptRepository.StudyProductTypeAlreadyUse(pharmacyStudyProductTypeDto.Id);
            if (!string.IsNullOrEmpty(result))
            {
                ModelState.AddModelError("Message", result);
                return BadRequest(ModelState);
            }

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

            var validate = _productReceiptRepository.StudyProductTypeAlreadyUse(id);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }


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
