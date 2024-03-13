using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class CurrencyController : BaseController
    {
        private readonly ICurrencyRepository _currencyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public CurrencyController(ICurrencyRepository currencyRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _currencyRepository = currencyRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var currency = _currencyRepository.GetCurrencyList(isDeleted);
            return Ok(currency);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var currency = _currencyRepository.Find(id);
            var CurrencyDto = _mapper.Map<CurrencyDto>(currency);
            return Ok(CurrencyDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] CurrencyDto CurrencyDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            CurrencyDto.Id = 0;
            var currency = _mapper.Map<Currency>(CurrencyDto);

            var validate = _currencyRepository.Duplicate(currency);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _currencyRepository.Add(currency);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "letters Formate failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(currency.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] CurrencyDto CurrencyDto)
        {
            if (CurrencyDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var currency = _mapper.Map<Currency>(CurrencyDto);
            var validate = _currencyRepository.Duplicate(currency);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _currencyRepository.Update(currency);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating currency on save.");
                return BadRequest(ModelState);
            }
            return Ok(currency.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _currencyRepository.Find(id);

            if (record == null)
                return NotFound();

            _currencyRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _currencyRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _currencyRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _currencyRepository.Active(record);
            _uow.Save();

            return Ok();
        }
        [HttpGet]
        [Route("GetCountryDropDown")]
        public IActionResult GetCountryDropDown()
        {
            return Ok(_currencyRepository.GetCountryDropDown());
        }
    }
}