using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class ReportFavouriteScreenController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IReportFavouriteScreenRepository _reportFavouriteScreenRepository;

        public ReportFavouriteScreenController(IReportFavouriteScreenRepository reportFavouriteScreenRepository,
            IUnitOfWork uow)
        {
            _reportFavouriteScreenRepository = reportFavouriteScreenRepository;
            _uow = uow;
        }

        [HttpGet]
        [AllowAnonymous]
        public IActionResult Get()
        {
            var favoriteScreens = _reportFavouriteScreenRepository.GetFavoriteByUserId();

            return Ok(favoriteScreens);
        }

        [HttpGet("Favorite/{ReportId}")]
        public IActionResult Favorite(int ReportId)
        {
            if (ReportId <= 0) return BadRequest();
            _reportFavouriteScreenRepository.Favorite(ReportId);
            _uow.Save();

            return Ok();
        }
    }
}
