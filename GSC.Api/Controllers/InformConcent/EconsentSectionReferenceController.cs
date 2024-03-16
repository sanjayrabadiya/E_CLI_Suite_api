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
    public class EconsentSectionReferenceController : BaseController
    {
        private readonly IEconsentSectionReferenceRepository _econsentSectionReferenceRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IProjectRepository _projectRepository;
        public EconsentSectionReferenceController(IEconsentSectionReferenceRepository econsentSectionReferenceRepository,
                                                IUnitOfWork uow,
                                                IMapper mapper,
                                                IUploadSettingRepository uploadSettingRepository, IJwtTokenAccesser jwtTokenAccesser, IGSCContext context, IProjectRepository projectRepository)
        {
            _econsentSectionReferenceRepository = econsentSectionReferenceRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _projectRepository = projectRepository;

        }

        //not use
        [HttpGet]
        [Route("GetSectionReference/{isDeleted}/{documentId}")]
        public IActionResult GetSectionReference(bool isDeleted, int documentId)
        {
            // display section reference data in grid          
            var sectionRefrencelist = _econsentSectionReferenceRepository.GetSectionReferenceList(isDeleted, documentId);
            return Ok(sectionRefrencelist);
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
            var sectionRef = _mapper.Map<EconsentSectionReference>(econsentSectionReferenceDto);
            int ProjectId = _context.EconsentSetup.Where(x => x.Id == econsentSectionReferenceDto.EconsentSetupId).Select(x => x.ProjectId).FirstOrDefault();
            foreach (var fileModeItem in econsentSectionReferenceDto.FileModel)
            {
                if (fileModeItem?.Base64?.Length > 0)
                {
                    var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit(ProjectId);
                    if (!string.IsNullOrEmpty(validateuploadlimit))
                    {
                        ModelState.AddModelError("Message", validateuploadlimit);
                        return BadRequest(ModelState);
                    }
                    sectionRef.FilePath = DocumentService.SaveUploadDocument(fileModeItem, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent, "EconsentSectionReference");
                }
                _econsentSectionReferenceRepository.Add(sectionRef);
                string root = Path.Combine(_uploadSettingRepository.GetDocumentPath(), FolderType.InformConcent.ToString(), "EconsentSectionReference");
                if (_uow.Save() <= 0)
                {
                    if (Directory.Exists(root))
                    {
                        Directory.Delete(root, true);
                    }

                    ModelState.AddModelError("Message", "Creating Section Refrence failed on save.");
                    return BadRequest(ModelState);
                }
            }
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
            var sectionRefrence = _mapper.Map<EconsentSectionReference>(econsentSectionReferenceDto);
            if (econsentSectionReferenceDto.FileModel.Count > 1)
            {
                ModelState.AddModelError("Message", "Please Update only single file");
                return BadRequest(ModelState);
            }
            int ProjectId = _context.EconsentSetup.Where(x => x.Id == econsentSectionReferenceDto.EconsentSetupId).Select(x => x.ProjectId).FirstOrDefault();
            if (econsentSectionReferenceDto.FileModel.Count > 0)
            {
                if (econsentSectionReferenceDto.FileModel[0]?.Base64?.Length > 0)
                {
                    var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit(ProjectId);
                    if (!string.IsNullOrEmpty(validateuploadlimit))
                    {
                        ModelState.AddModelError("Message", validateuploadlimit);
                        return BadRequest(ModelState);
                    }
                    DocumentService.RemoveFile(_uploadSettingRepository.GetDocumentPath(), document.FilePath);
                    sectionRefrence.FilePath = DocumentService.SaveUploadDocument(econsentSectionReferenceDto.FileModel[0], _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(ProjectId), FolderType.InformConcent, "EconsentSectionReference");
                }
            }
            else
            {
                sectionRefrence.FilePath = document.FilePath;
            }
            _econsentSectionReferenceRepository.Update(sectionRefrence);
            if (_uow.Save() <= 0)
            {
                ModelState.AddModelError("Message", "Updating Econsent file failed on save.");
                return BadRequest(ModelState);
            }
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
            var sectionrefrence = _econsentSectionReferenceRepository.GetSetionRefefrenceDetailList(documentId, sectionNo);
            return Ok(sectionrefrence);
        }

        [HttpPost]
        [Route("GetEconsentSectionReferenceDocument/{id}")]
        public IActionResult GetEconsentSectionReferenceDocument(int id)
        {
            // use for display the uploaded reference documents like (image,video,pdf,word)
            return Ok(_econsentSectionReferenceRepository.GetEconsentSectionReferenceDocument(id));
        }

        [HttpPost]
        [Route("GetEconsentSectionReferenceDocumentNew/{id}")]
        public IActionResult GetEconsentSectionReferenceDocumentNew(int id)
        {
            // use for display the uploaded reference documents like (image,video,pdf,word)
            return Ok(_econsentSectionReferenceRepository.GetEconsentSectionReferenceDocumentNew(id));
        }

        [HttpGet]
        [Route("GetEconsentSectionReferenceDocumentByUser")]
        public IActionResult GetEconsentSectionReferenceDocumentByUser()
        {
            // use for display the uploaded reference documents like (image,video,pdf,word)
            return Ok(_econsentSectionReferenceRepository.GetEconsentSectionReferenceDocumentByUser());
        }
    }
}
