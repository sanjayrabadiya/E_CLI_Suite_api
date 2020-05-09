using GSC.Respository.Common;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Common
{
    [Route("api/[controller]")]
    public class UserRecentItemController : BaseController
    {
        private readonly IUserRecentItemRepository _userRecentItemRepository;

        public UserRecentItemController(IUserRecentItemRepository userRecentItemRepository)
        {
            _userRecentItemRepository = userRecentItemRepository;
        }


        [HttpGet]
        [AllowAnonymous]
        public IActionResult GetGetRecentItemByUser()
        {
            return Ok(_userRecentItemRepository.GetRecentItemByUser());
        }
    }
}