using System.Collections.Generic;
using System.Linq;
using GSC.Api.Controllers.Common;
using GSC.Data.Entities.UserMgt;
using GSC.Helper;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class RolePermissionController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IRolePermissionRepository _rolePermissionRepository;

        public RolePermissionController(IRolePermissionRepository rolePermissionRepository, IJwtTokenAccesser jwtTokenAccesser)
        {
            _rolePermissionRepository = rolePermissionRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
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
        public IActionResult Post([FromBody] List<RolePermission> rolePermissions)
        {
            if (!ModelState.IsValid || !rolePermissions.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _rolePermissionRepository.Save(rolePermissions);

            return Ok();
        }
        [HttpPut]
        public IActionResult Put([FromBody] List<RolePermission> rolePermissions)
        {
            if (!ModelState.IsValid || !rolePermissions.Any()) return new UnprocessableEntityObjectResult(ModelState);

            _rolePermissionRepository.updatePermission(rolePermissions);

            return Ok();
        }

    }
}