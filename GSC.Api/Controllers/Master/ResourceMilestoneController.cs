using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Master;
using GSC.Shared.JWTAuth;
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
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public ResourceMilestoneController(IGSCContext context, IResourceMilestoneRepository PaymentMilestoneRepository, IUnitOfWork uow, IMapper mapper, IJwtTokenAccesser jwtTokenAccesser)
            
        {
            _paymentMilestoneRepository = PaymentMilestoneRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
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
        [Route("GetresourceMilestoneList/{isDeleted:bool?}/{studyId:int}/{siteId:int}/{countryId:int}/{filter}")]
        public IActionResult GetresourceMilestoneList(bool isDeleted, int studyId, int siteId, int countryId, CtmsStudyTaskFilter filter)
        {
            var paymentMilestone = _paymentMilestoneRepository.GetPaymentMilestoneList(isDeleted, studyId, siteId, countryId, filter);
            return Ok(paymentMilestone);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ResourceMilestoneDto paymentMilestoneDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            paymentMilestoneDto.Id = 0;
               

            var paymentMilestone = _mapper.Map<ResourceMilestone>(paymentMilestoneDto);

            var validate = _paymentMilestoneRepository.DuplicatePaymentMilestone(paymentMilestone);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            paymentMilestone.IpAddress = _jwtTokenAccesser.IpAddress;
            paymentMilestone.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
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

        [HttpGet]
        [Route("GetTaskListforMilestone/{studyId:int}/{siteId:int}/{countryId:int}/{filter}")]
        public IActionResult GetTaskListforMilestone(int studyId, int siteId, int countryId, CtmsStudyTaskFilter filter)
        {
            var studyplan = _paymentMilestoneRepository.GetTaskListforMilestone(studyId, siteId, countryId, filter);
            return Ok(studyplan);
        }

        [HttpGet]
        [Route("GetCountryDropdown/{parentId}")]
        public IActionResult GetCountryDropdown(int parentId)
        {
            return Ok(_paymentMilestoneRepository.GetBudgetCountryDropDown(parentId));
        }

        [HttpGet]
        [Route("GetSiteDropdown/{parentId}")]
        public IActionResult GetSiteDropdown(int parentId)
        {
            return Ok(_paymentMilestoneRepository.GetBudgetSiteDropDown(parentId));
        }
    }
}
