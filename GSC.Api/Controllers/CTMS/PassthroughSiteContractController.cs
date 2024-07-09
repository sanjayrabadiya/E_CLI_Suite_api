using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Respository.Master;
using Microsoft.AspNetCore.Mvc;
using System;


namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class PassthroughSiteContractController : BaseController
    {
        private readonly IPassthroughSiteContractRepository _passthroughSiteContractRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public PassthroughSiteContractController(IPassthroughSiteContractRepository PassthroughSiteContractRepository, IUnitOfWork uow, IMapper mapper)
            
        {
            _passthroughSiteContractRepository = PassthroughSiteContractRepository;
            _uow = uow;
            _mapper = mapper;
        }  

        #region Common
        
        [HttpGet]
        [Route("GetPassthroughSiteContractList/{isDeleted:bool?}/{siteContractId:int}")]
        public IActionResult GetPassthroughSiteContractList(bool isDeleted, int siteContractId)
        {
            var paymentMilestone = _passthroughSiteContractRepository.GetPassthroughSiteContractList(isDeleted, siteContractId);
            return Ok(paymentMilestone);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var task = _passthroughSiteContractRepository.Find(id);
            var taskDto = _mapper.Map<PassthroughSiteContractDto>(task);
            return Ok(taskDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PassthroughSiteContractDto SiteContractDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            SiteContractDto.Id = 0;

            var siteContract = _mapper.Map<PassthroughSiteContract>(SiteContractDto);
            var validate = _passthroughSiteContractRepository.Duplicate(SiteContractDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _passthroughSiteContractRepository.Add(siteContract);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Site Contract failed on save.");
                return BadRequest(ModelState);
            }
            SiteContractDto.Id = siteContract.Id;
            return Ok(SiteContractDto.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] PassthroughSiteContractDto SiteContractDto)
        {
                var Id = SiteContractDto.Id;
                if (Id <= 0) return BadRequest();
                if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
                var task = _passthroughSiteContractRepository.Find(Id);
                var taskmaster = _mapper.Map<PassthroughSiteContract>(task);
                _passthroughSiteContractRepository.Update(taskmaster);
                if (_uow.Save() <= 0) return Ok(new Exception("Updating Task Master failed on save."));
                return Ok(taskmaster.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _passthroughSiteContractRepository.Find(id);
            if (record == null)
                return NotFound();

            _passthroughSiteContractRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _passthroughSiteContractRepository.Find(id);

            if (record == null)
                return NotFound();
            _passthroughSiteContractRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet("GetPassthroughTotalAmount/{parentProjectId:int}/{siteId:int}/{passThroughCostActivityId}")]
        public IActionResult GetPassthroughTotalAmount(int parentProjectId, int siteId, int passThroughCostActivityId)
        {
            var studyplan = _passthroughSiteContractRepository.GetPassthroughTotalAmount(parentProjectId, siteId, passThroughCostActivityId);
            return Ok(studyplan);
        }

        #endregion

    }
}
