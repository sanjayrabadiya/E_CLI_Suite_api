using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Helper;
using GSC.Shared.DocumentService;
using GSC.Respository.Configuration;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;
using GSC.Shared.JWTAuth;
using GSC.Respository.EditCheckImpact;
using GSC.Domain.Context;
using GSC.Respository.Master;
using System.Linq;
using GSC.Respository.Project.Design;
using System.Collections.Generic;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateValueController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IUnitOfWork _uow;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IScreeningTemplateValueAuditRepository _screeningTemplateValueAuditRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IScreeningTemplateValueChildRepository _screeningTemplateValueChildRepository;
        private readonly IImpactService _impactService;
        private readonly IGSCContext _context;
        private readonly IProjectRepository _projectRepository;
        private readonly IScreeningEntryRepository _screeningEntrytRepository;
        public ScreeningTemplateValueController(IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IUploadSettingRepository uploadSettingRepository,
            IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueAuditRepository screeningTemplateValueAuditRepository,
            IScreeningTemplateValueChildRepository screeningTemplateValueChildRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IImpactService impactService,
            IGSCContext context,
            IProjectRepository projectRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IScreeningEntryRepository screeningEntrytRepository)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _uploadSettingRepository = uploadSettingRepository;
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningVisitRepository = screeningVisitRepository;
            _screeningTemplateValueAuditRepository = screeningTemplateValueAuditRepository;
            _screeningTemplateValueChildRepository = screeningTemplateValueChildRepository;
            _impactService = impactService;
            _context = context;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _projectRepository = projectRepository;
            _screeningEntrytRepository = screeningEntrytRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();

            var screeningTemplateValue = _screeningTemplateValueRepository.Find(id);

            var screeningTemplateValueDto = _mapper.Map<ScreeningTemplateValueDto>(screeningTemplateValue);
            return Ok(screeningTemplateValueDto);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var projectDesignTemplateId = _projectDesignVariableRepository.All.Where(x => x.Id == screeningTemplateValueDto.ProjectDesignVariableId).Select(t => t.ProjectDesignTemplateId).FirstOrDefault();

            if (projectDesignTemplateId != screeningTemplateValueDto.ProjectDesignTemplateId)
            {
                ModelState.AddModelError("Message", "Please select proper template!");
                return BadRequest(ModelState);
            }



            var value = _screeningTemplateValueRepository.GetValueForAudit(screeningTemplateValueDto);

            var screeningTemplateValue = _mapper.Map<ScreeningTemplateValue>(screeningTemplateValueDto);
            screeningTemplateValue.Id = 0;

            var randomization = _context.Randomization.Include(x => x.ScreeningEntry).FirstOrDefault(x => x.UserId == _jwtTokenAccesser.UserId);
            if (randomization != null)
            {
                var dbLock = _context.ScreeningTemplate.FirstOrDefault(x => x.ProjectDesignTemplateId == screeningTemplateValue.ScreeningTemplateId && x.ScreeningVisit.ScreeningEntry.RandomizationId == randomization.Id);
                if (dbLock != null && (dbLock.IsLocked || dbLock.IsHardLocked))
                {
                    ModelState.AddModelError("Message", "Template is locked");
                    return BadRequest(ModelState);
                }
            }

            _screeningTemplateValueRepository.Add(screeningTemplateValue);

            var aduit = new ScreeningTemplateValueAudit
            {
                ScreeningTemplateValue = screeningTemplateValue,
                Value = screeningTemplateValueDto.IsNa ? "N/A" : value,
                OldValue = screeningTemplateValueDto.OldValue,
                ProjectDesignVariableValueId = screeningTemplateValue.Children.Count() == 0 ? null : screeningTemplateValue.Children[0]?.ProjectDesignVariableValueId,
            };
            _screeningTemplateValueAuditRepository.Save(aduit);

            _screeningTemplateValueChildRepository.Save(screeningTemplateValue);

            ScreeningTemplateStatus(screeningTemplateValueDto, screeningTemplateValue.ScreeningTemplateId);

            _screeningEntrytRepository.SetFitnessValue(screeningTemplateValue);

            _uow.Save();

            var result = _screeningTemplateRepository.ValidateVariableValue(screeningTemplateValue, screeningTemplateValueDto.EditCheckIds, screeningTemplateValueDto.CollectionSource);
            //for variable email .prakash chauhan 14-05-2022
            if (screeningTemplateValueDto.CollectionSource == CollectionSources.RadioButton)
                _screeningTemplateRepository.SendEmailOnVaribleConfiguration(screeningTemplateValue.ScreeningTemplateId);


            return Ok(result);
        }

        private void ScreeningTemplateStatus(ScreeningTemplateValueDto screeningTemplateValueDto, int screeningTemplateId)
        {
            if (screeningTemplateValueDto.ScreeningStatus == Helper.ScreeningTemplateStatus.Pending)
            {
                var screeningTemplate = _screeningTemplateRepository.Find(screeningTemplateId);
                if (screeningTemplate.Status > Helper.ScreeningTemplateStatus.InProcess) return;
                screeningTemplate.Status = Helper.ScreeningTemplateStatus.InProcess;
                screeningTemplate.IsDisable = false;
                screeningTemplate.IsHide = false;
                screeningTemplate.IsNA = false;
                _screeningTemplateRepository.Update(screeningTemplate);

                var screeningVisit = _screeningVisitRepository.Find(screeningTemplate.ScreeningVisitId);
                if (screeningVisit != null)
                {
                    screeningVisit.Status = ScreeningVisitStatus.InProgress;
                    _screeningVisitRepository.Update(screeningVisit);
                }
            }


        }


        [HttpPut]
        [TransactionRequired]
        public IActionResult Put([FromBody] ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (screeningTemplateValueDto.Id <= 0) return BadRequest();


            var projectDesignTemplateId = _projectDesignVariableRepository.All.Where(x => x.Id == screeningTemplateValueDto.ProjectDesignVariableId).Select(t => t.ProjectDesignTemplateId).FirstOrDefault();

            if (projectDesignTemplateId != screeningTemplateValueDto.ProjectDesignTemplateId)
            {
                ModelState.AddModelError("Message", "Please select proper template!");
                return BadRequest(ModelState);
            }

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (screeningTemplateValueDto.IsDeleted && (
                screeningTemplateValueDto.CollectionSource == CollectionSources.Date ||
                screeningTemplateValueDto.CollectionSource == CollectionSources.DateTime ||
                screeningTemplateValueDto.CollectionSource == CollectionSources.Time) &&
                !_screeningTemplateRepository.IsRepated(screeningTemplateValueDto.ScreeningTemplateId) && _impactService.CheckReferenceVariable(screeningTemplateValueDto.ProjectDesignVariableId))
            {
                ModelState.AddModelError("Message", "Reference schedule date can't clear!");
                return BadRequest(ModelState);
            }

            var value = _screeningTemplateValueRepository.GetValueForAudit(screeningTemplateValueDto);

            var screeningTemplateValue = _mapper.Map<ScreeningTemplateValue>(screeningTemplateValueDto);

            var aduit = new ScreeningTemplateValueAudit
            {
                ScreeningTemplateValueId = screeningTemplateValue.Id,
                Value = value,
                Note = screeningTemplateValueDto.IsDeleted ? "Clear Data" : null,
                OldValue = screeningTemplateValueDto.OldValue,
                ReasonOth = _jwtTokenAccesser.RoleId == 2 ? null : _jwtTokenAccesser.GetHeader("audit-reason-oth"),
                ReasonId = _jwtTokenAccesser.RoleId == 2 ? null : int.Parse(_jwtTokenAccesser.GetHeader("audit-reason-id"))
            };
            _screeningTemplateValueAuditRepository.Save(aduit);

            if (screeningTemplateValueDto.IsDeleted)
                _screeningTemplateValueRepository.DeleteChild(screeningTemplateValue.Id);


            _screeningTemplateValueChildRepository.Save(screeningTemplateValue);

            _screeningTemplateValueRepository.Update(screeningTemplateValue);
            _context.Entry(screeningTemplateValue).Property("LabManagementUploadExcelDataId").IsModified = false;
            _context.Entry(screeningTemplateValue).Property("IsScheduleTerminate").IsModified = false;
            _context.Entry(screeningTemplateValue).Property("IsHide").IsModified = false;

            ScreeningTemplateStatus(screeningTemplateValueDto, screeningTemplateValue.ScreeningTemplateId);

            _screeningEntrytRepository.SetFitnessValue(screeningTemplateValue);

            _uow.Save();

            var result = _screeningTemplateRepository.ValidateVariableValue(screeningTemplateValue, screeningTemplateValueDto.EditCheckIds, screeningTemplateValueDto.CollectionSource);


            return Ok(result);
        }



        [HttpPut("UploadDocument")]
        public IActionResult UploadDocument([FromBody] ScreeningTemplateValueDto screeningTemplateValueDto)
        {
            if (screeningTemplateValueDto.Id <= 0) return BadRequest();

            var screeningTemplateValue = _screeningTemplateValueRepository.Find(screeningTemplateValueDto.Id);

            var documentPath = _uploadSettingRepository.GetDocumentPath();

            if (screeningTemplateValueDto.FileModel?.Base64?.Length > 0)
            {
                var screningDetails = _context.ScreeningTemplate.Where(x => x.Id == screeningTemplateValue.ScreeningTemplateId).Select(a => new { a.ScreeningVisit.ScreeningEntry.ProjectId, a.ScreeningVisit.ScreeningEntry.Randomization.Initial, a.ScreeningVisit.ScreeningEntry.Randomization.ScreeningNumber }).FirstOrDefault();
                if (screningDetails != null)
                {
                    var validateuploadlimit = _uploadSettingRepository.ValidateUploadlimit(screningDetails.ProjectId);
                    if (!string.IsNullOrEmpty(validateuploadlimit))
                    {
                        ModelState.AddModelError("Message", validateuploadlimit);
                        return BadRequest(ModelState);
                    }
                    DocumentService.RemoveFile(documentPath, screeningTemplateValue.DocPath);
                    string subject = screningDetails.ScreeningNumber + "-" + screningDetails.Initial;
                    screeningTemplateValue.DocPath = DocumentService.SaveUploadDocument(screeningTemplateValueDto.FileModel,
                          documentPath, _jwtTokenAccesser.CompanyId.ToString(), _projectRepository.GetStudyCode(screningDetails.ProjectId), FolderType.DataEntry, subject);
                }

                screeningTemplateValue.MimeType = screeningTemplateValueDto.FileModel.Extension;
            }

            var documentUrl = _uploadSettingRepository.GetWebDocumentUrl();
            screeningTemplateValueDto.DocPath = screeningTemplateValue.DocPath;
            screeningTemplateValueDto.DocFullPath = documentUrl + screeningTemplateValue.DocPath;

            _screeningTemplateValueRepository.Update(screeningTemplateValue);
            _uow.Save();

            return Ok(screeningTemplateValueDto);
        }

        [HttpGet("GetQueryStatusCount/{id}")]
        public IActionResult GetQueryStatusCount(int id)
        {
            return Ok(_screeningTemplateValueRepository.GetQueryStatusCount(id));
        }

        [HttpGet("GetQueryStatusBySubject/{id}")]
        public IActionResult GetQueryStatusBySubject(int id)
        {
            return Ok(_screeningTemplateValueRepository.GetQueryStatusBySubject(id));
        }

        [HttpGet("GetTemplateQueryList/{id}")]
        public IActionResult GetTemplateQueryList(int id)
        {
            return Ok(_screeningTemplateValueRepository.GetTemplateQueryList(id));
        }


        [HttpGet("GetQueryVariableDetail/{id}/{screeningEntryId}")]
        public IActionResult GetQueryVariableDetail(int id, int screeningEntryId)
        {
            return Ok(_screeningTemplateValueRepository.GetQueryVariableDetail(id, screeningEntryId));
        }

        [HttpPost]
        [Route("DeleteChild")]
        public IActionResult DeleteChild([FromBody] List<int> Ids)
        {
            _screeningTemplateValueChildRepository.DeleteChild(Ids);
            _uow.Save();
            return Ok();
        }
    }
}