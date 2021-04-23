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

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentSectionReferenceController : BaseController
    {
        private readonly IEconsentSectionReferenceRepository _econsentSectionReferenceRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        public EconsentSectionReferenceController(IEconsentSectionReferenceRepository econsentSectionReferenceRepository,
                                                IUnitOfWork uow,
                                                IMapper mapper,
                                                IUploadSettingRepository uploadSettingRepository)
        {
            _econsentSectionReferenceRepository = econsentSectionReferenceRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
        }


        [HttpGet]
        [Route("GetSectionReference/{isDeleted}/{documentId}")]
        public IActionResult GetSectionReference(bool isDeleted, int documentId)
        {
            // display section reference data in grid
            var econsentSectionReferences = _econsentSectionReferenceRepository.FindByInclude(x => x.EconsentSetupId == documentId && (isDeleted ? x.DeletedDate != null : x.DeletedDate == null)).OrderByDescending(x => x.Id).ToList();
            var econsentSectionReferenceDto = _mapper.Map<IEnumerable<EconsentSectionReferenceDto>>(econsentSectionReferences).ToList();
            foreach (var item in econsentSectionReferenceDto)
            {
                item.IsDeleted = isDeleted;
            }
            return Ok(econsentSectionReferenceDto);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            // calls when edit particular entry
            if (id <= 0) return BadRequest();
            var econsentSectionReference = _econsentSectionReferenceRepository.Find(id);
            var econsentSectionReferenceDto = _mapper.Map<EconsentSectionReferenceDto>(econsentSectionReference);
            return Ok(econsentSectionReferenceDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] EconsentSectionReferenceDto econsentSectionReferenceDto)
        {
            // add data for section reference
            if (!ModelState.IsValid)
               return new UnprocessableEntityObjectResult(ModelState);
            _econsentSectionReferenceRepository.AddEconsentSectionReference(econsentSectionReferenceDto);
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] EconsentSectionReferenceDto econsentSectionReferenceDto)
        {
            //update section reference
            if (econsentSectionReferenceDto.Id <= 0)
                return BadRequest();
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            var document = _econsentSectionReferenceRepository.Find(econsentSectionReferenceDto.Id);
            if (econsentSectionReferenceDto.FileModel != null && econsentSectionReferenceDto.FileModel[0]?.Base64?.Length > 0)
                document.FilePath = DocumentService.SaveEconsentSectionReferenceFile(econsentSectionReferenceDto.FileModel[0], _uploadSettingRepository.GetDocumentPath(), FolderType.InformConcent, "EconsentSectionReference");
            document.SectionNo = econsentSectionReferenceDto.SectionNo;
            document.ReferenceTitle = econsentSectionReferenceDto.ReferenceTitle;
            document.EconsentSetupId = econsentSectionReferenceDto.EconsentSetupId;
            _econsentSectionReferenceRepository.Update(document);
            if (_uow.Save() <= 0)
               throw new Exception($"Updating Econsent file failed on save.");
            return Ok(document.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            //use for deactivate record
            var record = _econsentSectionReferenceRepository.Find(id);
            if (record == null)
                return NotFound();
            _econsentSectionReferenceRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            //use for activate record
            var record = _econsentSectionReferenceRepository.Find(id);
            if (record == null)
                return NotFound();
            _econsentSectionReferenceRepository.Active(record);
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetEconsentDocumentSectionDropDown/{documentId}")]
        public IActionResult GetEconsentDocumentSectionDropDown(int documentId)
        {
            //fetch sections from the document that we have uploaded in econsent setup
            return Ok(_econsentSectionReferenceRepository.GetEconsentDocumentSectionDropDown(documentId));
        }

        [HttpGet]
        [Route("GetEconsentDocumentSectionReference/{documentId}/{sectionNo}")]
        public IActionResult GetEconsentDocumentSectionReference(int documentId, int sectionNo)
        {
            // in patient portal document review page right side section reference data comes from this api
            var references = _econsentSectionReferenceRepository.FindBy(x => x.EconsentSetupId == documentId && x.SectionNo == sectionNo).ToList();
            return Ok(references);
        }

        [HttpPost]
        [Route("GetEconsentSectionReferenceDocument/{id}")]
        public IActionResult GetEconsentSectionReferenceDocument(int id)
        {
            // use for display the uploaded reference documents like (image,video,pdf,word)
            return Ok(_econsentSectionReferenceRepository.GetEconsentSectionReferenceDocument(id));
        }
    }
}
