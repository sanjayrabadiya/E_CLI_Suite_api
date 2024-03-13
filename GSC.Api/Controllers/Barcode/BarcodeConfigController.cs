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
    public class BarcodeConfigController : BaseController
    {
        private readonly IBarcodeConfigRepository _barcodeConfigRepository;
        private readonly IBarcodeDisplayInfoRepository _barcodeDisplayInfoRepository;
        private readonly IBarcodeCombinationRepository _barcodeCombinationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public BarcodeConfigController(IBarcodeConfigRepository barcodeConfigRepository,
            IBarcodeDisplayInfoRepository barcodeDisplayInfoRepository,
            IBarcodeCombinationRepository barcodeCombinationRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _barcodeConfigRepository = barcodeConfigRepository;
            _barcodeDisplayInfoRepository = barcodeDisplayInfoRepository;
            _barcodeCombinationRepository = barcodeCombinationRepository;
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
        public IActionResult Post([FromBody] BarcodeConfigDto barcodeConfigDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            barcodeConfigDto.Id = 0;
            var barcodeConfig = _mapper.Map<BarcodeConfig>(barcodeConfigDto);

            _barcodeConfigRepository.Add(barcodeConfig);

            var ordNo = 0;
            foreach (var item in barcodeConfig.BarcodeDisplayInfo)
            {
                item.OrderNumber = ++ordNo;
                _barcodeDisplayInfoRepository.Add(item);
            }

            foreach (var item in barcodeConfig.BarcodeCombination)
            {
                _barcodeCombinationRepository.Add(item);
            }

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating barcode config failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(barcodeConfig.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] BarcodeConfigDto barcodeConfigDto)
        {
            if (barcodeConfigDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var barcodeConfig = _mapper.Map<BarcodeConfig>(barcodeConfigDto);

            _barcodeConfigRepository.Update(barcodeConfig);

            UpdateBarcodeDisplayInformaition(barcodeConfig);
            UpdateBarcodeCombinationInformation(barcodeConfig);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating barcode config failed on save.");
                return BadRequest(ModelState);
            }
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

        private void UpdateBarcodeDisplayInformaition(BarcodeConfig barcodeConfig)
        {
            var data = _barcodeDisplayInfoRepository.FindBy(x => x.BarcodConfigId == barcodeConfig.Id).ToList();
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

        private void UpdateBarcodeCombinationInformation(BarcodeConfig barcodeConfig)
        {
            var data = _barcodeCombinationRepository.FindBy(x => x.BarcodConfigId == barcodeConfig.Id).ToList();
            var deletevalues = data.Where(t => barcodeConfig.BarcodeCombination.Where(a => a.Id == t.Id).ToList().Count <= 0).ToList();
            var addvalues = barcodeConfig.BarcodeCombination.Where(x => x.Id == 0).ToList();
            var updatevalues = barcodeConfig.BarcodeCombination.Where(x => x.Id != 0).ToList();

            foreach (var value in deletevalues)
                _barcodeCombinationRepository.Remove(value);

            foreach (var item in addvalues)
            {
                _barcodeCombinationRepository.Add(item);
            }

            foreach (var value in updatevalues)
                _barcodeCombinationRepository.Update(value);
        }

    }
}