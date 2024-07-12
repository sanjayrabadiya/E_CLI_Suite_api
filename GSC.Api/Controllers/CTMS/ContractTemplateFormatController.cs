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
    public class ContractTemplateFormatController : BaseController
    {
        private readonly IContractTemplateFormatRepository _contractTemplateFormatRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ContractTemplateFormatController(IContractTemplateFormatRepository contractTemplateFormatRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _contractTemplateFormatRepository = contractTemplateFormatRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var ContractTemplateFormat = _contractTemplateFormatRepository.GetContractTemplateFormateList(isDeleted);
            return Ok(ContractTemplateFormat);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var ContractTemplateFormat = _contractTemplateFormatRepository.Find(id);
            var ContractTemplateFormatDto = _mapper.Map<ContractTemplateFormatDto>(ContractTemplateFormat);
            return Ok(ContractTemplateFormatDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ContractTemplateFormatDto ContractTemplateFormatDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            ContractTemplateFormatDto.Id = 0;
            var ContractTemplateFormat = _mapper.Map<ContractTemplateFormat>(ContractTemplateFormatDto);

            var validate = _contractTemplateFormatRepository.Duplicate(ContractTemplateFormat);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _contractTemplateFormatRepository.Add(ContractTemplateFormat);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Contract Template Format failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(ContractTemplateFormat.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ContractTemplateFormatDto ContractTemplateFormatDto)
        {
            if (ContractTemplateFormatDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var ContractTemplateFormat = _mapper.Map<ContractTemplateFormat>(ContractTemplateFormatDto);
            var validate = _contractTemplateFormatRepository.Duplicate(ContractTemplateFormat);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _contractTemplateFormatRepository.Update(ContractTemplateFormat);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Contract Template Format failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(ContractTemplateFormat.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _contractTemplateFormatRepository.Find(id);

            if (record == null)
                return NotFound();

            _contractTemplateFormatRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _contractTemplateFormatRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _contractTemplateFormatRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _contractTemplateFormatRepository.Active(record);
            _uow.Save();

            return Ok();
        }
        [HttpGet]
        [Route("GetContractFormatTypeDropDown")]
        public IActionResult GetContractFormatTypeDropDown()
        {
            return Ok(_contractTemplateFormatRepository.GetContractFormatTypeDropDown());
        }
    }
}