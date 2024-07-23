using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;


namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class SitePaymentController : BaseController
    {
        private readonly ISitePaymentRepository _sitePaymentRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public SitePaymentController(ISitePaymentRepository SitePaymentRepository, IUnitOfWork uow, IMapper mapper)
            
        {
            _sitePaymentRepository = SitePaymentRepository;
            _uow = uow;
            _mapper = mapper;
        }

        #region Patient
        [HttpGet]
        [Route("GetVisitDropDown/{parentProjectId:int}/{siteId:int}")]
        public IActionResult GetVisitDropDown(int parentProjectId, int siteId)
        {
            return Ok(_sitePaymentRepository.GetVisitDropDown(parentProjectId, siteId));
        }

        [HttpGet("GetVisitAmount/{parentProjectId:int}/{siteId:int}/{visitId}")]
        public IActionResult GetVisitAmount(int parentProjectId, int siteId, int visitId)
        {
            var studyplan = _sitePaymentRepository.GetVisitAmount(parentProjectId, siteId, visitId);
            return Ok(studyplan);
        }
        #endregion

        #region PassThroughCost
        [HttpGet]
        [Route("GetPassThroughCostActivity/{projectId:int}/{siteId:int}")]
        public IActionResult GetPassThroughCostActivity(int projectId, int siteId)
        {
            return Ok(_sitePaymentRepository.GetPassThroughCostActivity(projectId, siteId));
        }

        [HttpGet("GetPassthroughTotalAmount/{parentProjectId:int}/{siteId:int}/{passThroughCostActivityId}")]
        public IActionResult GetPassthroughTotalAmount(int parentProjectId, int siteId, int passThroughCostActivityId)
        {
            var studyplan = _sitePaymentRepository.GetPassthroughTotalAmount(parentProjectId, siteId, passThroughCostActivityId);
            return Ok(studyplan);
        }
        #endregion

        #region Common
        
        [HttpGet]
        [Route("GetSitePaymentList/{isDeleted:bool?}/{studyId:int}/{siteId:int}")]
        public IActionResult GetSitePaymentList(bool isDeleted, int studyId, int siteId)
        {
            var paymentMilestone = _sitePaymentRepository.GetSitePaymentList(isDeleted, studyId, siteId);
            return Ok(paymentMilestone);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SitePaymentDto paymentMilestoneDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            paymentMilestoneDto.Id = 0;

            var paymentMilestone = _mapper.Map<SitePayment>(paymentMilestoneDto);
            var validate = _sitePaymentRepository.Duplicate(paymentMilestoneDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _sitePaymentRepository.Add(paymentMilestone);
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
            var record = _sitePaymentRepository.Find(id);
            if (record == null)
                return NotFound();

            _sitePaymentRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _sitePaymentRepository.Find(id);
            var paymentMilestone = _mapper.Map<SitePaymentDto>(record);
            var validate = _sitePaymentRepository.Duplicate(paymentMilestone);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            if (record == null)
                return NotFound();
            _sitePaymentRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        #endregion

    }
}
