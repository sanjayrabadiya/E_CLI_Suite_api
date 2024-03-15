using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Respository.Screening;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Api.Controllers.Screening
{
    [Route("api/[controller]")]
    public class ScreeningTemplateLockUnlockController : BaseController
    {
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IScreeningTemplateLockUnlockRepository _screeningTemplateLockUnlockRepository;
        private readonly IUnitOfWork _uow;
        private readonly IMapper _mapper;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        public ScreeningTemplateLockUnlockController(IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningTemplateLockUnlockRepository screeningTemplateLockUnlockRepository,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IUnitOfWork uow, IMapper mapper)
        {
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningTemplateLockUnlockRepository = screeningTemplateLockUnlockRepository;
            _uow = uow;
            _mapper = mapper;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
        }

        [HttpGet]
        [Route("GetLockUnlockList")]
        public IActionResult GetLockUnlockList([FromQuery] LockUnlockSearchDto lockUnlockParams)
        {
            if (lockUnlockParams.ProjectId <= 0)
            {
                return BadRequest();
            }

            var lockUnlockTemplates = _screeningTemplateRepository.GetLockUnlockList(lockUnlockParams);

            return Ok(lockUnlockTemplates);
        }

        [HttpPut]
        [Route("LockUnlockTemplateList")]
        [TransactionRequired]
        public IActionResult LockUnlockTemplateList([FromBody] List<ScreeningTemplateLockUnlockAuditDto> AuditList)
        {
            var screeningTemplateIds = AuditList.Select(t => t.ScreeningTemplateId).Distinct().ToList();

            foreach (var x in screeningTemplateIds)
            {
                var item = AuditList.Find(t => t.ScreeningTemplateId == x);

                if (item.IsLocked)
                {
                    CheckEditCheck(x);

                    string validateMsg = _screeningTemplateValueRepository.CheckCloseQueries(x);

                    if (!string.IsNullOrEmpty(validateMsg))
                    {
                        _uow.Commit();
                        _uow.Begin();
                        ModelState.AddModelError("Message", validateMsg);
                        return BadRequest(ModelState);
                    }
                }
                var screeningTemplate = _screeningTemplateRepository.Find(x);
                var screeningTemplateLockUnlock = _mapper.Map<ScreeningTemplateLockUnlockAudit>(item);
                _screeningTemplateLockUnlockRepository.Insert(screeningTemplateLockUnlock);
                screeningTemplate.IsLocked = item.IsLocked;
                screeningTemplate.IsHardLocked = item.IsHardLocked;
                _screeningTemplateRepository.Update(screeningTemplate);

                _uow.Save();

                if (!item.IsLocked)
                    CheckEditCheck(screeningTemplate.Id);
            }

            return Ok();
        }

        void CheckEditCheck(int id)
        {
            _screeningTemplateRepository.SubmitReviewTemplate(id, true);
            _uow.Save();
        }

        [HttpGet]
        [Route("GetLockUnlockHistoryDetails/{projectId}/{parentProjectId}")]
        public IActionResult GetLockUnlockHistoryDetails(int projectId, int parentProjectId)
        {
            if (projectId <= 0) return BadRequest();
            return Ok(_screeningTemplateLockUnlockRepository.ProjectLockUnLockHistory(projectId, parentProjectId));
        }
    }
}
