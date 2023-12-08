using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.SupplyManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementUploadFileController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementUploadFileRepository _supplyManagementUploadFileRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public SupplyManagementUploadFileController(ISupplyManagementUploadFileRepository supplyManagementUploadFileRepository,
            IUnitOfWork uow, IMapper mapper,
            IUploadSettingRepository uploadSettingRepository,
            IProjectRepository projectRepository,
            IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
        {
            _supplyManagementUploadFileRepository = supplyManagementUploadFileRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _mapper = mapper;
            _projectRepository = projectRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}/{projectId}")]
        public IActionResult Get(bool isDeleted, int projectId)
        {
            return Ok(_supplyManagementUploadFileRepository.GetSupplyManagementUploadFileList(isDeleted, projectId));
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] SupplyManagementUploadFileDto supplyManagementUploadFileDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            supplyManagementUploadFileDto.Id = 0;
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == supplyManagementUploadFileDto.ProjectId).FirstOrDefault();
            if (setting == null)
            {
                ModelState.AddModelError("Message", "Please set kit number setting");
                return BadRequest(ModelState);
            }
            string SiteCode = "";
            //set file path and extension
            if (supplyManagementUploadFileDto.FileModel?.Base64?.Length > 0)
            {
                SiteCode = _projectRepository.GetStudyCode(supplyManagementUploadFileDto.ProjectId);
                supplyManagementUploadFileDto.PathName = DocumentService.SaveUploadDocument(supplyManagementUploadFileDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), SiteCode, FolderType.RandomizationUpload, "");
                supplyManagementUploadFileDto.MimeType = supplyManagementUploadFileDto.FileModel.Extension;
                supplyManagementUploadFileDto.FileName = "SupplyManagementData_" + DateTime.Now.Ticks + "." + supplyManagementUploadFileDto.FileModel.Extension;
            }
            
            var supplyManagementUploadFile = _mapper.Map<SupplyManagementUploadFile>(supplyManagementUploadFileDto);
            supplyManagementUploadFile.IpAddress = _jwtTokenAccesser.IpAddress;
            supplyManagementUploadFile.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");

            //Upload Excel data into database table
            var validate = _supplyManagementUploadFileRepository.InsertExcelDataIntoDatabaseTable(supplyManagementUploadFile, setting);

            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            //if (_uow.Save() <= 0) throw new Exception("Creating updaload data failed on save.");
            return Ok(supplyManagementUploadFile.Id);
        }

        //Get Format
        [HttpPost]
        [Route("DownloadFormat")]
        public IActionResult DownloadFormat([FromBody] SupplyManagementUploadFileDto search)
        {
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == search.ProjectId).FirstOrDefault();
            if (setting == null)
            {
                ModelState.AddModelError("Message", "Please set kit number setting");
                return BadRequest(ModelState);
            }
            return _supplyManagementUploadFileRepository.DownloadFormat(search, setting);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementUploadFileDto supplyManagementUploadFileDto)
        {
            if (supplyManagementUploadFileDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var supplyManagementUploadFile = _supplyManagementUploadFileRepository.Find(supplyManagementUploadFileDto.Id);
            supplyManagementUploadFile.Status = supplyManagementUploadFileDto.Status;
            supplyManagementUploadFile.AuditReasonId = supplyManagementUploadFileDto.AuditReasonId;
            supplyManagementUploadFile.ReasonOth = supplyManagementUploadFileDto.ReasonOth;

            _supplyManagementUploadFileRepository.Update(supplyManagementUploadFile);

            if (_uow.Save() <= 0) throw new Exception("Updating lab management data failed on action.");
            _supplyManagementUploadFileRepository.SendRandomizationUploadSheetEmail(supplyManagementUploadFile);
            return Ok(supplyManagementUploadFile.Id);
        }
        [HttpGet("CheckUploadApproalPending/{ProjectId}/{SiteId}/{CountryId}")]
        public IActionResult CheckUploadApproalPending(int ProjectId, int SiteId, int CountryId)
        {
            var productVerification = _supplyManagementUploadFileRepository.CheckUploadApproalPending(ProjectId, SiteId, CountryId);
            return Ok(productVerification);
        }
        [HttpGet("Checknumbersetting/{ProjectId}")]
        public IActionResult Checknumbersetting(int ProjectId)
        {
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == ProjectId).FirstOrDefault();
            if (setting == null)
            {
                return Ok(false);
            }
            return Ok(true);
        }

    }
}
