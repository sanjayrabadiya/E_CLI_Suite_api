using System;
using System.IO;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Entities.InformConcent;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.InformConcent;
using GSC.Respository.Master;
using GSC.Shared.DocumentService;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;

namespace GSC.Api.Controllers.InformConcent
{
    [Route("api/[controller]")]
    [ApiController]
    public class EconsentsetupController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IEconsentSetupRepository _econsentSetupRepository;
        private readonly IUnitOfWork _uow;
        private readonly IUploadSettingRepository _uploadSettingRepository; 
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IProjectRepository _projectRepository;
        public EconsentsetupController(
            IEconsentSetupRepository econsentSetupRepository,
            IUnitOfWork uow,
            IMapper mapper, IUploadSettingRepository uploadSettingRepository,             
            IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IProjectRepository projectRepository)
        {
            _econsentSetupRepository = econsentSetupRepository;
            _uow = uow;
            _mapper = mapper;
            _uploadSettingRepository = uploadSettingRepository;            
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _projectRepository = projectRepository;
        }


        [HttpGet]
        [HttpGet("{projectid}/{isDeleted:bool?}")]
        public IActionResult Get(int projectid, bool isDeleted)
        {
            //Get Econsent document list for selected project
            var econsentSetups = _econsentSetupRepository.GetEconsentSetupList(projectid, isDeleted);
            return Ok(econsentSetups);
        }


        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            //calls when edit particular document         
            if (id <= 0) return BadRequest();
            //var ecincentSetup = _econsentSetupRepository.FindByInclude(x => x.Id == id, x => x.Roles, x => x.PatientStatus).FirstOrDefault();
            var ecincentSetup = _econsentSetupRepository.FindByInclude(x => x.Id == id).FirstOrDefault();
            var econcentsetupDto = _mapper.Map<EconsentSetupDto>(ecincentSetup);
            return Ok(econcentsetupDto);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            //calls when deactivate econsent document
            var record = _econsentSetupRepository.Find(id);
            if (record == null)
                return NotFound();
            _econsentSetupRepository.Delete(record);
            _uow.Save();
            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            //calls when activate econsent document
            var record = _econsentSetupRepository.Find(id);
            if (record == null)
                return NotFound();
            var validate = _econsentSetupRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            _econsentSetupRepository.Active(record);
            _uow.Save();
            return Ok();
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] EconsentSetupDto econsentSetupDto)
        {
            // add econsent document
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            
            var econsent = _mapper.Map<EconsentSetup>(econsentSetupDto);
            econsent.DocumentStatusId = DocumentStatus.Pending;
            var validate = _econsentSetupRepository.Duplicate(econsent);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);             
                return BadRequest(ModelState);
            }
            if (econsentSetupDto.FileModel?.Base64?.Length > 0)
            {
                var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit(econsentSetupDto.ProjectId);
                if (!string.IsNullOrEmpty(validateuploadlimit))
                {
                    ModelState.AddModelError("Message", validateuploadlimit);
                    return BadRequest(ModelState);
                }
                econsent.DocumentPath = DocumentService.SaveUploadDocument(econsentSetupDto.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(econsentSetupDto.ProjectId), FolderType.InformConcent, "EconsentSetup");               
            }
            string fullpath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), econsent.DocumentPath);
            var validatedocument = _econsentSetupRepository.validateDocument(fullpath);
            if (!string.IsNullOrEmpty(validatedocument))
            {
                ModelState.AddModelError("Message",validatedocument);
                if (Directory.Exists(fullpath))
                {
                    Directory.Delete(fullpath, true);
                }
                return BadRequest(ModelState);
            }
            _econsentSetupRepository.Add(econsent);      
            if (_uow.Save() <= 0) throw new Exception($"Creating Econsent File failed on save.");
            return Ok(econsent.Id);            
        }


        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] EconsentSetupDto econsentSetupDto)
        {
            //update econsent document
            if (econsentSetupDto.Id <= 0)
                return BadRequest();
            if (!ModelState.IsValid)
                return new UnprocessableEntityObjectResult(ModelState);
            var econsent = _mapper.Map<EconsentSetup>(econsentSetupDto);            
            var document = _econsentSetupRepository.Find(econsentSetupDto.Id);
            var validate = _econsentSetupRepository.Duplicate(econsent);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message",validate);               
                return BadRequest(ModelState);
            }
            if (econsentSetupDto.FileModel?.Base64?.Length > 0)
            {
                var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit(econsentSetupDto.ProjectId);
                if (!string.IsNullOrEmpty(validateuploadlimit))
                {
                    ModelState.AddModelError("Message", validateuploadlimit);
                    return BadRequest(ModelState);
                }
                DocumentService.RemoveFile(_uploadSettingRepository.GetDocumentPath(), document.DocumentPath);
                econsent.DocumentPath = DocumentService.SaveUploadDocument(econsentSetupDto.FileModel, _uploadSettingRepository.GetDocumentPath(),_jwtTokenAccesser.CompanyId.ToString(),_projectRepository.GetStudyCode(econsentSetupDto.ProjectId),FolderType.InformConcent, "EconsentSetup");
                string fullpath = Path.Combine(_uploadSettingRepository.GetDocumentPath(), econsent.DocumentPath);
                var validatedocument = _econsentSetupRepository.validateDocument(fullpath);
                if (!string.IsNullOrEmpty(validatedocument))
                {
                    ModelState.AddModelError("Message",validatedocument);
                    if (Directory.Exists(fullpath))
                    {
                        Directory.Delete(fullpath, true);
                    }
                    return BadRequest(ModelState);
                }
            }
            else
            {
                econsent.DocumentPath = document.DocumentPath;
            }            
            _econsentSetupRepository.Update(econsent);           
            if (_uow.Save() <= 0) throw new Exception($"Updating Econsent File failed on save.");            
            _uow.Save();
            return Ok(econsent.Id);            
        }

        //not use check
        [HttpGet]
        [Route("GetEconsentDocumentDropDown/{projectId}")]
        public IActionResult GetEconsentDocumentDropDown(int projectId)
        {
            // use in econsent section reference page for document drop down data
            return Ok(_econsentSetupRepository.GetEconsentDocumentDropDown(projectId));
        }

        [HttpGet]
        [Route("GetPatientStatusDropDown")]
        public IActionResult GetPatientStatusDropDown()
        {
            // patient status dropdown use in econsent setup add/edit popup
            return Ok(_econsentSetupRepository.GetPatientStatusDropDown());
        }

        [HttpGet]
        [Route("UpdateDocumentStatus/{econcentSetupId}")]
        public IActionResult UpdateDocumentStatus(int econcentSetupId)
        {
            var econsent = _econsentSetupRepository.Find(econcentSetupId);
            econsent.DocumentStatusId = DocumentStatus.Final;
            _econsentSetupRepository.SendDocumentEmailPatient(econsent);
            _econsentSetupRepository.Update(econsent);
            if (_uow.Save() <= 0) throw new Exception($"Updating Econsent File failed on save.");
            return Ok(econsent.Id);
        }
    }
}
