using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class ProductTypeController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IProductTypeRepository _productTypeRepository;
        private readonly IUnitOfWork _uow;

        public ProductTypeController(IProductTypeRepository productTypeRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _productTypeRepository = productTypeRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var productTypes = _productTypeRepository
                .All.Where(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
                ).OrderByDescending(x => x.Id).ToList();
            var productTypesDto = _mapper.Map<IEnumerable<ProductTypeDto>>(productTypes);
            productTypesDto.ForEach(b =>
            {
                b.CreatedByUser = _userRepository.Find(b.CreatedBy).UserName;
                if (b.ModifiedBy != null)
                    b.ModifiedByUser = _userRepository.Find((int)b.ModifiedBy).UserName;
                if (b.DeletedBy != null)
                    b.DeletedByUser = _userRepository.Find((int)b.DeletedBy).UserName;
                if (b.CompanyId != null)
                    b.CompanyName = _companyRepository.Find((int)b.CompanyId).CompanyName;
            });
            return Ok(productTypesDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var productType = _productTypeRepository.Find(id);
            var productTypeDto = _mapper.Map<ProductTypeDto>(productType);
            return Ok(productTypeDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] ProductTypeDto productTypeDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            productTypeDto.Id = 0;
            var productType = _mapper.Map<ProductType>(productTypeDto);
            var validate = _productTypeRepository.Duplicate(productType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _productTypeRepository.Add(productType);
            if (_uow.Save() <= 0) throw new Exception("Creating product type failed on save.");
            return Ok(productType.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] ProductTypeDto productTypeDto)
        {
            if (productTypeDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var productType = _mapper.Map<ProductType>(productTypeDto);
            var validate = _productTypeRepository.Duplicate(productType);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            /* Added by swati for effective Date on 02-06-2019 */
            _productTypeRepository.AddOrUpdate(productType);

            if (_uow.Save() <= 0) throw new Exception("Updating product type failed on save.");
            return Ok(productType.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _productTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            _productTypeRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _productTypeRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _productTypeRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _productTypeRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetProductTypeDropDown")]
        public IActionResult GetProductTypeDropDown()
        {
            return Ok(_productTypeRepository.GetProductTypeDropDown());
        }
    }
}