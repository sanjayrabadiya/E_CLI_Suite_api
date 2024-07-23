using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.IO;


namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class SiteContractController : BaseController
    {
        private readonly ISiteContractRepository _siteContractRepository;
        private readonly IPatientSiteContractRepository _PatientSiteContractRepository;
        private readonly IPassthroughSiteContractRepository _PassthroughSiteContractRepository;
        private readonly IContractTemplateFormatRepository _contractTemplateFormatRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public SiteContractController(IContractTemplateFormatRepository contractTemplateFormatRepository, IPatientSiteContractRepository PatientSiteContractRepository, IPassthroughSiteContractRepository PassthroughSiteContractRepository, ISiteContractRepository SiteContractRepository, IUnitOfWork uow, IMapper mapper, IUploadSettingRepository uploadSettingRepository, IJwtTokenAccesser jwtTokenAccesser)
        {
            _siteContractRepository = SiteContractRepository;
            _PatientSiteContractRepository = PatientSiteContractRepository;
            _PassthroughSiteContractRepository = PassthroughSiteContractRepository;
            _contractTemplateFormatRepository = contractTemplateFormatRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
        }  
        #region Common
        
        [HttpGet]
        [Route("GetSiteContractList/{isDeleted:bool?}/{studyId:int}/{siteId:int}")]
        public IActionResult GetSiteContractList(bool isDeleted, int studyId, int siteId)
        {
            var paymentMilestone = _siteContractRepository.GetSiteContractList(isDeleted, studyId, siteId);
            return Ok(paymentMilestone);
        }
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var task = _siteContractRepository.Find(id);
            var taskDto = _mapper.Map<SiteContractDto>(task);
            return Ok(taskDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] SiteContractDto siteContractDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            siteContractDto.Id = 0;

            
            var validate = _siteContractRepository.Duplicate(siteContractDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            if (!siteContractDto.IsDocument)
            {
                var contractTemplateFormate = _contractTemplateFormatRepository.Find((int)siteContractDto.ContractTemplateFormatId);

                _siteContractRepository.CreateContractTemplateFormat(contractTemplateFormate, siteContractDto);
            }
            else
            {
                if (siteContractDto.ContractFileModel?.Base64?.Length > 0 && siteContractDto.ContractFileModel?.Base64 != null)
                {
                    siteContractDto.ContractDocumentPath = DocumentService.SaveUploadDocument(siteContractDto.ContractFileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "SiteContract");
                }
            }

            var siteContract = _mapper.Map<SiteContract>(siteContractDto);
            siteContract.IpAddress = _jwtTokenAccesser.IpAddress;
            siteContract.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _siteContractRepository.Add(siteContract);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Creating Site Contract failed on save.");
                return BadRequest(ModelState);
            }
            siteContractDto.Id = siteContract.Id;
            return Ok(siteContractDto.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SiteContractDto SiteContractDto)
        {
            var Id = SiteContractDto.Id;
            if (Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var taskmaster = _mapper.Map<SiteContract>(SiteContractDto);
            if (SiteContractDto.ContractFileModel?.Base64?.Length > 0 && SiteContractDto.ContractFileModel?.Base64 != null)
            {
                taskmaster.ContractDocumentPath = DocumentService.SaveUploadDocument(SiteContractDto.ContractFileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "SiteContract");
            }
            taskmaster.IpAddress = _jwtTokenAccesser.IpAddress;
            taskmaster.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");

            _siteContractRepository.Update(taskmaster);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating Task Master failed on save."));
            return Ok(taskmaster.Id);
        }

        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _siteContractRepository.Find(id);
            if (record == null)
                return NotFound();
            //Delete PatientSite Contract
            var patientSiteContract = _PatientSiteContractRepository.FindBy(x => x.SiteContractId == id);
            foreach (var task in patientSiteContract)
            {
                _PatientSiteContractRepository.Delete(task);
                _uow.Save();
            }

            //Delete Pass through Site Contract
            var pssthroughSiteContract = _PassthroughSiteContractRepository.FindBy(x => x.SiteContractId == id);
            foreach (var task in pssthroughSiteContract)
            {
                _PassthroughSiteContractRepository.Delete(task);
                _uow.Save();
            }

            _siteContractRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public IActionResult Active(int id)
        {
            var record = _siteContractRepository.Find(id);
            var siteContract = _mapper.Map<SiteContractDto>(record);
            var validate = _siteContractRepository.Duplicate(siteContract);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            //Active PatientSite Contract
            var patientSiteContract = _PatientSiteContractRepository.FindBy(x => x.SiteContractId == id);
            foreach (var task in patientSiteContract)
            {
                _PatientSiteContractRepository.Active(task);
                _uow.Save();
            }

            //Active Pass through Site Contract
            var pssthroughSiteContract = _PassthroughSiteContractRepository.FindBy(x => x.SiteContractId == id);
            foreach (var task in pssthroughSiteContract)
            {
                _PassthroughSiteContractRepository.Active(task);
                _uow.Save();
            }
            if (record == null)
                return NotFound();
            _siteContractRepository.Active(record);
            _uow.Save();

            return Ok();
        }
        [HttpGet]
        [Route("DownloadDocument/{id}")]
        public IActionResult DownloadDocument(int id)
        {
            var file = _siteContractRepository.Find(id);
            var filePath = Path.Combine(_uploadSettingRepository.GetDocumentPath(),
                file.ContractDocumentPath);
            if (System.IO.File.Exists(filePath))
            {
                return File(System.IO.File.OpenRead(filePath), ObjectExtensions.GetMIMEType(file.ContractFileName), file.ContractFileName);
            }
            return NotFound();
        }

        #endregion
    }
}
