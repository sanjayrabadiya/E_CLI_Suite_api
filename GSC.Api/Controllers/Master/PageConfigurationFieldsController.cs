using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageConfigurationFieldsController : BaseController
    {
        private readonly IPageConfigurationFieldsRepository _pageConfigurationFieldsRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PageConfigurationFieldsController(IPageConfigurationFieldsRepository pageConfigurationRepository,
            IMapper mapper, IUnitOfWork uow)
        {
            _pageConfigurationFieldsRepository = pageConfigurationRepository;
            _mapper = mapper;
            _uow = uow;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var pageConfigurations = _pageConfigurationFieldsRepository.GetPageConfigurationList(isDeleted);
            return Ok(pageConfigurations);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var pageConfig = _mapper.Map<PageConfigurationFieldsDto>(_pageConfigurationFieldsRepository.Find(id));
            return Ok(pageConfig);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PageConfigurationFieldsDto pageConfigDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            pageConfigDto.Id = 0;
            var pageConfig = _mapper.Map<PageConfigurationFields>(pageConfigDto);
            var duplicate = _pageConfigurationFieldsRepository.Duplicate(pageConfig);
            if (string.IsNullOrEmpty(duplicate))
            {
                _pageConfigurationFieldsRepository.Add(pageConfig);
                if (_uow.Save() <= 0)
                {
                    ModelState.AddModelError("Message", "Creating page configuration failed on save.");
                    return BadRequest(ModelState);
                }
                return Ok(pageConfig.Id);
            }
            else
            {
                return BadRequest(duplicate);
            }
        }

        [HttpPut]
        public IActionResult Put([FromBody] PageConfigurationFieldsDto pageConfigDto)
        {
            if (pageConfigDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var pageConfiguration = _mapper.Map<PageConfigurationFields>(pageConfigDto);
            _pageConfigurationFieldsRepository.AddOrUpdate(pageConfiguration);

            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Page Configuration failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(pageConfiguration.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _pageConfigurationFieldsRepository.Find(id);

            if (record == null)
                return NotFound();

            _pageConfigurationFieldsRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _pageConfigurationFieldsRepository.Find(id);

            if (record == null)
                return NotFound();
            _pageConfigurationFieldsRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetFields/{screenCode}")]
        public ActionResult GetFields(int screenCode)
        {
            var dropDownList = _pageConfigurationFieldsRepository.GetPageConfigurationFieldsDropDown(screenCode);
            return Ok(dropDownList);
        }
    }
}
