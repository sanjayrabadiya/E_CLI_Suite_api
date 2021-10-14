using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    [ApiController]
    public class SiteTeamController : ControllerBase
    {
        private readonly ISiteTeamRepository _siteTeamRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IUserRepository _userRepository;
        private readonly IRoleRepository _roleRepository;

        public SiteTeamController(ISiteTeamRepository siteTeamRepository,
            IUnitOfWork uow,
            IMapper mapper,
            IGSCContext context,
            IUserRepository userRepository,
            IRoleRepository roleRepository)
        {
            _siteTeamRepository = siteTeamRepository;
            _uow = uow;
            _mapper = mapper;
            _context = context;
            _userRepository = userRepository;
            _roleRepository = roleRepository;

        }

        [HttpGet("{projectid}/{isDeleted:bool?}")]
        public IActionResult Get(int projectid, bool isDeleted)
        {
            var siteteams = _siteTeamRepository.GetSiteTeamList(projectid, isDeleted);
            foreach (var item in siteteams)
            {
                item.UserName = _userRepository.Find(item.UserId).UserName;
                item.ContactEmail = _userRepository.Find(item.UserId).Email;
                item.ContactMobile = _userRepository.Find(item.UserId).Phone;
                item.IsDeleted = isDeleted;
                item.Role = _roleRepository.Find(item.RoleId).RoleName;                
            }
            return Ok(siteteams);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }
            var siteTeam = _siteTeamRepository.Find(id);
            var siteTeamdto = _mapper.Map<SiteTeamDto>(siteTeam);
            siteTeamdto.ContactEmail = _userRepository.Find(siteTeamdto.UserId).Email;
            siteTeamdto.ContactMobile = _userRepository.Find(siteTeamdto.UserId).Phone;
            return Ok(siteTeamdto);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _siteTeamRepository.Find(id);

            if (record == null)
                return NotFound();

            _siteTeamRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _siteTeamRepository.Find(id);

            if (record == null)
                return NotFound();

            SiteTeamDto siteTeamDto = new SiteTeamDto();
            siteTeamDto.Id = record.Id;
            siteTeamDto.RoleId = record.RoleId;
            siteTeamDto.UserId = record.UserId;
            siteTeamDto.ProjectId = record.ProjectId;
            
            var validate = _siteTeamRepository.Duplicate(siteTeamDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _siteTeamRepository.Active(record);
            _uow.Save();

            return Ok();
        }


        [HttpPost]
        public IActionResult Post([FromBody] SiteTeamDto siteTeamDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            siteTeamDto.Id = 0;
            var siteTeam = _mapper.Map<SiteTeam>(siteTeamDto);

            
            var validate = _siteTeamRepository.Duplicate(siteTeamDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _siteTeamRepository.Add(siteTeam);
            if (_uow.Save() <= 0) throw new Exception("Creating Site Team failed on save.");

            return Ok(siteTeam);
        }

        [HttpPut]
        public IActionResult Put([FromBody] SiteTeamDto siteTeamDto)
        {
            if (siteTeamDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var siteTeam = _mapper.Map<SiteTeam>(siteTeamDto);
            siteTeam.Id = siteTeamDto.Id;

            var validate = _siteTeamRepository.Duplicate(siteTeamDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _siteTeamRepository.Update(siteTeam);
            if (_uow.Save() <= 0) throw new Exception("Updating Site Team failed on save.");
            return Ok(siteTeam.Id);
        }

        [HttpGet("GetRoleDropdownForSiteTeam/{projectid}")]
        public IActionResult GetRoleDropdownForSiteTeam(int projectid)
        {
            var data = _siteTeamRepository.GetRoleDropdownForSiteTeam(projectid);
            return Ok(data);
        }

        [HttpGet("GetUserDropdownForSiteTeam/{projectid}/{roleId}")]
        public IActionResult GetUserDropdownForSiteTeam(int projectid, int roleId)
        {
            var data = _siteTeamRepository.GetUserDropdownForSiteTeam(projectid,roleId);
            return Ok(data);
        }

    }
}
