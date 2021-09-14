using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.SupplyManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

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


        public ProductReceiptController(IProductReceiptRepository productReceiptRepository,
            ICentralDepotRepository centralDepotRepository,
            IUploadSettingRepository uploadSettingRepository,
        IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _productReceiptRepository = productReceiptRepository;
            _centralDepotRepository = centralDepotRepository;
            _uploadSettingRepository = uploadSettingRepository;

            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        //[HttpGet("{isDeleted:bool?}")]
        //public IActionResult Get(bool isDeleted)
        //{
        //    return Ok(_productReceiptRepository.GetProductReceiptList(isDeleted));
        //}

        [HttpGet("GetProductReceiptList/{projectId}/{isDeleted:bool?}")]
        public IActionResult GetProductReceiptList(int projectId, bool isDeleted)
        {
            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            var productReciept = _productReceiptRepository.GetProductReceiptList(projectId, isDeleted);
            productReciept.ForEach(t => t.PathName = documentUrl + t.PathName);
            return Ok(productReciept);

            //return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProductReceiptDto productReceiptDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            productReceiptDto.Id = 0;
            //set file path and extension
            if (productReceiptDto.FileModel?.Base64?.Length > 0)
            {
                productReceiptDto.PathName = DocumentService.SaveUploadDocument(productReceiptDto.FileModel, _uploadSettingRepository.GetDocumentPath(),_jwtTokenAccesser.CompanyId.ToString(), FolderType.ProductReceipt,"");
                productReceiptDto.MimeType = productReceiptDto.FileModel.Extension;
                productReceiptDto.FileName = "ProductReceipt_" + DateTime.Now.Ticks + "." + productReceiptDto.FileModel.Extension;
            }


            var productReceipt = _mapper.Map<ProductReceipt>(productReceiptDto);
            var validate = _productReceiptRepository.Duplicate(productReceipt);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _productReceiptRepository.Add(productReceipt);
            if (_uow.Save() <= 0) throw new Exception("Creating product receipt failed on save.");
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
                productReceiptDto.PathName = DocumentService.SaveUploadDocument(productReceiptDto.FileModel, _uploadSettingRepository.GetDocumentPath(),_jwtTokenAccesser.CompanyId.ToString(), FolderType.ProductReceipt, "");
                productReceiptDto.MimeType = productReceiptDto.FileModel.Extension;
                productReceiptDto.FileName = "ProductReceipt_" + DateTime.Now.Ticks + "." + productReceiptDto.FileModel.Extension;
            }
            else {
                productReceiptDto.PathName = productRec.PathName;
                productReceiptDto.MimeType = productRec.MimeType;
                productReceiptDto.FileName = productRec.FileName;
            }

            var productReceipt = _mapper.Map<ProductReceipt>(productReceiptDto);
            var validate = _productReceiptRepository.Duplicate(productReceipt);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _productReceiptRepository.AddOrUpdate(productReceipt);

            if (_uow.Save() <= 0) throw new Exception("Updating product receipt failed on save.");
            return Ok(productReceipt.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _productReceiptRepository.Find(id);

            if (record == null)
                return NotFound();

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
            var validate = _productReceiptRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _productReceiptRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetProductReceipteDropDown/{projectId}")]
        public IActionResult GetProductReceipteDropDown(int projectId)
        {
            return Ok(_productReceiptRepository.GetProductReceipteDropDown(projectId));
        }
    }
}
