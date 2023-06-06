using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class VendorManagementController : BaseController
    {
        private readonly IVendorManagementRepository _vendormanagementRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public VendorManagementController(IVendorManagementRepository vendormanagementRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _vendormanagementRepository = vendormanagementRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var vendors = _vendormanagementRepository.GetVendorManagementList(isDeleted);
            return Ok(vendors);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var vendor = _vendormanagementRepository.Find(id);
            var vendorDto = _mapper.Map<VendorManagementDto>(vendor);
            return Ok(vendorDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VendorManagementDto vendorDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            vendorDto.Id = 0;
            var vendor = _mapper.Map<VendorManagement>(vendorDto);
            var validate = _vendormanagementRepository.Duplicate(vendor);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _vendormanagementRepository.Add(vendor);
            if (_uow.Save() <= 0) throw new Exception("Creating Vendor failed on save.");
            return Ok(vendor.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] VendorManagementDto vendorDto)
        {
            if (vendorDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var vendor = _mapper.Map<VendorManagement>(vendorDto);
            var validate = _vendormanagementRepository.Duplicate(vendor);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _vendormanagementRepository.AddOrUpdate(vendor);

            if (_uow.Save() <= 0) throw new Exception("Updating Vendor failed on save.");
            return Ok(vendor.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _vendormanagementRepository.Find(id);

            if (record == null)
                return NotFound();

            _vendormanagementRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _vendormanagementRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _vendormanagementRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _vendormanagementRepository.Active(record);
            _uow.Save();
            return Ok();
        }


        [HttpGet]
        [Route("GetVendorDropDown")]
        public IActionResult GetVendorDropDown()
        {
            return Ok(_vendormanagementRepository.GetVendorDropDown());
        }
    }
}