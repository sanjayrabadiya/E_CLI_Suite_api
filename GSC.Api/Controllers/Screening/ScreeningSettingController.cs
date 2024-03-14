using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Respository.Screening;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    [ApiController]
    public class ScreeningSettingController : BaseController
    {
        private readonly IUnitOfWork _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IScreeningSettingRepository _screeningSettingRepository;
        public ScreeningSettingController(
            IScreeningSettingRepository screeningSettingRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IUnitOfWork uow)
        {
            _uow = uow;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningSettingRepository = screeningSettingRepository;
        }

        // Get project default data
        [HttpGet]
        [Route("GetProjectDefaultData")]
        public IActionResult GetProjectDefaultData()
        {
            return Ok(_screeningSettingRepository.GetProjectDefaultData());
        }

        // Get project default data
        [HttpPost]
        public IActionResult Post([FromBody] ScreeningSettingDto screeningSettingDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var exists = _screeningSettingRepository.All.Where(x => x.UserId == _jwtTokenAccesser.UserId 
                && x.RoleId == _jwtTokenAccesser.RoleId && x.DeletedDate == null).FirstOrDefault();

            if (exists == null)
            {
                ScreeningSetting screeningSetting = new ScreeningSetting();
                screeningSetting.ProjectId = screeningSettingDto.ProjectId;
                screeningSetting.VisitId = screeningSettingDto.VisitId;
                screeningSetting.RoleId = _jwtTokenAccesser.RoleId;
                screeningSetting.UserId = _jwtTokenAccesser.UserId;

                _screeningSettingRepository.Add(screeningSetting);
            }
            else
            {
                exists.ProjectId = screeningSettingDto.ProjectId;
                exists.VisitId= screeningSettingDto.VisitId;
                _screeningSettingRepository.Update(exists);
            }
            if (_uow.Save() <= 0) return Ok(new Exception("Set default project failed."));
            return Ok();
        }
    }
}
