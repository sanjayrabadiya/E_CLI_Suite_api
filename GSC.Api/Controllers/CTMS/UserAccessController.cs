using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    public class UserAccessController : BaseController
    {
        private readonly IUserAccessRepository _userAccessRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public UserAccessController(IUserAccessRepository userAccessRepository,

            IUnitOfWork uow, IMapper mapper)
        {
            _userAccessRepository = userAccessRepository;
            _uow = uow;
            _mapper = mapper;
        }

        [HttpGet("{isDeleted:bool?}/{studyId}/{siteId}")]
        public IActionResult Get(bool isDeleted, int studyId, int siteId)
        {
            var userAccess = _userAccessRepository.GetUserAccessList(isDeleted, studyId, siteId);
            return Ok(userAccess);
        }

        [HttpPost]
        public IActionResult Post([FromBody] UserAccessDto userAccessDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var validate = _userAccessRepository.Duplicate(userAccessDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var ActiveData = _userAccessRepository.getActive(userAccessDto);
            if(ActiveData.Count==0)
                _userAccessRepository.AddSiteUserAccesse(userAccessDto);

            return Ok(true);
        }

        [HttpPost("GetRevokeUser")]
        public ActionResult GetRevokeUser([FromBody] List<int> mySelection)
        {
            foreach (var item in mySelection)
            {
                var record = _userAccessRepository.Find(item);
                if (record == null)
                    return NotFound();
                _userAccessRepository.Delete(record);
                _uow.Save();
            }
            return Ok(true);
        }

        [HttpGet]
        [Route("GetRollUserDropDown")]
        public IActionResult GetRollUserDropDown()
        {
            return Ok(_userAccessRepository.GetRollUserDropDown());
        }

        [HttpGet("GetUserAccessHistory/{id}")]
        public IActionResult GetUserAccessHistory(int id)
        {
            if (id <= 0) return BadRequest();

            var result = _userAccessRepository.GetUserAccessHistory(id);
            return Ok(result);
        }
    }
}
