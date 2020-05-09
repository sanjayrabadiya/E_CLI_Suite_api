using GSC.Api.Controllers.Common;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class UserRoleController : BaseController
    {
        private readonly IUserRoleRepository _userRoleRepository;

        public UserRoleController(IUserRoleRepository userRoleRepository)
        {
            _userRoleRepository = userRoleRepository;
        }

        [HttpGet]
        [Route("GetMenuList")]
        public IActionResult GetMenuList()
        {
            var result = _userRoleRepository.GetMenuList();
            return Ok(result);
        }
    }
}