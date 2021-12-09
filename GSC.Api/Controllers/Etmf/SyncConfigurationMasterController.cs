using System;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Respository.Etmf;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Etmf
{
    [Route("api/[controller]")]
    [ApiController]
    public class SyncConfigurationMasterController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly ISyncConfigurationMasterRepository _syncConfigurationMasterRepository;
        private readonly ISyncConfigurationMasterDetailsRepository _syncConfigurationMasterDetailsRepository;
        private readonly ISyncConfigurationMasterDetailsRepositoryAudit _configurationMasterDetailsRepositoryAudit;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public SyncConfigurationMasterController(IUnitOfWork uow, IMapper mapper, ISyncConfigurationMasterRepository syncConfigurationMasterRepository, ISyncConfigurationMasterDetailsRepository syncConfigurationMasterDetailsRepository, ISyncConfigurationMasterDetailsRepositoryAudit configurationMasterDetailsRepositoryAudit, IJwtTokenAccesser jwtTokenAccesser)
        {
            _uow = uow;
            _mapper = mapper;
            _syncConfigurationMasterRepository = syncConfigurationMasterRepository;
            _syncConfigurationMasterDetailsRepository = syncConfigurationMasterDetailsRepository;
            _configurationMasterDetailsRepositoryAudit = configurationMasterDetailsRepositoryAudit;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var syncConfigGridDto = _syncConfigurationMasterRepository.GetSyncConfigurationMastersList(isDeleted);
            return Ok(syncConfigGridDto);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var syncconfigration = _syncConfigurationMasterRepository.FindByInclude(x => x.Id == id, x => x.SyncConfigurationMasterDetails).FirstOrDefault();
            var syncConfigrationDto = _mapper.Map<SyncConfigurationMasterDto>(syncconfigration);
            return Ok(syncConfigrationDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SyncConfigurationMasterDto details)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            details.Id = 0;
            var syncconfigDetails = _mapper.Map<SyncConfigurationMaster>(details);
            var validate = _syncConfigurationMasterRepository.Duplicate(syncconfigDetails);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _syncConfigurationMasterRepository.Add(syncconfigDetails);
            foreach (var item in syncconfigDetails.SyncConfigurationMasterDetails)
            {
                _syncConfigurationMasterDetailsRepository.Add(item);
            }
            _uow.Save();
            foreach (var item in syncconfigDetails.SyncConfigurationMasterDetails)
            {
                var syncConfigrationMasterDetails = _mapper.Map<SyncConfigurationMasterDetailsAudit>(item);
                syncConfigrationMasterDetails.Id = 0;
                syncConfigrationMasterDetails.ReportScreenId = syncconfigDetails.ReportScreenId;
                syncConfigrationMasterDetails.Version = syncconfigDetails.Version;
                syncConfigrationMasterDetails.Activity = "Added";
                syncConfigrationMasterDetails.IpAddress = _jwtTokenAccesser.IpAddress;
                syncConfigrationMasterDetails.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                syncConfigrationMasterDetails.ReasonId = Convert.ToInt32(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                _configurationMasterDetailsRepositoryAudit.Add(syncConfigrationMasterDetails);
            }
            if (_uow.Save() <= 0) throw new Exception("Creating Sync Configuration failed on save.");
            return Ok(syncconfigDetails.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SyncConfigurationMasterDto details)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var syncconfigDetails = _mapper.Map<SyncConfigurationMaster>(details);
            var validate = _syncConfigurationMasterRepository.Duplicate(syncconfigDetails);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _syncConfigurationMasterRepository.Update(syncconfigDetails);
            foreach (var item in syncconfigDetails.SyncConfigurationMasterDetails)
            {
                //if (item.Id > 0)
                _syncConfigurationMasterDetailsRepository.Update(item);
            }
            _uow.Save();
            foreach (var item in syncconfigDetails.SyncConfigurationMasterDetails)
            {
                var syncConfigrationMasterDetails = _mapper.Map<SyncConfigurationMasterDetailsAudit>(item);
                syncConfigrationMasterDetails.Id = 0;
                syncConfigrationMasterDetails.ReportScreenId = syncconfigDetails.ReportScreenId;
                syncConfigrationMasterDetails.Version = syncconfigDetails.Version;
                syncConfigrationMasterDetails.Activity = "Updated";
                syncConfigrationMasterDetails.IpAddress = _jwtTokenAccesser.IpAddress;
                syncConfigrationMasterDetails.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                syncConfigrationMasterDetails.ReasonId = Convert.ToInt32(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                _configurationMasterDetailsRepositoryAudit.Add(syncConfigrationMasterDetails);
            }
            if (_uow.Save() <= 0) throw new Exception("Creating Sync Configuration failed on save.");
            return Ok(syncconfigDetails.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _syncConfigurationMasterRepository.FindByInclude(x => x.Id == id, x => x.SyncConfigurationMasterDetails).FirstOrDefault();
            if (record == null)
                return NotFound();
            var recordDetails = _syncConfigurationMasterDetailsRepository.FindBy(x => x.SyncConfigurationMasterId == record.Id);
            foreach (var item in recordDetails)
            {
                _syncConfigurationMasterDetailsRepository.Delete(item);

                var syncConfigrationMasterDetails = _mapper.Map<SyncConfigurationMasterDetailsAudit>(item);
                syncConfigrationMasterDetails.Id = 0;
                syncConfigrationMasterDetails.ReportScreenId = record.ReportScreenId;
                syncConfigrationMasterDetails.Version = record.Version;
                syncConfigrationMasterDetails.Activity = "Deleted";
                syncConfigrationMasterDetails.IpAddress = _jwtTokenAccesser.IpAddress;
                syncConfigrationMasterDetails.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                syncConfigrationMasterDetails.ReasonId = Convert.ToInt32(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                _configurationMasterDetailsRepositoryAudit.Add(syncConfigrationMasterDetails);
            }
            _syncConfigurationMasterRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _syncConfigurationMasterRepository.Find(id);
            if (record == null)
                return NotFound();
            var validate = _syncConfigurationMasterRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var recordDetails = _syncConfigurationMasterDetailsRepository.FindBy(x => x.SyncConfigurationMasterId == record.Id);
            foreach (var item in recordDetails)
            {
                _syncConfigurationMasterDetailsRepository.Active(item);

                var syncConfigrationMasterDetails = _mapper.Map<SyncConfigurationMasterDetailsAudit>(item);
                syncConfigrationMasterDetails.Id = 0;
                syncConfigrationMasterDetails.ReportScreenId = record.ReportScreenId;
                syncConfigrationMasterDetails.Version = record.Version;
                syncConfigrationMasterDetails.Activity = "Activated";
                syncConfigrationMasterDetails.IpAddress = _jwtTokenAccesser.IpAddress;
                syncConfigrationMasterDetails.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                syncConfigrationMasterDetails.ReasonId = Convert.ToInt32(_jwtTokenAccesser.GetHeader("audit-reason-id"));
                _configurationMasterDetailsRepositoryAudit.Add(syncConfigrationMasterDetails);
            }
            _syncConfigurationMasterRepository.Active(record);
            _uow.Save();
            return Ok();
        }


        [HttpGet]
        [Route("GetAudit")]
        public IActionResult GetAudit()
        {
            var audit = _syncConfigurationMasterRepository.GetAudit();
            return Ok(audit);
        }

        [HttpPost]       
        [Route("GetSyncConfigrationPath")]
        public IActionResult GetSyncConfigrationPath([FromBody] SyncConfigurationParameterDto details)
        {
            int ProjectWorkplaceArtificateId;
            string validate = _syncConfigurationMasterRepository.ValidateMasterConfiguration(details);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var path = _syncConfigurationMasterRepository.GetsyncConfigurationPath(details,out ProjectWorkplaceArtificateId);    
            return  Ok(new { PathDetail = path });
        }
    }
}
