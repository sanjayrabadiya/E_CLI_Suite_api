using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.UserMgt;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.UserMgt
{
    [Route("api/[controller]")]
    public class AppScreenController : BaseController
    {
        private readonly IAppScreenRepository _appScreenRepository;
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public AppScreenController(IAppScreenRepository appScreenRepository,
            IUserRepository userRepository,
            ICompanyRepository companyRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _appScreenRepository = appScreenRepository;
            _userRepository = userRepository;
            _companyRepository = companyRepository;
            _uow = uow;
            _mapper = mapper;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var appScreens = _appScreenRepository.All.Where(x =>
                isDeleted ? x.DeletedDate != null : x.DeletedDate == null
            ).OrderByDescending(x => x.Id).ToList();
            var appScreensDto = _mapper.Map<IEnumerable<AppScreenDto>>(appScreens);

            foreach (var item in appScreensDto)
            {
                var name = _appScreenRepository.All.Where(x => x.Id == item.ParentAppScreenId).Select(c => c.ScreenName)
                    .FirstOrDefault();
                item.ParentScreenName = name;
            }

            return Ok(appScreensDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var appScreen = _appScreenRepository.Find(id);
            var appScreenDto = _mapper.Map<AppScreenDto>(appScreen);
            return Ok(appScreenDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] AppScreenDto appScreenDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            appScreenDto.Id = 0;
            var appScreen = _mapper.Map<AppScreen>(appScreenDto);
            _appScreenRepository.Add(appScreen);
            if (_uow.Save() <= 0) throw new Exception("Creating App Screen failed on save.");
            return Ok(appScreen.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] AppScreenDto appScreenDto)
        {
            if (appScreenDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var appScreen = _mapper.Map<AppScreen>(appScreenDto);

            _appScreenRepository.Update(appScreen);
            if (_uow.Save() <= 0) throw new Exception("Updating App Screen failed on save.");
            return Ok(appScreen.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _appScreenRepository.Find(id);

            if (record == null)
                return NotFound();

            _appScreenRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _appScreenRepository.Find(id);

            if (record == null)
                return NotFound();
            _appScreenRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetAppScreenParentFromDropDown")]
        public IActionResult GetAppScreenParentFromDropDown()
        {
            return Ok(_appScreenRepository.GetAppScreenParentFromDropDown());
        }

        [HttpGet]
        [Route("GetAppScreenChildParentFromDropDown/{Id}")]
        public IActionResult GetAppScreenChildParentFromDropDown(int Id)
        {
            return Ok(_appScreenRepository.GetAppScreenChildParentFromDropDown(Id));
        }

        [HttpGet]
        [Route("GetTableColunms/{Id}")]
        public IActionResult GetTableColunms(int Id)
        {
            return Ok(_appScreenRepository.GetTableColunms(Id));
        }

        [HttpGet]
        [Route("GetMasterTableName")]
        public IActionResult GetMasterTableName()
        {
            return Ok(_appScreenRepository.GetMasterTableName());
        }

        [HttpGet("GetSubMenus")]
        public IActionResult GetSubMenus()
        {
            var appScreens = _appScreenRepository.All.Where(x => x.DeletedDate == null && x.ParentAppScreenId != null
            ).OrderByDescending(x => x.Id).Select(c => new DropDownDto { Id = c.Id, Value = c.ScreenName, Code = c.ScreenCode, IsDeleted = c.DeletedDate != null }).OrderBy(o => o.Value)
               .ToList();
            return Ok(appScreens);
        }


        [HttpGet]
        [Route("GetAppScreenDropDownByParentScreenCode/{screenCode}")]
        public IActionResult GetAppScreenDropDownByParentScreenCode(string screenCode)
        {
            return Ok(_appScreenRepository.GetAppScreenDropDownByParentScreenCode(screenCode));
        }

        [HttpGet]
        [Route("GetTableColunmsIWRS/{Id}")]
        public IActionResult GetTableColunmsIWRS(int Id)
        {
            return Ok(_appScreenRepository.GetTableColunmsIWRS(Id));
        }

    }
}