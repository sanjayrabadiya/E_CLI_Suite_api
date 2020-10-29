using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class SiteController : BaseController
    {
        private readonly ISiteRepository _siteRepository;
        private readonly IManageSiteRepository _manageSiteRepository;
        private readonly IIecirbRepository _iecirbRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public SiteController(ISiteRepository siteRepository,
    IUnitOfWork uow, IMapper mapper,
    IManageSiteRepository manageSiteRepository,
    IIecirbRepository iecirbRepository)
        {
            _siteRepository = siteRepository;
            _uow = uow;
            _mapper = mapper;
            _manageSiteRepository = manageSiteRepository;
            _iecirbRepository = iecirbRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var site = _siteRepository.GetSiteList(isDeleted);
            return Ok(site);
        }

        [HttpGet]
        [Route("GetSiteById/{InvestigatorContactId}/{isDeleted:bool?}")]
        public IActionResult GetSiteById(int InvestigatorContactId, bool isDeleted)
        {
            var site = _siteRepository.GetSiteById(InvestigatorContactId, isDeleted);
            return Ok(site);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var site = _siteRepository.Find(id);
            var siteDto = _mapper.Map<SiteDto>(site);
            return Ok(siteDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SiteDto siteDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            siteDto.Id = 0;

            foreach (var variable in siteDto.ManageSiteIds)
            {
                siteDto.ManageSiteId = variable;
                bool duplicateExists = _siteRepository.All.AsNoTracking().Any(x => x.ManageSiteId == siteDto.ManageSiteId && x.InvestigatorContactId == siteDto.InvestigatorContactId && x.DeletedDate == null);
                if (!duplicateExists)
                {
                    var site = _mapper.Map<Site>(siteDto);
                    //var validate = _siteRepository.Duplicate(site);
                    //if (!string.IsNullOrEmpty(validate))
                    //{
                    //    ModelState.AddModelError("Message", validate);
                    //    return BadRequest(ModelState);
                    //}

                    _siteRepository.Add(site);
                    if (_uow.Save() <= 0) throw new Exception("Creating Site failed on save.");
                }
            }
            return Ok(siteDto.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SiteDto siteDto)
        {
            if (siteDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var site = _mapper.Map<Site>(siteDto);
            var validate = _siteRepository.Duplicate(site);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by darshil for effective Date on 26-10-2020 */
            _siteRepository.AddOrUpdate(site);

            if (_uow.Save() <= 0) throw new Exception("Updating Site failed on save.");
            return Ok(site.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _siteRepository.Find(id);

            if (record == null)
                return NotFound();

            _siteRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _siteRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _siteRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _siteRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}
