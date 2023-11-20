using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.SupplyManagement;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace GSC.Api.Controllers.SupplyManagement
{
    [Route("api/[controller]")]
    [ApiController]
    public class SupplyManagementFactorMappingController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ISupplyManagementFactorMappingRepository _supplyManagementFactorMappingRepository;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        public SupplyManagementFactorMappingController(ISupplyManagementFactorMappingRepository supplyManagementFactorMappingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IGSCContext context)
        {
            _supplyManagementFactorMappingRepository = supplyManagementFactorMappingRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        [HttpGet("GetSupplyFactorMappingList/{projectId}/{isDeleted:bool?}")]
        public IActionResult Get(int projectId, bool isDeleted)
        {
            var productTypes = _supplyManagementFactorMappingRepository.GetSupplyFactorMappingList(isDeleted, projectId);
            return Ok(productTypes);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var centralDepo = _supplyManagementFactorMappingRepository.Find(id);
            var centralDepoDto = _mapper.Map<SupplyManagementFactorMappingDto>(centralDepo);
            return Ok(centralDepoDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] SupplyManagementFactorMappingDto centralDepotDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            centralDepotDto.Id = 0;
            var centralDepot = _mapper.Map<SupplyManagementFactorMapping>(centralDepotDto);

            var validate = _supplyManagementFactorMappingRepository.Validation(centralDepot);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _supplyManagementFactorMappingRepository.Add(centralDepot);
            if (_uow.Save() <= 0) throw new Exception("Creating factor mapping failed on save.");
            return Ok(centralDepot.Id);
        }


        [HttpPut]
        public IActionResult Put([FromBody] SupplyManagementFactorMappingDto centralDepotDto)
        {
            if (centralDepotDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var centralDepot = _mapper.Map<SupplyManagementFactorMapping>(centralDepotDto);

            var validate = _supplyManagementFactorMappingRepository.Validation(centralDepot);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            centralDepot.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            centralDepot.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementFactorMappingRepository.Update(centralDepot);

            if (_uow.Save() <= 0) throw new Exception("Updating factor mapping failed on save.");
            return Ok(centralDepot.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _supplyManagementFactorMappingRepository.Find(id);

            if (record == null)
                return NotFound();
            record.ReasonOth = _jwtTokenAccesser.GetHeader("audit-reason-oth");
            record.AuditReasonId = int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"));
            _supplyManagementFactorMappingRepository.Update(record);
            _uow.Save();

            _supplyManagementFactorMappingRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _supplyManagementFactorMappingRepository.Find(id);

            if (record == null)
                return NotFound();

            _supplyManagementFactorMappingRepository.Active(record);
            _uow.Save();

            return Ok();
        }
        [HttpGet("GetFactorsByFactorSetting/{projectId}")]
        public IActionResult GetFactorsByFactorSetting(int projectId)
        {
            var setting = _context.SupplyManagementFectorDetail.Include(s => s.SupplyManagementFector).Where(s => s.SupplyManagementFector.ProjectId == projectId
                          && s.SupplyManagementFector.DeletedDate == null && s.DeletedDate == null).Select(s => (int)s.Fector).ToList();
            if (setting == null)
                return Ok(new List<DropDownEnum>());

            var fectore = Enum.GetValues(typeof(Fector))
                .Cast<Fector>().Select(e => new DropDownEnum
                {
                    Id = Convert.ToInt16(e),
                    Value = e.GetDescription()
                }).Where(s => setting.Contains(s.Id)).OrderBy(o => o.Id).ToList();

            return Ok(fectore);
        }
    }
}
