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
    public class PatientMilestoneController : BaseController
    {
        private readonly IPatientMilestoneRepository _paymentMilestoneRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PatientMilestoneController(IPatientMilestoneRepository PaymentMilestoneRepository, IUnitOfWork uow, IMapper mapper)
        {
            _paymentMilestoneRepository = PaymentMilestoneRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetFinalPatienTotal/{ProjectId}")]
        public IActionResult GetFinalPatienTotal(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var ctmsActionPoint = _paymentMilestoneRepository.GetFinalPatienTotal(ProjectId);
            return Ok(ctmsActionPoint);
        }

        [HttpGet]
        [Route("GetPatientMilestoneList/{parentProjectId:int}/{isDeleted:bool?}")]
        public IActionResult GetPatientMilestoneList(int parentProjectId, bool isDeleted)
        {
            var paymentMilestone = _paymentMilestoneRepository.GetPaymentMilestoneList(parentProjectId, isDeleted);
            return Ok(paymentMilestone);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PatientMilestoneDto paymentMilestoneDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            paymentMilestoneDto.Id = 0;
            var paymentMilestone = _mapper.Map<PatientMilestone>(paymentMilestoneDto);

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
            paymentMilestoneDto.Id = paymentMilestone.Id;

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

            // delete ant task Add amount in total Amount #1550
            _paymentMilestoneRepository.UpdatePaybalAmount(record);

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _paymentMilestoneRepository.Find(id);

            var validate = _paymentMilestoneRepository.DuplicatePaymentMilestone(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            if (record == null)
                return NotFound();
            _paymentMilestoneRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetVisitMilestoneAmount/{ParentProjectId:int}/{visitId}")]
        public IActionResult GetVisitMilestoneAmount(int ParentProjectId,int visitId)
        {
            var studyplan = _paymentMilestoneRepository.GetEstimatedMilestoneAmount(ParentProjectId,visitId);
            return Ok(studyplan);
        }

        [HttpGet]
        [Route("GetVisitDropDown/{parentProjectId:int}")]
        public IActionResult GetVisitDropDown(int parentProjectId)
        {
            return Ok(_paymentMilestoneRepository.GetVisitDropDown(parentProjectId));
        }
    }
}
