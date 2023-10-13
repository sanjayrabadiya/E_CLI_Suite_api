using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using GSC.Shared.DocumentService;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Project.Design
{
    [Route("api/[controller]")]
    [ApiController]
    public class VisitEmailConfigurationController : BaseController
    {
        private readonly IVisitEmailConfigurationRepository _visitEmailConfigurationRepository;
        private readonly IVisitEmailConfigurationRolesRepository _visitEmailConfigurationRolesRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;

        public VisitEmailConfigurationController(
            IVisitEmailConfigurationRepository visitEmailConfigurationRepository,
            IVisitEmailConfigurationRolesRepository visitEmailConfigurationRolesRepository,
        IUnitOfWork uow, IMapper mapper,
            IGSCContext context
          )
        {
            _visitEmailConfigurationRepository = visitEmailConfigurationRepository;
            _visitEmailConfigurationRolesRepository = visitEmailConfigurationRolesRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}/{projectDesignVisitId}")]
        public IActionResult Get(bool isDeleted, int projectDesignVisitId)
        {
            var visitEmail = _visitEmailConfigurationRepository.GetVisitEmailConfigurationList(isDeleted, projectDesignVisitId);
            return Ok(visitEmail);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var visitEmail = _visitEmailConfigurationRepository.Find(id);
            var visitEmailConfigurationDto = _mapper.Map<VisitEmailConfigurationDto>(visitEmail);
            visitEmailConfigurationDto.RoleId = _visitEmailConfigurationRolesRepository.All.Where(x => x.VisitEmailConfigurationId == id
            && x.DeletedDate == null).Select(x => x.SecurityRoleId).ToArray();
            return Ok(visitEmailConfigurationDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] VisitEmailConfigurationDto emailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            emailDto.Id = 0;

            if (emailDto.VisitEmailConfigurationRoles != null)
                emailDto.VisitEmailConfigurationRoles = emailDto.VisitEmailConfigurationRoles.Where(x => !x.IsDeleted).ToList();

            var email = _mapper.Map<VisitEmailConfiguration>(emailDto);
            var validate = _visitEmailConfigurationRepository.Duplicate(email);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _visitEmailConfigurationRepository.Add(email);

            int i = 0;
            foreach (var item in email.VisitEmailConfigurationRoles)
            {
                item.SecurityRoleId = emailDto.RoleId[i];
                _visitEmailConfigurationRolesRepository.Add(item);
                i++;
            }

            if (_uow.Save() <= 0) throw new Exception("Creating email visit template failed on save.");
            return Ok(email.Id);
        }

        [TransactionRequired]
        [HttpPut]
        public IActionResult Put([FromBody] VisitEmailConfigurationDto emailDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var email = _mapper.Map<VisitEmailConfiguration>(emailDto);
            var validate = _visitEmailConfigurationRepository.Duplicate(email);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _visitEmailConfigurationRepository.Update(email);
            _visitEmailConfigurationRolesRepository.updateVisitEmailRole(emailDto);
            if (_uow.Save() <= 0) throw new Exception("Updating email visit template failed on save.");
            return Ok(email.Id);
        }

        [TransactionRequired]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var record = _visitEmailConfigurationRepository.FindByInclude(x => x.Id == id, x => x.VisitEmailConfigurationRoles)
             .FirstOrDefault();

            if (record == null)
                return NotFound();

            if (record != null && record.VisitEmailConfigurationRoles != null)
                record.VisitEmailConfigurationRoles = record.VisitEmailConfigurationRoles.Where(x => x.DeletedDate == null).ToList();

            _visitEmailConfigurationRepository.Delete(record);

            record.VisitEmailConfigurationRoles.ToList().ForEach(r =>
            {
                _visitEmailConfigurationRolesRepository.Delete(r);
            });

            _uow.Save();
            return Ok();
        }
    }
}
