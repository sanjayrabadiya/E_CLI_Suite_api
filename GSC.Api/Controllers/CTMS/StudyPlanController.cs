using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Api.Helpers;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyPlanController : BaseController
    {
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IStudyPlanRepository _studyPlanRepository;
        private readonly IGSCContext _context;

        public StudyPlanController(IUnitOfWork uow, IMapper mapper,
            IStudyPlanRepository studyPlanRepository, IGSCContext context)
        {
            _uow = uow;
            _mapper = mapper;
            _studyPlanRepository = studyPlanRepository;
            _context = context;
        }

        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var studyplan = _studyPlanRepository.GetStudyplanList(isDeleted);
            return Ok(studyplan);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var studyplan = _studyPlanRepository.Find(id);
            var studyplandetail = _mapper.Map<StudyPlanDto>(studyplan);
            if (studyplan != null)
            {
                var currencyRatedata = _context.CurrencyRate.Where(s => s.StudyPlanId == studyplan.Id && s.DeletedBy==null).ToList();
                List<CurrencyRateDTO> CurrencyRateList1 = new List<CurrencyRateDTO>();
                currencyRatedata.ForEach(s=>{
                    var CurrencyRateList = new CurrencyRateDTO()
                    {
                        localCurrencyId = s.CurrencyId,
                        localCurrencyRate = s.LocalCurrencyRate
                    };
                    CurrencyRateList1.Add(CurrencyRateList);
                });
                studyplandetail.CurrencyRateList = CurrencyRateList1;
            }
            return Ok(studyplandetail);
        }

        [HttpPost]
        [TransactionRequired]
        public IActionResult Post([FromBody] StudyPlanDto studyplanDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var lstStudyPlan = new List<StudyPlanDto>();
            lstStudyPlan.Add(studyplanDto);

            var TaskMaster = _context.RefrenceTypes.Include(d => d.TaskMaster).Where(x => x.TaskMaster.TaskTemplateId == studyplanDto.TaskTemplateId && x.DeletedDate==null).Any(x => x.RefrenceType == Helper.RefrenceType.Sites);
            if (TaskMaster)
            {
                var sites = _context.Project.Where(x => x.DeletedDate == null && x.ParentProjectId == studyplanDto.ProjectId).ToList();
                sites.ForEach(s =>
                {
                    var data = new StudyPlanDto();
                    data.StartDate = studyplanDto.StartDate;
                    data.EndDate = studyplanDto.EndDate;
                    data.ProjectId = s.Id;
                    data.TaskTemplateId = studyplanDto.TaskTemplateId;
                    lstStudyPlan.Add(data);
                });
            }

            foreach (var item in lstStudyPlan)
            {
                item.Id = 0;
                var studyplan = _mapper.Map<StudyPlan>(item);
                var validatecode = _studyPlanRepository.Duplicate(studyplan);
                if (!string.IsNullOrEmpty(validatecode))
                {
                    ModelState.AddModelError("Message", validatecode);
                    return BadRequest(ModelState);
                }
                _studyPlanRepository.Add(studyplan);
                if (_uow.Save() <= 0) throw new Exception("Study plan is failed on save.");
                studyplanDto.Id = studyplan.Id;


                var validate = _studyPlanRepository.ImportTaskMasterData(studyplan);
                if (!string.IsNullOrEmpty(validate))
                {
                    ModelState.AddModelError("Message", validate);
                    return BadRequest(ModelState);
                }
                
            }

            _studyPlanRepository.PlanUpdate(studyplanDto.ProjectId);
            _studyPlanRepository.CurrencyRateAdd(studyplanDto);
            return Ok();
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlanDto studyplanDto)
        {
            if (studyplanDto.Id <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var studyplan = _mapper.Map<StudyPlan>(studyplanDto);
            var validatecode = _studyPlanRepository.Duplicate(studyplan);
            if (!string.IsNullOrEmpty(validatecode))
            {
                ModelState.AddModelError("Message", validatecode);
                return BadRequest(ModelState);
            }

            _studyPlanRepository.Update(studyplan);
            if (_uow.Save() <= 0) throw new Exception("Study plan is failed on save.");
            _studyPlanRepository.CurrencyRateUpdate(studyplanDto);
            return Ok(studyplan.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyPlanRepository.Find(id);

            var AllProject = _context.Project.Where(x => x.DeletedDate == null && (x.ParentProjectId == record.ProjectId || x.Id == record.ProjectId)).ToList();
            foreach (var item in AllProject)
            {
                var data = _studyPlanRepository.FindByInclude(x => x.DeletedDate == null && x.ProjectId == item.Id).FirstOrDefault();
                if (data != null)
                    _studyPlanRepository.Delete(data.Id);
            }
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _studyPlanRepository.Find(id);

            var AllProject = _context.Project.Where(x => x.DeletedDate == null && (x.ParentProjectId == record.ProjectId || x.Id == record.ProjectId)).ToList();
            foreach (var item in AllProject)
            {
                var data = _studyPlanRepository.FindByInclude(x => x.DeletedDate != null && x.ProjectId == item.Id).FirstOrDefault();
                if (data != null)
                {
                    var validatecode = _studyPlanRepository.Duplicate(data);
                    if (string.IsNullOrEmpty(validatecode))
                    {
                        data.DeletedBy = null;
                        data.DeletedDate = null;
                        _studyPlanRepository.Active(data);
                    }
                }
            }
            _uow.Save();

            return Ok();
        }


    }
}
