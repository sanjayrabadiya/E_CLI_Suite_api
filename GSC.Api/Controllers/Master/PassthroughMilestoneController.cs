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
    public class PassthroughMilestoneController : BaseController
    {
        private readonly IPassthroughMilestoneRepository _paymentMilestoneRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PassthroughMilestoneController(IPassthroughMilestoneRepository PaymentMilestoneRepository, IUnitOfWork uow, IMapper mapper)
        {
            _paymentMilestoneRepository = PaymentMilestoneRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetPassthroughMilestoneList/{parentProjectId:int}/{isDeleted:bool?}")]
        public IActionResult GetPassthroughMilestoneList(int parentProjectId, bool isDeleted)
        {
            var paymentMilestone = _paymentMilestoneRepository.GetPassthroughMilestoneList(parentProjectId, isDeleted);
            return Ok(paymentMilestone);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PassthroughMilestoneDto paymentMilestoneDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            paymentMilestoneDto.Id = 0;
            var paymentMilestone = _mapper.Map<PassthroughMilestone>(paymentMilestoneDto);

            var validate = _paymentMilestoneRepository.DuplicatePaymentMilestone(paymentMilestone);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _paymentMilestoneRepository.Add(paymentMilestone);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Investigator PaymentMilestone failed on save.");
                return BadRequest(ModelState);
            }

            return Ok(paymentMilestone.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _paymentMilestoneRepository.Find(id);

            if (record == null)
                return NotFound();

            _paymentMilestoneRepository.Delete(record);
            _uow.Save();

            //Add by mitul-> delete ant task Add amount in total Amount #1550
            _paymentMilestoneRepository.UpdatePaybalAmount(record);
            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _paymentMilestoneRepository.Find(id);

            if (record == null)
                return NotFound();
            _paymentMilestoneRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpPost("GetPassthroughMilestoneAmount")]
        public IActionResult GetPassthroughMilestoneAmount([FromBody] PassthroughMilestoneDto paymentMilestoneDto)
        {
            var studyplan = _paymentMilestoneRepository.GetPassthroughMilestoneAmount(paymentMilestoneDto);
            return Ok(studyplan);
        }

        [HttpGet]
        [Route("GetPassThroughCostActivity/{projectId:int}")]
        public IActionResult GetPassThroughCostActivity(int projectId)
        {
            return Ok(_paymentMilestoneRepository.GetPassThroughCostActivity(projectId));
        }

        [HttpGet]
        [Route("GetFinalPassthroughTotal/{ProjectId}")]
        public IActionResult GetFinalPassthroughTotal(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var ctmsActionPoint = _paymentMilestoneRepository.GetFinalPassthroughTotal(ProjectId);
            return Ok(ctmsActionPoint);
        }
    }
}
