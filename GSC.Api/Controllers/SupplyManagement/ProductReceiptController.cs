using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.SupplyManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;


namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductReceiptController : BaseController
    {

        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProductReceiptRepository _productReceiptRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly ICentralDepotRepository _centralDepotRepository;
        private readonly IGSCContext _context;
        private readonly IProductVerificationRepository _productVerificationRepository;
        private readonly IProductVerificationDetailRepository _productVerificationDetailRepository;
        

        public ProductReceiptController(IProductReceiptRepository productReceiptRepository,
            ICentralDepotRepository centralDepotRepository,
            IUploadSettingRepository uploadSettingRepository,
            IProductVerificationDetailRepository productVerificationDetailRepository,
            IProductVerificationRepository productVerificationRepository,
            IGSCContext context,
        IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _productReceiptRepository = productReceiptRepository;
            _centralDepotRepository = centralDepotRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _productVerificationRepository = productVerificationRepository;
            _productVerificationDetailRepository = productVerificationDetailRepository;
            _context = context;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            
        }

        [HttpGet("GetProductReceiptList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetProductReceiptList(int projectId, bool isDeleted)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var productReciept = _productReceiptRepository.GetProductReceiptList(projectId, isDeleted);
            productReciept.ForEach(t =>
            {
                t.PathName = t.PathName == null ? "" : documentUrl + t.PathName;
                t.ProductVerificationDetaild = _context.ProductVerificationDetail.Where(x => x.ProductReceiptId == t.Id).FirstOrDefault() != null ?
                _context.ProductVerificationDetail.Where(x => x.ProductReceiptId == t.Id).Select(x => x.Id).FirstOrDefault() : 0;
                var verification = _context.ProductVerification.Where(x => x.ProductReceiptId == t.Id && x.DeletedDate == null).FirstOrDefault();
                if (verification != null)
                {
                    var unit = _context.Unit.Where(s => s.Id == verification.UnitId).FirstOrDefault();
                    t.PacketTypeName = verification.PacketTypeId.GetDescription();
                    t.Dose = verification.Dose;
                    if (unit != null)
                        t.UnitName = unit.UnitName;
                }
            });
            return Ok(productReciept);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProductReceiptDto productReceiptDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            productReceiptDto.Id = 0;
            //set file path and extension
            if (productReceiptDto.FileModel?.Base64?.Length > 0)
            {
                productReceiptDto.PathName = DocumentService.SaveUploadDocument(productReceiptDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.ProductReceipt, "");
                productReceiptDto.MimeType = productReceiptDto.FileModel.Extension;
            }

            var productReceipt = _mapper.Map<ProductReceipt>(productReceiptDto);
            productReceipt.Status = ProductVerificationStatus.Quarantine;
            productReceipt.IpAddress = _jwtTokenAccesser.IpAddress;
            productReceipt.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _productReceiptRepository.Add(productReceipt);
            if (_uow.Save() <= 0) return Ok(new Exception("Creating product receipt failed on save."));
            _productReceiptRepository.GenerateProductRecieptBarcode(productReceipt);
            return Ok(productReceipt.Id);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var productReceipt = _productReceiptRepository.Find(id);
            var productReceiptDto = _mapper.Map<ProductReceiptDto>(productReceipt);
            productReceiptDto.DepotType = _centralDepotRepository.Find(productReceiptDto.CentralDepotId).DepotType;
            return Ok(productReceiptDto);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProductReceiptDto productReceiptDto)
        {
            if (productReceiptDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            //added by vipul for if document send empty if they cant want to change docuemnt
            var productRec = _productReceiptRepository.Find(productReceiptDto.Id);
            if (productReceiptDto.FileModel?.Base64?.Length > 0)
            {
                productReceiptDto.PathName = DocumentService.SaveUploadDocument(productReceiptDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.ProductReceipt, "");
                productReceiptDto.MimeType = productReceiptDto.FileModel.Extension;
            }
            else
            {
                productReceiptDto.PathName = productRec.PathName;
                productReceiptDto.MimeType = productRec.MimeType;
            }

            var productReceipt = _mapper.Map<ProductReceipt>(productReceiptDto);
            productReceipt.Status = productRec.Status;
            productReceipt.IpAddress = _jwtTokenAccesser.IpAddress;
            productReceipt.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
            _productReceiptRepository.AddOrUpdate(productReceipt);
            if (_uow.Save() <= 0) return Ok(new Exception("Updating product receipt failed on save."));
            return Ok(productReceipt.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _productReceiptRepository.Find(id);

            if (record == null)
                return NotFound();

            var verification = _productVerificationRepository.All.Where(x => x.ProductReceiptId == id).FirstOrDefault();
            if (verification != null)
                _productVerificationRepository.Delete(verification);

            var verificationDetail = _productVerificationDetailRepository.All.Where(x => x.ProductReceiptId == id).FirstOrDefault();
            if (verificationDetail != null)
                _productVerificationDetailRepository.Delete(verificationDetail);

            _productReceiptRepository.Delete(record);


            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _productReceiptRepository.Find(id);

            if (record == null)
                return NotFound();

            _productReceiptRepository.Active(record);
            var verification = _productVerificationRepository.All.Where(x => x.ProductReceiptId == id).FirstOrDefault();
            if (verification != null)
                _productVerificationRepository.Active(verification);

            var verificationDetail = _productVerificationDetailRepository.All.Where(x => x.ProductReceiptId == id).FirstOrDefault();
            if (verificationDetail != null)
                _productVerificationDetailRepository.Active(verificationDetail);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetProductReceipteDropDown/{projectId}")]
        public IActionResult GetProductReceipteDropDown(int projectId)
        {
            return Ok(_productReceiptRepository.GetProductReceipteDropDown(projectId));
        }

        [HttpGet]
        [Route("IsCentralExists/{projectId}")]
        public IActionResult IsCentralExists(int projectId)
        {
            return Ok(_centralDepotRepository.IsCentralExists(projectId));
        }
        [HttpGet]
        [Route("GetLotBatchList/{projectId}")]
        public IActionResult GetLotBatchList(int projectId)
        {
            return Ok(_productReceiptRepository.GetLotBatchList(projectId));
        }
        [HttpGet]
        [Route("ProductRecieptViewBarcode/{productReceiptId}")]
        public IActionResult ProductRecieptViewBarcode(int productReceiptId)
        {
            var productReciept = _productReceiptRepository.Find(productReceiptId);

            var barcodeConfig = _context.PharmacyBarcodeConfig.Include(s => s.BarcodeDisplayInfo).Where(x => x.ProjectId == productReciept.ProjectId && x.BarcodeModuleType == BarcodeModuleType.Verification && x.DeletedBy == null).FirstOrDefault();
            if (barcodeConfig == null)
            {
                ModelState.AddModelError("Message", "Barcode configuration not found.");
                return BadRequest(ModelState);
            }
            return Ok(_productReceiptRepository.GetProductReceiptBarcodeDetail(barcodeConfig, productReceiptId));
        }
    }
}
