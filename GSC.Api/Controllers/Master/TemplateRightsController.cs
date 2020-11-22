using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Respository.Master;
using GSC.Shared;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.Master
{
    [Route("api/[controller]")]
    public class TemplateRightsController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly ITemplateRightsRepository _templateRightsRepository;
        private readonly ITemplateRightsRoleListRepository _templateRightsRoleListRepository;
        private readonly IUnitOfWork _uow;

        public TemplateRightsController(ITemplateRightsRepository templateRightsRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            ITemplateRightsRoleListRepository templateRightsRoleListRepository
        )
        {
            _templateRightsRepository = templateRightsRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _templateRightsRoleListRepository = templateRightsRoleListRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            //var templaterights = _templateRightsRepository.All.Where(x =>
            //    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
            //    && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)
            //).ToList();


            var templaterights = _templateRightsRepository.FindByInclude(x => (x.CompanyId == null
                                                                               || x.CompanyId ==
                                                                               _jwtTokenAccesser.CompanyId) &&
                                                                              isDeleted ? x.DeletedDate != null : x.DeletedDate == null,
                x => x.VariableTemplate).ToList();


            var templateRightsDto = _mapper.Map<IEnumerable<TemplateRightsDto>>(templaterights);
            return Ok(templateRightsDto);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var templaterights = _templateRightsRepository.Find(id);
            var templateRightsDto = _mapper.Map<TemplateRightsDto>(templaterights);
            return Ok(templateRightsDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] TemplateRightsDto templateRightsDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            templateRightsDto.Id = 0;
            var templaterights = _mapper.Map<TemplateRights>(templateRightsDto);
            var validate = _templateRightsRepository.Duplicate(templaterights);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }


            _templateRightsRepository.Add(templaterights);

            var returnValue = _uow.Save();

            returnValue = _templateRightsRepository.All.OrderByDescending(x => x.Id).FirstOrDefault().Id;
            if (returnValue > 0)
            {
                var rolesplit = templaterights.RoleId.Split(",");
                for (var i = 0; i < rolesplit.Length; i++)
                {
                    var obj = new TemplateRightsRoleList();
                    obj.SecurityRoleId = rolesplit[i];
                    obj.TemplateRightsId = returnValue;
                    _templateRightsRoleListRepository.Add(obj);
                    _uow.Save();
                }
            }

            if (_uow.Save() <= 0) throw new Exception("Creating Templalte right failed on save.");
            return Ok(templaterights.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] TemplateRightsDto templateRightsDto)
        {
            if (templateRightsDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var templateRights = _mapper.Map<TemplateRights>(templateRightsDto);
            var validate = _templateRightsRepository.Duplicate(templateRights);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _templateRightsRepository.Update(templateRights);
            if (_uow.Save() <= 0) throw new Exception("Updating Templalte right failed on save.");
            return Ok(templateRights.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _templateRightsRepository.Find(id);

            if (record == null)
                return NotFound();

            _templateRightsRepository.Delete(record);
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _templateRightsRepository.Find(id);

            if (record == null)
                return NotFound();
            _templateRightsRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GeTemplateRightsDropDown")]
        public IActionResult GetDropDown()
        {
            return Ok(_templateRightsRepository.GetDrugDropDown());
        }
    }
}