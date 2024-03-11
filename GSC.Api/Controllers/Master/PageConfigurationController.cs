using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Api.Controllers.Common;
using GSC.Respository.Master;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class PageConfigurationController : BaseController
    {
        private readonly IPageConfigurationRepository _pageConfigurationRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public PageConfigurationController(IPageConfigurationRepository pageConfigurationRepository,
            IMapper mapper, IUnitOfWork uow)
        {
            _pageConfigurationRepository = pageConfigurationRepository;
            _mapper = mapper;
            _uow = uow;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var pageConfigurations = _pageConfigurationRepository.GetPageConfigurationList(isDeleted);
            return Ok(pageConfigurations);
        }

        // GET: api/<controller>
        [HttpGet("{screenId}/{isDeleted:bool?}")]
        public IActionResult Get(int screenId, bool isDeleted)
        {
            var pageConfigurations = _pageConfigurationRepository.GetPageConfigurationListByScreen(screenId, isDeleted);
            return Ok(pageConfigurations);
        }



        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var pageConfig = _mapper.Map<PageConfigurationDto>(_pageConfigurationRepository.Find(id));
            return Ok(pageConfig);
        }


        [HttpPost]
        public IActionResult Post([FromBody] PageConfigurationDto pageConfigDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            pageConfigDto.Id = 0;
            var pageConfig = _mapper.Map<PageConfiguration>(pageConfigDto);
            var duplicate = _pageConfigurationRepository.Duplicate(pageConfig);
            if (string.IsNullOrEmpty(duplicate))
            {
                _pageConfigurationRepository.Add(pageConfig);
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
        public IActionResult Put([FromBody] PageConfigurationDto pageConfigDto)
        {
            if (pageConfigDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var pageConfiguration = _mapper.Map<PageConfiguration>(pageConfigDto);
            _pageConfigurationRepository.AddOrUpdate(pageConfiguration);

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
            var record = _pageConfigurationRepository.Find(id);

            if (record == null)
                return NotFound();

            _pageConfigurationRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _pageConfigurationRepository.Find(id);

            if (record == null)
                return NotFound();
            _pageConfigurationRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetPageConfigurationByAppScreen/{screenCode}")]
        public ActionResult GetPageConfigurationByScreen(string screenCode)
        {
            return Ok(_pageConfigurationRepository.GetPageConfigurationByAppScreen(screenCode));
        }

        [HttpGet("GetPageConfigurationByScreen/{screenId}")]
        public ActionResult GetPageConfigurationByScreen(int screenId)
        {
            return Ok(_pageConfigurationRepository.GetPageConfigurationByAppScreen(screenId));
        }
    }
}
