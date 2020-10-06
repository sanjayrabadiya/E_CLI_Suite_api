using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Helper.DocumentService;
using GSC.Respository.Attendance;
using GSC.Respository.Configuration;
using GSC.Respository.InformConcent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using EJ2WordDocument = Syncfusion.EJ2.DocumentEditor.WordDocument;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentReviewDetailsController : ControllerBase
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly GscContext _context;
        private readonly IEconsentReviewDetailsRepository _econsentReviewDetailsRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public EconsentReviewDetailsController(IUnitOfWork<GscContext> uow,
            IMapper mapper,
            IEconsentSetupRepository econsentSetupRepository,
            IRandomizationRepository noneRegisterRepository,
            IEconsentReviewDetailsRepository econsentReviewDetailsRepository,
            IUploadSettingRepository uploadSettingRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _context = uow.Context;
            _econsentReviewDetailsRepository = econsentReviewDetailsRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        [HttpGet]
        [Route("GetEconsentDocumentHeaders/{patientId}")]
        public IActionResult GetEconsentDocumentHeaders(int patientId)
        {
            var sectionsHeaders = _econsentReviewDetailsRepository.GetEconsentDocumentHeaders(patientId);
            return Ok(sectionsHeaders);
        }

        [HttpGet]
        [Route("GetEconsentDocumentHeadersByDocumentId/{documentId}")]
        public IActionResult GetEconsentDocumentHeadersByDocumentId(int documentId)
        {
            var sectionsHeaders = _econsentReviewDetailsRepository.GetEconsentDocumentHeadersByDocumentId(documentId);
            return Ok(sectionsHeaders);
        }

        [AllowAnonymous]
        [HttpPost]
        [Route("ImportSectionData/{id}/{sectionno}")]
        public string ImportSectionData(int id, int sectionno)
        {
            var jsonnew = _econsentReviewDetailsRepository.ImportSectionData(id, sectionno);
            return jsonnew;
        }

        [HttpPost]
        public IActionResult Post([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var econsentReviewDetail = _mapper.Map<EconsentReviewDetails>(econsentReviewDetailsDto);
            econsentReviewDetail.patientapproveddatetime = DateTime.Now;
            var validate = _econsentReviewDetailsRepository.Duplicate(econsentReviewDetailsDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            if (econsentReviewDetailsDto.patientdigitalSignBase64?.Length > 0)
            {
                FileModel fileModel = new FileModel();
                fileModel.Base64 = econsentReviewDetailsDto.patientdigitalSignBase64;
                fileModel.Extension = "png";
                econsentReviewDetail.patientdigitalSignImagepath = new ImageService().ImageSave(fileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.InformConcent);
            }

            _econsentReviewDetailsRepository.Add(econsentReviewDetail);
            
            if (_uow.Save() <= 0) throw new Exception("Creating Econsent review insert failed on save.");

            return Ok(econsentReviewDetail.Id);
        }

        [HttpGet]
        [Route("GetPatientDropdown/{projectid}")]
        public IActionResult GetPatientDropdown(int projectid)
        {
            return Ok(_econsentReviewDetailsRepository.GetPatientDropdown(projectid));
        }

        [HttpGet]
        [Route("GetUnApprovedEconsentDocumentList/{patientid}")]
        public IActionResult GetUnApprovedEconsentDocumentList(int patientid)
        {
            return Ok(_econsentReviewDetailsRepository.GetUnApprovedEconsentDocumentList(patientid));
        }

        [HttpGet]
        [Route("GetApprovedEconsentDocumentList/{projectid}")]
        public IActionResult GetApprovedEconsentDocumentList(int projectid)
        {
            return Ok(_econsentReviewDetailsRepository.GetApprovedEconsentDocumentList(projectid));
        }

        [HttpPost]
        [Route("GetEconsentDocument/{id}")]
        public IActionResult GetEconsentDocument(int id)
        {
            var json = _econsentReviewDetailsRepository.GetEconsentDocument(id);
            return Ok(json);
        }

        [HttpPut]
        [Route("ApproveEconsentDocument")]
        public IActionResult ApproveEconsentDocument([FromBody] EconsentReviewDetailsDto econsentReviewDetailsDto)
        {
            if (econsentReviewDetailsDto.Id <= 0) return BadRequest();

            var econsentReviewDetails = _econsentReviewDetailsRepository.Find(econsentReviewDetailsDto.Id);
            econsentReviewDetails.IsApprovedByInvestigator = true;
            econsentReviewDetails.investigatorapproveddatetime = DateTime.Now;

            if (econsentReviewDetailsDto.investigatordigitalSignBase64?.Length > 0)
            {
                FileModel fileModel = new FileModel();
                fileModel.Base64 = econsentReviewDetailsDto.investigatordigitalSignBase64;
                fileModel.Extension = "png";
                econsentReviewDetails.investigatordigitalSignImagepath = new ImageService().ImageSave(fileModel,
                    _uploadSettingRepository.GetImagePath(), FolderType.InformConcent);
            }

            _econsentReviewDetailsRepository.Update(econsentReviewDetails);

            if (_uow.Save() <= 0) throw new Exception("Approving failed");
            return Ok(econsentReviewDetails.Id);
        }


    }
}
