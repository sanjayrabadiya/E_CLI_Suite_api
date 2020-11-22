using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ProductController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProductRepository _productRepository;
        private readonly IUnitOfWork _uow;

        public ProductController(IProductRepository productRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _productRepository = productRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {

            var products = _productRepository.FindByInclude(x => (x.CompanyId == null
                                                                  || x.CompanyId == _jwtTokenAccesser.CompanyId) &&
                                                                 isDeleted ? x.DeletedDate != null : x.DeletedDate == null, x => x.Project,
                x => x.ProductType).ToList();

            var productsDto = _mapper.Map<IEnumerable<ProductDto>>(products);
            return Ok(productsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var product = _productRepository.Find(id);
            var productDto = _mapper.Map<ProductDto>(product);
            return Ok(productDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ProductDto productDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            productDto.Id = 0;
            var product = _mapper.Map<Product>(productDto);
            var validate = _productRepository.Duplicate(product);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _productRepository.Add(product);
            if (_uow.Save() <= 0) throw new Exception("Creating product type failed on save.");
            return Ok(product.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] ProductDto productDto)
        {
            if (productDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var product = _mapper.Map<Product>(productDto);
            var validate = _productRepository.Duplicate(product);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _productRepository.Update(product);
            if (_uow.Save() <= 0) throw new Exception("Updating product type failed on save.");
            return Ok(product.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _productRepository.Find(id);

            if (record == null)
                return NotFound();

            _productRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _productRepository.Find(id);

            if (record == null)
                return NotFound();
            _productRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetProductDropDown")]
        public IActionResult GetProductDropDown()
        {
            return Ok(_productRepository.GetProductDropDown());
        }
    }
}