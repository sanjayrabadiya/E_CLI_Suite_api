using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class PaymentTermsController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPaymentTermsRepository _paymentTermsRepository;
        private readonly IUnitOfWork _uow;

        public PaymentTermsController(IPaymentTermsRepository paymentTypeRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _paymentTermsRepository = paymentTypeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var paymentTypes = _paymentTermsRepository.GetPaymentTermsList(isDeleted);
            return Ok(paymentTypes);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var paymentType = _paymentTermsRepository.Find(id);
            var PaymentTermsDto = _mapper.Map<PaymentTermsDto>(paymentType);
            return Ok(PaymentTermsDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PaymentTermsDto PaymentTermsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            PaymentTermsDto.Id = 0;
            var paymentType = _mapper.Map<PaymentTerms>(PaymentTermsDto);
            var validate = _paymentTermsRepository.Duplicate(paymentType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _paymentTermsRepository.Add(paymentType);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating PaymentType failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(paymentType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PaymentTermsDto PaymentTermsDto)
        {
            if (PaymentTermsDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var paymentType = _mapper.Map<PaymentTerms>(PaymentTermsDto);
            var validate = _paymentTermsRepository.Duplicate(paymentType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _paymentTermsRepository.AddOrUpdate(paymentType);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating PaymentType failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(paymentType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _paymentTermsRepository.Find(id);

            if (record == null)
                return NotFound();

            _paymentTermsRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _paymentTermsRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _paymentTermsRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _paymentTermsRepository.Active(record);
            _uow.Save();

            return Ok();
        }


        [HttpGet]
        [Route("GetPaymentTermsDropDown")]
        public IActionResult GetPaymentTermsDropDown()
        {
            return Ok(_paymentTermsRepository.GetPaymentTermsDropDown());
        }
    }
}
