using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    [ApiController]
    public class AppScreenPatientRightsController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IAppScreenPatientRightsRepository _appScreenPatientRightsRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public AppScreenPatientRightsController(IJwtTokenAccesser jwtTokenAccesser,
            IUnitOfWork uow, IMapper mapper,
            IAppScreenPatientRightsRepository appScreenPatientRightsRepository)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _appScreenPatientRightsRepository = appScreenPatientRightsRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet]
        [Route("GetAppScreenPatientList/{projectid}")]
        public IActionResult GetAppScreenPatientList(int projectid)
        {
            var data = _appScreenPatientRightsRepository.GetAppScreenPatientList(projectid);
            return Ok(data);
        }

        [HttpGet]
        [Route("GetAppScreenPatientModules/{projectid}")]
        public IActionResult GetAppScreenPatientModules(int projectid)
        {
            var data = _appScreenPatientRightsRepository.GetAppScreenPatientModules(projectid);
            return Ok(data);
        }

        [HttpPost]
        public IActionResult Post([FromBody] List<AppScreenPatientRightsDto> data)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            if (data.Count > 0)
            {
                var deletedata = _appScreenPatientRightsRepository.FindBy(x => x.ProjectId == data[0].ProjectId).ToList();
                foreach (var item in deletedata)
                {
                    _appScreenPatientRightsRepository.Remove(item);
                }

                var adddata = data.Where(x => x.IsChecked == true).ToList();
                foreach (var item in adddata)
                {
                    var appScreenPatientRights = _mapper.Map<AppScreenPatientRights>(item);
                    _appScreenPatientRightsRepository.Add(appScreenPatientRights);
                }
                _uow.Save();
            }
            return Ok();
        }

    }
}
