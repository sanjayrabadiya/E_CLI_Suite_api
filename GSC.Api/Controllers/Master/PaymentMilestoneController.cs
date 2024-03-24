using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class PaymentMilestoneController : BaseController
    {
        private readonly IPaymentMilestoneRepository _paymentMilestoneRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PaymentMilestoneController(IPaymentMilestoneRepository PaymentMilestoneRepository, IUnitOfWork uow, IMapper mapper)
        {
            _paymentMilestoneRepository = PaymentMilestoneRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetRevenueData/{parentProjectId:int}/{siteId:int?}/{countryId:int?}/{isDeleted:bool?}")]
        public IActionResult GetRevenueData(int parentProjectId, int? siteId, int? countryId, bool isDeleted)
        {
            var paymentMilestone = _paymentMilestoneRepository.GetPaymentMilestoneList(parentProjectId, siteId, countryId, isDeleted);
            return Ok(paymentMilestone);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PaymentMilestoneDto paymentMilestoneDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            paymentMilestoneDto.Id = 0;
            var paymentMilestone = _mapper.Map<PaymentMilestone>(paymentMilestoneDto);

            var validate = _paymentMilestoneRepository.DuplicatePaymentMilestone(paymentMilestone);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _paymentMilestoneRepository.Add(paymentMilestone);
            if (_uow.Save() <= 0) throw new Exception("Creating Investigator PaymentMilestone failed on save.");
            paymentMilestoneDto.Id = paymentMilestone.Id;
            _paymentMilestoneRepository.AddPaymentMilestoneTaskDetail(paymentMilestoneDto);
            return Ok(paymentMilestone.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            _paymentMilestoneRepository.DeletePaymentMilestoneTaskDetail(id);
            var record = _paymentMilestoneRepository.Find(id);

            if (record == null)
                return NotFound();

            _paymentMilestoneRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            _paymentMilestoneRepository.ActivePaymentMilestoneTaskDetail(id);
            var record = _paymentMilestoneRepository.Find(id);

            if (record == null)
                return NotFound();
            _paymentMilestoneRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTaskListforMilestone/{parentProjectId:int}/{siteId:int?}/{countryId:int?}")]
        public IActionResult GetTaskListforMilestone(int parentProjectId, int? siteId, int? countryId)
        {
            var studyplan = _paymentMilestoneRepository.GetTaskListforMilestone(parentProjectId, siteId, countryId);
            return Ok(studyplan);
        }

        [HttpPost("GetEstimatedMilestoneAmount")]
        public IActionResult GetEstimatedMilestoneAmount([FromBody] PaymentMilestoneDto paymentMilestoneDto)
        {
            var studyplan = _paymentMilestoneRepository.GetEstimatedMilestoneAmount(paymentMilestoneDto);
            return Ok(studyplan);
        }
    }
}
