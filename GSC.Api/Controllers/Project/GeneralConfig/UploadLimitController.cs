using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.GeneralConfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Respository.Project.GeneralConfig;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Project.GeneralConfig
{
    [Route("api/[controller]")]   
    public class UploadLimitController : BaseController
    {
      
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IUploadlimitRepository _uploadlimitRepository;
        public UploadLimitController(
            IUnitOfWork uow, IMapper mapper, IUploadlimitRepository uploadlimitRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _uploadlimitRepository = uploadlimitRepository;         
        }

        //UploadlimitDto
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var uploadLimit = _uploadlimitRepository.FindBy(x=>x.ProjectId==id).FirstOrDefault();
            var uploadLimitDto = _mapper.Map<UploadlimitDto>(uploadLimit);
            return Ok(uploadLimitDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] UploadlimitDto uploadlimitDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            uploadlimitDto.Id = 0;
            var uploadLimit = _mapper.Map<UploadLimit>(uploadlimitDto);

            _uploadlimitRepository.Add(uploadLimit);
            if (_uow.Save() <= 0) throw new Exception("Creating Upload limit failed on save.");
            return Ok(uploadLimit.Id);
        }
        [HttpPut]
        public IActionResult Put([FromBody] UploadlimitDto uploadlimitDto)
        {
            if (uploadlimitDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var uploadlimit = _mapper.Map<UploadLimit>(uploadlimitDto);

            _uploadlimitRepository.Update(uploadlimit);

            if (_uow.Save() <= 0) throw new Exception("Update upload limit failed on save.");
            return Ok(uploadlimit.Id);
        }
    }
}
