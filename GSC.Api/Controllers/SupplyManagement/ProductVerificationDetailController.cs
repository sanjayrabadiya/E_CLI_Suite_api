using System;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Respository.SupplyManagement;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProductVerificationDetailController : BaseController
    {
        
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IProductVerificationDetailRepository _productVerificationDetailRepository;

        public ProductVerificationDetailController(
            IMapper mapper
            
            , IUnitOfWork uow
            , IProductVerificationDetailRepository productVerificationDetailRepository)
        {
            
            _mapper = mapper;
            _uow = uow;
            _productVerificationDetailRepository = productVerificationDetailRepository;
        }

        [HttpGet("GetProductVerificationDetailList/{ProductReceiptId}")]
        public IActionResult GetProductVerificationDetailList(int ProductReceiptId)
        {
            var productVerification = _productVerificationDetailRepository.GetProductVerificationDetailList(ProductReceiptId);
            return Ok(productVerification);
        }

        [HttpPost]
        public IActionResult Post([FromBody] ProductVerificationDetailDto productVerificationDetailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            productVerificationDetailDto.Id = 0;

            var productVerificationDetail = _mapper.Map<ProductVerificationDetail>(productVerificationDetailDto);

            _productVerificationDetailRepository.Add(productVerificationDetail);

            if (_uow.Save() <= 0) return Ok(new Exception("Creating product verification failed on save."));

            return Ok(productVerificationDetail.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProductVerificationDetailDto productVerificationDetailDto)
        {
            if (productVerificationDetailDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var productVerificationDetail = _mapper.Map<ProductVerificationDetail>(productVerificationDetailDto);

            _productVerificationDetailRepository.AddOrUpdate(productVerificationDetail);

            if (_uow.Save() <= 0) return Ok(new Exception("Updating product verification failed on save."));
            return Ok(productVerificationDetail.Id);
        }
    }
}
