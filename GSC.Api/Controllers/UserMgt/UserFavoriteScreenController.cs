using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Domain.Context;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class UserFavoriteScreenController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IUserFavoriteScreenRepository _userFavoriteScreenRepository;

        public UserFavoriteScreenController(IUserFavoriteScreenRepository userFavoriteScreenRepository,
            IUnitOfWork uow)
        {
            _userFavoriteScreenRepository = userFavoriteScreenRepository;
            _uow = uow;
        }

        [HttpGet]
        public IActionResult Get()
        {
            var favoriteScreens = _userFavoriteScreenRepository.GetFavoriteByUserId();

            return Ok(favoriteScreens);
        }

        [HttpGet("Favorite/{appScreenId}")]
        public IActionResult Favorite(int appScreenId)
        {
            if (appScreenId <= 0) return BadRequest();
            _userFavoriteScreenRepository.Favorite(appScreenId);
            _uow.Save();

            return Ok();
        }
    }
}