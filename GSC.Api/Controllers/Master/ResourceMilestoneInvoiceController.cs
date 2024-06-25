using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ResourceMilestoneInvoiceController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IResourceMilestoneInvoiceRepository _resourceMilestoneInvoiceRepository;
        private readonly IUnitOfWork _uow;

        public ResourceMilestoneInvoiceController(IResourceMilestoneInvoiceRepository resourceMilestoneInvoiceRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _resourceMilestoneInvoiceRepository = resourceMilestoneInvoiceRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var resourceMilestoneInvoices = _resourceMilestoneInvoiceRepository.GetResourceMilestoneInvoiceList(isDeleted);
            return Ok(resourceMilestoneInvoices);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var resourceMilestoneInvoice = _resourceMilestoneInvoiceRepository.Find(id);
            var resourceMilestoneInvoiceDto = _mapper.Map<ResourceMilestoneInvoiceDto>(resourceMilestoneInvoice);
            return Ok(resourceMilestoneInvoiceDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ResourceMilestoneInvoiceDto resourceMilestoneInvoiceDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            resourceMilestoneInvoiceDto.Id = 0;
            var resourceMilestoneInvoice = _mapper.Map<ResourceMilestoneInvoice>(resourceMilestoneInvoiceDto);
            var validate = _resourceMilestoneInvoiceRepository.Duplicate(resourceMilestoneInvoice);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _resourceMilestoneInvoiceRepository.Add(resourceMilestoneInvoice);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Resource Milestone Invoice failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(resourceMilestoneInvoice.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ResourceMilestoneInvoiceDto resourceMilestoneInvoiceDto)
        {
            if (resourceMilestoneInvoiceDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var resourceMilestoneInvoice = _mapper.Map<ResourceMilestoneInvoice>(resourceMilestoneInvoiceDto);
            var validate = _resourceMilestoneInvoiceRepository.Duplicate(resourceMilestoneInvoice);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _resourceMilestoneInvoiceRepository.AddOrUpdate(resourceMilestoneInvoice);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Resource Milestone Invoice failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(resourceMilestoneInvoice.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _resourceMilestoneInvoiceRepository.Find(id);

            if (record == null)
                return NotFound();

            _resourceMilestoneInvoiceRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _resourceMilestoneInvoiceRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _resourceMilestoneInvoiceRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _resourceMilestoneInvoiceRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetResourceMilestoneInvoiceById/{milestoneId}")]
        public ActionResult GetResourceMilestoneInvoiceById(int milestoneId)
        {
            var record = _resourceMilestoneInvoiceRepository.GetResourceMilestoneInvoiceById(milestoneId);
            return Ok(record);
        }
    }
}
