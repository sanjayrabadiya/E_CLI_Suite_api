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
    public class PaymentTypeController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IPaymentTypeRepository _paymentTypeRepository;
        private readonly IUnitOfWork _uow;

        public PaymentTypeController(IPaymentTypeRepository paymentTypeRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _paymentTypeRepository = paymentTypeRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var paymentTypes = _paymentTypeRepository.GetPaymentTypeList(isDeleted);
            return Ok(paymentTypes);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var paymentType = _paymentTypeRepository.Find(id);
            var PaymentTypeDto = _mapper.Map<PaymentTypeDto>(paymentType);
            return Ok(PaymentTypeDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PaymentTypeDto PaymentTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            PaymentTypeDto.Id = 0;
            var paymentType = _mapper.Map<PaymentType>(PaymentTypeDto);
            var validate = _paymentTypeRepository.Duplicate(paymentType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _paymentTypeRepository.Add(paymentType);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating PaymentType failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(paymentType.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PaymentTypeDto PaymentTypeDto)
        {
            if (PaymentTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var paymentType = _mapper.Map<PaymentType>(PaymentTypeDto);
            var validate = _paymentTypeRepository.Duplicate(paymentType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _paymentTypeRepository.AddOrUpdate(paymentType);

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
            var record = _paymentTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _paymentTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _paymentTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _paymentTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _paymentTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}
