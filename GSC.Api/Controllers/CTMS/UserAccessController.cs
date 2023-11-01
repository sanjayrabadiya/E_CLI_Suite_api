using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Respository.CTMS;
using GSC.Data.Entities.CTMS;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Linq;

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

        //[HttpGet("{id}")]
        //public IActionResult Get(int id)
        //{
        //    if (id <= 0) return BadRequest();
        //    var taskmaster = _userAccessRepository.FindByInclude(x => x.Id == id, x => x.siteUserAccess).FirstOrDefault();
        //    if (taskmaster != null && taskmaster.siteUserAccess != null)
        //        taskmaster.siteUserAccess = taskmaster.siteUserAccess.Where(x => x.DeletedDate == null).ToList();
        //    var taskDto = _mapper.Map<UserAccessDto>(taskmaster);
        //    return Ok(taskDto);
        //}

        [HttpPost]
        public IActionResult Post([FromBody] UserAccessDto userAccessDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            //userAccessDto.Id = 0;
            //var userAccessData = _mapper.Map<UserAccess>(userAccessDto);
            //_userAccessRepository.Add(userAccessData);
            //if (_uow.Save() <= 0) throw new Exception("Creating User Access on save.");
            //userAccessDto.Id = userAccessData.Id;

            var validate = _userAccessRepository.Duplicate(userAccessDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _userAccessRepository.AddSiteUserAccesse(userAccessDto);
            return Ok(true);
        }

        //[HttpPut]
        //public IActionResult Put([FromBody] UserAccessDto userAccessDto)
        //{
        //    if (userAccessDto.Id <= 0) return BadRequest();
        //    if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
        //    var userAccessdata = _mapper.Map<UserAccess>(userAccessDto);
        //    _userAccessRepository.UpdateSiteUserAccess(userAccessdata);
        //    _userAccessRepository.Update(userAccessdata);
        //    if (_uow.Save() <= 0) throw new Exception("Updating User Access failed on save.");
        //    return Ok(userAccessdata.Id);
        //}

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _userAccessRepository.Find(id);
            if (record == null)
                return NotFound();
            _userAccessRepository.Delete(record);
           // _userAccessRepository.DeleteSiteUserAccess(id);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _userAccessRepository.Find(id);

            if (record == null)
                return NotFound();
            //_userAccessRepository.ActiveSiteUserAccess(id);

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
