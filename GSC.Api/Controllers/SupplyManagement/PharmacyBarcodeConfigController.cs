using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Respository.Barcode;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    public class PharmacyBarcodeConfigController : BaseController
    {
        private readonly IPharmacyBarcodeConfigRepository _barcodeConfigRepository;
        private readonly IPharmacyBarcodeDisplayInfoRepository _barcodeDisplayInfoRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PharmacyBarcodeConfigController(IPharmacyBarcodeConfigRepository barcodeConfigRepository,
            IPharmacyBarcodeDisplayInfoRepository barcodeDisplayInfoRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _barcodeConfigRepository = barcodeConfigRepository;
            _barcodeDisplayInfoRepository = barcodeDisplayInfoRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            return Ok(_barcodeConfigRepository.GetBarcodeConfig(isDeleted));
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var barcodeConfig = _barcodeConfigRepository.GetBarcodeConfigById(id);
            return Ok(barcodeConfig);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PharmacyBarcodeConfigDto barcodeConfigDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            barcodeConfigDto.Id = 0;
            var barcodeConfig = _mapper.Map<PharmacyBarcodeConfig>(barcodeConfigDto);
            var message = _barcodeConfigRepository.ValidateBarcodeConfig(barcodeConfig);
            if (!string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("Message", message);
                return BadRequest(ModelState);
            }
            _barcodeConfigRepository.Add(barcodeConfig);

            var ordNo = 0;
            foreach (var item in barcodeConfig.BarcodeDisplayInfo)
            {
                item.OrderNumber = ++ordNo;
                _barcodeDisplayInfoRepository.Add(item);
            }


            if (_uow.Save() <= 0) throw new Exception("Creating barcode config failed on save.");
            return Ok(barcodeConfig.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] PharmacyBarcodeConfigDto barcodeConfigDto)
        {
            if (barcodeConfigDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var barcodeConfig = _mapper.Map<PharmacyBarcodeConfig>(barcodeConfigDto);

            var message = _barcodeConfigRepository.ValidateBarcodeConfig(barcodeConfig);
            if (!string.IsNullOrEmpty(message))
            {
                ModelState.AddModelError("Message", message);
                return BadRequest(ModelState);
            }

            _barcodeConfigRepository.Update(barcodeConfig);

            UpdateBarcodeDisplayInformaition(barcodeConfig);

            if (_uow.Save() <= 0) throw new Exception("Updating barcode config failed on save.");
            return Ok(barcodeConfig.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _barcodeConfigRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeConfigRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _barcodeConfigRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeConfigRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        private void UpdateBarcodeDisplayInformaition(PharmacyBarcodeConfig barcodeConfig)
        {
            var data = _barcodeDisplayInfoRepository.FindBy(x => x.PharmacyBarcodeConfigId == barcodeConfig.Id).ToList();
            var deletevalues = data.Where(t => barcodeConfig.BarcodeDisplayInfo.Where(a => a.Id == t.Id).ToList().Count <= 0).ToList();
            var addvalues = barcodeConfig.BarcodeDisplayInfo.Where(x => x.Id == 0).ToList();
            var updatevalues = barcodeConfig.BarcodeDisplayInfo.Where(x => x.Id != 0).ToList();

            foreach (var value in deletevalues)
                _barcodeDisplayInfoRepository.Remove(value);

            var ordNo = data.Count;
            foreach (var item in addvalues)
            {
                item.OrderNumber = ++ordNo;
                _barcodeDisplayInfoRepository.Add(item);
            }

            foreach (var value in updatevalues)
                _barcodeDisplayInfoRepository.Update(value);
        }



    }
}