using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class FileSizeConfigurationController : BaseController
    {
        private readonly IFileSizeConfigurationRepository _fileSizeConfigurationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public FileSizeConfigurationController(IFileSizeConfigurationRepository fileSizeConfigurationRepository,
            IMapper mapper, IUnitOfWork uow)
        {
            _fileSizeConfigurationRepository = fileSizeConfigurationRepository;
            _mapper = mapper;
            _uow = uow;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var fileSizes = _fileSizeConfigurationRepository.GetFileSizeConfigurationList(isDeleted);
            return Ok(fileSizes);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var fileSize = _fileSizeConfigurationRepository.Find(id);
            var fileSizeDto = _mapper.Map<FileSizeConfigurationDto>(fileSize);
            return Ok(fileSizeDto);
        }
        [HttpPost]
        public IActionResult Post([FromBody] FileSizeConfigurationDto fileSizeConfigurationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            fileSizeConfigurationDto.Id = 0;
            var fileSize = _mapper.Map<FileSizeConfiguration>(fileSizeConfigurationDto);
            var validate = _fileSizeConfigurationRepository.Duplicate(fileSize);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _fileSizeConfigurationRepository.Add(fileSize);
            if (_uow.Save() <= 0) throw new Exception("Creating File Size Configuration failed on save.");
            return Ok(fileSize.Id);
        }
        [HttpPut]
        public IActionResult Put([FromBody] FileSizeConfigurationDto fileSizeConfigurationDto)
        {
            if (fileSizeConfigurationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var fileSize = _mapper.Map<FileSizeConfiguration>(fileSizeConfigurationDto);
            var validate = _fileSizeConfigurationRepository.Duplicate(fileSize);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _fileSizeConfigurationRepository.AddOrUpdate(fileSize);

            if (_uow.Save() <= 0) throw new Exception("Updating File Size Configuration failed on update.");
            return Ok(fileSize.Id);
        }
        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _fileSizeConfigurationRepository.Find(id);
            if (record == null)
                return NotFound();
            _fileSizeConfigurationRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _fileSizeConfigurationRepository.Find(id);
            if (record == null)
                return NotFound();
            var validate = _fileSizeConfigurationRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _fileSizeConfigurationRepository.Active(record);
            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetFileSizeDropDown")]
        public IActionResult GetDrugDropDown()
        {
            return Ok(_fileSizeConfigurationRepository.GetFileSizeConfigurationDropDown());
        }
    }
}
