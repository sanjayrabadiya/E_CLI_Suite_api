using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.Master;
using GSC.Respository.SupplyManagement;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using GSC.Shared.Security;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementKITController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementKITRepository _supplyManagementKITRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        public SupplyManagementKITController(ISupplyManagementKITRepository supplyManagementKITRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
        {
            _supplyManagementKITRepository = supplyManagementKITRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementKITRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementKITDto>(centralDepo);
            return Ok(centralDepoDto);
        }

        [HttpGet("GetKITList/{projectId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectId, bool isDeleted)
        {
            var productTypes = _supplyManagementKITRepository.GetKITList(isDeleted, projectId);
            return Ok(productTypes);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] SupplyManagementKITDto supplyManagementUploadFileDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            supplyManagementUploadFileDto.Id = 0;

            var supplyManagementUploadFile = _mapper.Map<SupplyManagementKIT>(supplyManagementUploadFileDto);
            supplyManagementUploadFile.TotalUnits = (supplyManagementUploadFileDto.NoOfImp * supplyManagementUploadFileDto.NoofPatient * supplyManagementUploadFileDto.NoOfKits);
            _supplyManagementKITRepository.Add(supplyManagementUploadFile);
           
            if (_uow.Save() <= 0) throw new Exception("Creating Kit Creation failed on save.");
           
          
            supplyManagementUploadFile.KitNo = supplyManagementUploadFileDto.Id;
            _context.Entry(supplyManagementUploadFile).State = EntityState.Detached;
            _supplyManagementKITRepository.Update(supplyManagementUploadFile);
            if (_uow.Save() <= 0) throw new Exception("Creating Kit Creation failed on save.");

            return Ok(supplyManagementUploadFile.Id);

        }

        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementKITDto supplyManagementUploadFileDto)
        {
            if (supplyManagementUploadFileDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var supplyManagementUploadFile = _mapper.Map<SupplyManagementKIT>(supplyManagementUploadFileDto);
            supplyManagementUploadFile.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            supplyManagementUploadFile.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            supplyManagementUploadFile.TotalUnits = (supplyManagementUploadFileDto.NoOfImp * supplyManagementUploadFileDto.NoofPatient * supplyManagementUploadFileDto.NoOfKits);
            _supplyManagementKITRepository.Update(supplyManagementUploadFile);

            if (_uow.Save() <= 0) throw new Exception("Updating Kit Creation failed on action.");
            return Ok(supplyManagementUploadFile.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementKITRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementKITRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementKITRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementKITRepository.Active(record);
            _uow.Save();

            return Ok();
        }
    }
}
