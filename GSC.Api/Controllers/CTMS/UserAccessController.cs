using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;

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
            _userAccessRepository.AddSiteUserAccesse(userAccessDto);
            return Ok(true);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _userAccessRepository.Find(id);
            if (record == null)
                return NotFound();
            _userAccessRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _userAccessRepository.Find(id);

            if (record == null)
                return NotFound();
            var validate = _userAccessRepository.DuplicateIActive(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _userAccessRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetRollUserDropDown/{projectId}")]

        public IActionResult getSelectDateDrop(int projectId)
        {
            return Ok(_userAccessRepository.GetRollUserDropDown(projectId));
        }
    }
}
