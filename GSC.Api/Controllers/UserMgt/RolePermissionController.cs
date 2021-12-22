using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class RolePermissionController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IRolePermissionRepository _rolePermissionRepository;
        private readonly IMapper _mapper;

        public RolePermissionController(IRolePermissionRepository rolePermissionRepository, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var permissionDtos = _rolePermissionRepository.GetByRoleId(id);

            return Ok(permissionDtos);
        }

        [HttpGet]
        public IActionResult Get()
        {
            var permissionDtos =
                _rolePermissionRepository.GetByUserId(_jwtTokenAccesser.UserId, _jwtTokenAccesser.RoleId);

            return Ok(permissionDtos);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] List<RolePermission> rolePermissions)
        {
            if (!ModelState.IsValid || !rolePermissions.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _rolePermissionRepository.Save(rolePermissions);

            return Ok();
        }
        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] List<RolePermission> rolePermissions)
        {
            if (!ModelState.IsValid || !rolePermissions.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _rolePermissionRepository.updatePermission(rolePermissions);

            return Ok();
        }

        [HttpGet("GetScreenByRole/{roleid}")]
        public IActionResult GetScreenByRole(int roleid)
        {
            var rolePermission = _rolePermissionRepository.FindByInclude(q => q.UserRoleId == roleid && q.DeletedDate == null, x => x.AppScreens)
                .Where(s => s.AppScreens.ParentAppScreenId == null).ToList();

            return Ok(_mapper.Map<List<RolePermissionDto>>(rolePermission));
        }
    }
}