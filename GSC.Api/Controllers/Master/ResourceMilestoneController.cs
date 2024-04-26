using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;
using System.Linq;


namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ResourceMilestoneController : BaseController
    {
        private readonly IResourceMilestoneRepository _paymentMilestoneRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public ResourceMilestoneController(IGSCContext context, IResourceMilestoneRepository PaymentMilestoneRepository, IUnitOfWork uow, IMapper mapper)
            
        {
            _paymentMilestoneRepository = PaymentMilestoneRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet]
        [Route("GetFinalResourceTotal/{ProjectId}")]
        public IActionResult GetFinalResourceTotal(int ProjectId)
        {
            if (ProjectId <= 0) return BadRequest();
            var ctmsActionPoint = _paymentMilestoneRepository.GetFinalResourceTotal(ProjectId);
            return Ok(ctmsActionPoint);
        }

        [HttpGet]
        [Route("GetresourceMilestoneList/{parentProjectId:int}/{siteId:int?}/{countryId:int?}/{isDeleted:bool?}")]
        public IActionResult GetresourceMilestoneList(int parentProjectId, int? siteId, int? countryId, bool isDeleted)
        {
            var paymentMilestone = _paymentMilestoneRepository.GetPaymentMilestoneList(parentProjectId, siteId, countryId, isDeleted);
            return Ok(paymentMilestone);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ResourceMilestoneDto paymentMilestoneDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            paymentMilestoneDto.Id = 0;
            //fiend max date for selected tasks list
            if(paymentMilestoneDto.StudyPlanTaskIds != null)
            {
                paymentMilestoneDto.DueDate = _context.StudyPlanTask.Where(w => paymentMilestoneDto.StudyPlanTaskIds.Contains(w.Id) && w.EndDate != null).Max(s => s.EndDate);
            }
               

            var paymentMilestone = _mapper.Map<ResourceMilestone>(paymentMilestoneDto);

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

            if (paymentMilestoneDto.StudyPlanTaskIds != null)
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
        public IActionResult GetEstimatedMilestoneAmount([FromBody] ResourceMilestoneDto paymentMilestoneDto)
        {
            var studyplan = _paymentMilestoneRepository.GetEstimatedMilestoneAmount(paymentMilestoneDto);
            return Ok(studyplan);
        }
    }
}
