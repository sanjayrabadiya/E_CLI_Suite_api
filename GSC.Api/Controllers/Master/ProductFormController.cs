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
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ProductFormController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IProductFomRepository _productFormRepository;
        private readonly IUnitOfWork _uow;

        public ProductFormController(IProductFomRepository productFormRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _productFormRepository = productFormRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var units = _productFormRepository.All.Where(x =>
                (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                && isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var productFormDto = _mapper.Map<IEnumerable<ProductFormDto>>(units);
            return Ok(productFormDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var productForm = _productFormRepository.Find(id);
            var productFormDto = _mapper.Map<ProductFormDto>(productForm);
            return Ok(productFormDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ProductFormDto productFormDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            productFormDto.Id = 0;
            var productForm = _mapper.Map<MProductForm>(productFormDto);
            var validate = _productFormRepository.Duplicate(productForm);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _productFormRepository.Add(productForm);
            if (_uow.Save() <= 0) throw new Exception("Creating Product Form failed on save.");
            return Ok(productForm.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] ProductFormDto productFormDto)
        {
            if (productFormDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var productForm = _mapper.Map<MProductForm>(productFormDto);
            var validate = _productFormRepository.Duplicate(productForm);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by Vipul for effective Date on 14-10-2019 */
            Delete(productForm.Id);
            productForm.Id = 0;
            _productFormRepository.Add(productForm);

            if (_uow.Save() <= 0) throw new Exception("Updating Product Form failed on save.");
            return Ok(productForm.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _productFormRepository.Find(id);

            if (record == null)
                return NotFound();

            _productFormRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _productFormRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _productFormRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _productFormRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetProductFormDropDown")]
        public IActionResult GetProductFormDropDown()
        {
            return Ok(_productFormRepository.GetProductFormDropDown());
        }
    }
}