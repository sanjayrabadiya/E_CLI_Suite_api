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
    public class BlockCategoryController : BaseController
    {
        private readonly IBlockCategoryRepository _blockCategoryRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public BlockCategoryController(IBlockCategoryRepository blockCategoryRepository,
            IUnitOfWork uow, IMapper mapper,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IJwtTokenAccesser jwtTokenAccesser)
        {
            _blockCategoryRepository = blockCategoryRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var blockcategory = _blockCategoryRepository.GetBlockCategoryList(isDeleted);
            return Ok(blockcategory);

            //var blockCategorys = _blockCategoryRepository.All.Where(x =>isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            //).OrderByDescending(x => x.Id).ToList();
            //var blockCategorysDto = _mapper.Map<IEnumerable<BlockCategoryDto>>(blockCategorys);
            //return Ok(blockCategorysDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var blockCategory = _blockCategoryRepository.Find(id);
            var blockCategoryDto = _mapper.Map<BlockCategoryDto>(blockCategory);
            return Ok(blockCategoryDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] BlockCategoryDto blockCategoryDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            blockCategoryDto.Id = 0;
            var blockCategory = _mapper.Map<BlockCategory>(blockCategoryDto);
            var validate = _blockCategoryRepository.Duplicate(blockCategory);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _blockCategoryRepository.Add(blockCategory);
            if (_uow.Save() <= 0) throw new Exception("Creating Contact Type failed on save.");
            return Ok(blockCategory.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] BlockCategoryDto blockCategoryDto)
        {
            if (blockCategoryDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var blockCategory = _mapper.Map<BlockCategory>(blockCategoryDto);
            var validate = _blockCategoryRepository.Duplicate(blockCategory);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _blockCategoryRepository.AddOrUpdate(blockCategory);

            if (_uow.Save() <= 0) throw new Exception("Updating Block Category failed on save.");
            return Ok(blockCategory.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _blockCategoryRepository.Find(id);

            if (record == null)
                return NotFound();

            _blockCategoryRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _blockCategoryRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _blockCategoryRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _blockCategoryRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetBlockCategoryDropDown")]
        public IActionResult GetBlockCategoryDropDown()
        {
            return Ok(_blockCategoryRepository.GetBlockCategoryDropDown());
        }
    }
}