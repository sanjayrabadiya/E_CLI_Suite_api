using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Barcode;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Barcode
{
    [Route("api/[controller]")]
    public class BarcodeTypeController : BaseController
    {
        private readonly IBarcodeTypeRepository _barcodeTypeRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public BarcodeTypeController(IBarcodeTypeRepository barcodeTypeRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _barcodeTypeRepository = barcodeTypeRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var barcodeTypes = _barcodeTypeRepository
                .All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
                ).OrderByDescending(x => x.Id).ToList();
            var barcodeTypesDto = _mapper.Map<IEnumerable<BarcodeTypeDto>>(barcodeTypes);
            return Ok(barcodeTypesDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var barcodeType = _barcodeTypeRepository.Find(id);
            var barcodeTypeDto = _mapper.Map<BarcodeTypeDto>(barcodeType);
            return Ok(barcodeTypeDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] BarcodeTypeDto barcodeTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            barcodeTypeDto.Id = 0;
            var barcodeType = _mapper.Map<BarcodeType>(barcodeTypeDto);
            var validate = _barcodeTypeRepository.Duplicate(barcodeType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _barcodeTypeRepository.Add(barcodeType);
            if (_uow.Save() <= 0) throw new Exception("Creating barcode type failed on save.");
            return Ok(barcodeType.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] BarcodeTypeDto barcodeTypeDto)
        {
            if (barcodeTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var barcodeType = _mapper.Map<BarcodeType>(barcodeTypeDto);
            var validate = _barcodeTypeRepository.Duplicate(barcodeType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(barcodeType.Id);
            barcodeType.Id = 0;
            _barcodeTypeRepository.Add(barcodeType);

            if (_uow.Save() <= 0) throw new Exception("Updating barcode type failed on save.");
            return Ok(barcodeType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _barcodeTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _barcodeTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _barcodeTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _barcodeTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _barcodeTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetBarcodeTypeDropDown")]
        public IActionResult GetBarcodeTypeDropDown()
        {
            return Ok(_barcodeTypeRepository.GetBarcodeTypeDropDown());
        }

        //[HttpGet]
        //[Route("GetBarcodeSizeDropDown")]
        //public List<DropDownDto> GetBarcodeSizeDropDown()
        //{
        //    return _gscContext.BarcodeSize.Where(x =>
        //            (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null)
        //        .Select(c => new DropDownDto { Id = c.Id, Value = c.Name }).OrderBy(o => o.Value).ToList();

        //}
    }
}