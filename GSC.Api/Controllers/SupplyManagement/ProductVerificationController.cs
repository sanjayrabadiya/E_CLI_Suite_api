using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using GSC.Report;
using GSC.Respository.Configuration;
using GSC.Respository.SupplyManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVerificationController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProductVerificationRepository _productVerificationRepository;
        private readonly IProductVerificationDetailRepository _productVerificationDetailRepository;
        private readonly IProductVerificationReport _productVerificationReport;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public ProductVerificationController(
            IMapper mapper
            , IJwtTokenAccesser jwtTokenAccesser
            , IUnitOfWork uow
            , IProductVerificationRepository productVerificationRepository
            , IProductVerificationDetailRepository productVerificationDetailRepository
            , IProductVerificationReport productVerificationReport
            , IUploadSettingRepository uploadSettingRepository
            )
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _uow = uow;
            _productVerificationRepository = productVerificationRepository;
            _productVerificationDetailRepository = productVerificationDetailRepository;
            _productVerificationReport = productVerificationReport;
            _uploadSettingRepository = uploadSettingRepository;
        }

        [HttpGet("GetProductVerificationList/{productReceiptId}")]
        public IActionResult GetProductVerificationList(int productReceiptId)
        {
            var productVerification = _productVerificationRepository.GetProductVerificationList(productReceiptId);
            return Ok(productVerification);
        }

        [HttpGet("GetProductVerificationData/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetProductReceiptList(int projectId, bool isDeleted)
        {
            var productVerification = _productVerificationRepository.GetProductVerification(projectId, isDeleted);
            return Ok(productVerification);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var productVerification = _productVerificationRepository.Find(id);
            var productVerificationtDto = _mapper.Map<ProductVerificationGridDto>(productVerification);
            return Ok(productVerificationtDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProductVerificationDto productVerificationDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            productVerificationDto.Id = 0;

            //set file path and extension
            if (productVerificationDto.FileModel?.Base64?.Length > 0)
            {
                productVerificationDto.PathName = DocumentService.SaveUploadDocument(productVerificationDto.FileModel, _uploadSettingRepository.GetDocumentPath(),_jwtTokenAccesser.CompanyId.ToString(), FolderType.ProductVerification,"");
                productVerificationDto.MimeType = productVerificationDto.FileModel.Extension;
                productVerificationDto.FileName = "ProductVerification_" + DateTime.Now.Ticks + "." + productVerificationDto.FileModel.Extension;
            }

            var productVerification = _mapper.Map<ProductVerification>(productVerificationDto);

            _productVerificationRepository.Add(productVerification);

            if (_uow.Save() <= 0) throw new Exception("Creating product verification failed on save.");

            return Ok(productVerification.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProductVerificationDto productVerificationDto)
        {
            if (productVerificationDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //added by vipul for if document send empty if they cant want to change docuemnt
            var productRec = _productVerificationRepository.Find(productVerificationDto.Id);
            if (productVerificationDto.FileModel?.Base64?.Length > 0)
            {
                productVerificationDto.PathName = DocumentService.SaveDocument(productVerificationDto.FileModel, _uploadSettingRepository.GetDocumentPath(), FolderType.ProductReceipt, "");
                productVerificationDto.MimeType = productVerificationDto.FileModel.Extension;
                productVerificationDto.FileName = "ProductVerification_" + DateTime.Now.Ticks + "." + productVerificationDto.FileModel.Extension;
            }
            else
            {
                productVerificationDto.PathName = productRec.PathName;
                productVerificationDto.MimeType = productRec.MimeType;
                productVerificationDto.FileName = productRec.FileName;
            }

            var productVerification = _mapper.Map<ProductVerification>(productVerificationDto);

            _productVerificationRepository.AddOrUpdate(productVerification);

            if (_uow.Save() <= 0) throw new Exception("Updating product verification failed on save.");
            return Ok(productVerification.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _productVerificationRepository.Find(id);
            if (record == null)
                return NotFound();
            _productVerificationRepository.Delete(record);

            var verifyRecord = _productVerificationDetailRepository.FindByInclude(x => x.ProductVerificationId == id).ToList();

            if (verifyRecord != null)
                _productVerificationDetailRepository.Delete(verifyRecord[0]);

            _uow.Save();
            return Ok();
        }

        [HttpGet]
        [Route("GetProductVerificationSummary/{ProductReceiptId}")]
        [AllowAnonymous]
        public IActionResult GetProductVerificationSummary(int ProductReceiptId)
        {
            ReportSettingNew reportSetting = new ReportSettingNew();
            reportSetting.AnnotationType = true;
            reportSetting.LeftMargin = Convert.ToDecimal(0.5);
            reportSetting.RightMargin = Convert.ToDecimal(0.5);
            reportSetting.BottomMargin = Convert.ToDecimal(0.5);
            reportSetting.TopMargin = Convert.ToDecimal(0.5);
            reportSetting.IsClientLogo = true;
            reportSetting.IsCompanyLogo = true;
            reportSetting.NonCRF = true;

            var response = _productVerificationReport.GetProductVerificationSummary(reportSetting, ProductReceiptId);
            return response;
        }

    }
}
