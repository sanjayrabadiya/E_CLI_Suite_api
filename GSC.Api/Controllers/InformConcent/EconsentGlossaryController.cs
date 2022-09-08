using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.InformConcent;
using Microsoft.AspNetCore.Mvc;
using GSC.Data.Entities.InformConcent;
using System.IO;
using GSC.Shared.JWTAuth;
using GSC.Domain.Context;
using GSC.Respository.Master;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentGlossaryController : BaseController
    {
        private readonly IEconsentSectionReferenceRepository _econsentSectionReferenceRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IProjectRepository _projectRepository;
        private readonly IEconsentGlossaryRepository _econsentGlossaryRepository;
        public EconsentGlossaryController(IEconsentSectionReferenceRepository econsentSectionReferenceRepository,
                                                IUnitOfWork uow,
                                                IMapper mapper,
                                                IUploadSettingRepository uploadSettingRepository, IJwtTokenAccesser jwtTokenAccesser, IGSCContext context, IProjectRepository projectRepository,
                                                IEconsentGlossaryRepository econsentGlossaryRepository)
        {
            _econsentSectionReferenceRepository = econsentSectionReferenceRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _projectRepository = projectRepository;
            _econsentGlossaryRepository = econsentGlossaryRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // calls when edit particular entry
            if (id <= 0) return BadRequest();
            var econsentGlossary = _econsentGlossaryRepository.Find(id);
            var econsentGlossaryDto = _mapper.Map<EconsentGlossaryDto>(econsentGlossary);
            return Ok(econsentGlossaryDto);
        }

        [HttpGet]
        [Route("GetGlossaryList/{isDeleted}/{EconsentSetupId}")]
        public IActionResult GetGlossaryList(bool isDeleted, int EconsentSetupId)
        {
            // display glossaryList in grid          
            var GlossaryList = _econsentGlossaryRepository.GetGlossaryList(isDeleted, EconsentSetupId);
            return Ok(GlossaryList);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EconsentGlossaryDto econsentGlossaryDto)
        {
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            var econsentGlossary = _mapper.Map<EconsentGlossary>(econsentGlossaryDto);
            
            _econsentGlossaryRepository.Add(econsentGlossary);
            if (_uow.Save() <= 0) throw new Exception($"Updating Econsent glossary failed on save.");
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] EconsentGlossaryDto econsentGlossaryDto)
        {
            if (econsentGlossaryDto.Id <= 0)
                return BadRequest();
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
          
            var Glossary = _econsentGlossaryRepository.Find(econsentGlossaryDto.Id);
            var econsentGlossary = _mapper.Map<EconsentGlossary>(econsentGlossaryDto);

            _econsentGlossaryRepository.Update(econsentGlossary);
            if (_uow.Save() <= 0) throw new Exception($"Updating Econsent glossary failed on save.");
            return Ok();
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            //use for deactivate record
            var record = _econsentGlossaryRepository.Find(id);
            if (record == null)
                return NotFound();
            _econsentGlossaryRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            //use for activate record
            var record = _econsentGlossaryRepository.Find(id);
            if (record == null)
                return NotFound();
            _econsentGlossaryRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetEconsentDocumentWordDropDown/{documentId}")]
        public IActionResult GetEconsentDocumentWordDropDown(int documentId)
        {
            //fetch word from the document that we have uploaded in econsent setup
            return Ok(_econsentGlossaryRepository.GetEconsentDocumentWordDropDown(documentId));
        }

    }
}
