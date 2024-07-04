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
    public class PatientSiteContractController : BaseController
    {
        private readonly IPatientSiteContractRepository _patientSiteContractRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public PatientSiteContractController(IPatientSiteContractRepository PatientSiteContractRepository, IUnitOfWork uow, IMapper mapper)
            
        {
            _patientSiteContractRepository = PatientSiteContractRepository;
            _uow = uow;
            _mapper = mapper;
        }  
        #region Common
        
        [HttpGet]
        [Route("GetPatientSiteContractList/{isDeleted:bool?}/{siteContractId:int}")]
        public IActionResult GetPatientSiteContractList(bool isDeleted, int siteContractId)
        {
            var paymentMilestone = _patientSiteContractRepository.GetPatientSiteContractList(isDeleted, siteContractId);
            return Ok(paymentMilestone);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var task = _patientSiteContractRepository.Find(id);
            var taskDto = _mapper.Map<SiteContractDto>(task);
            return Ok(taskDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] PatientSiteContractDto patientSiteContractDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            patientSiteContractDto.Id = 0;

            var patientSiteContract = _mapper.Map<PatientSiteContract>(patientSiteContractDto);
            _patientSiteContractRepository.Add(patientSiteContract);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Site Contract failed on save.");
                return BadRequest(ModelState);
            }
            return Ok(patientSiteContract.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SiteContractDto SiteContractDto)
        {
                var Id = SiteContractDto.Id;
                if (Id <= 0) return BadRequest();
                if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
                var task = _patientSiteContractRepository.Find(Id);
                var taskmaster = _mapper.Map<PatientSiteContract>(task);
                _patientSiteContractRepository.Update(taskmaster);
                if (_uow.Save() <= 0) return Ok(new Exception("Updating Task Master failed on save."));
                return Ok(taskmaster.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _patientSiteContractRepository.Find(id);
            if (record == null)
                return NotFound();

            _patientSiteContractRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _patientSiteContractRepository.Find(id);

            if (record == null)
                return NotFound();
            _patientSiteContractRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        #endregion
    }
}
