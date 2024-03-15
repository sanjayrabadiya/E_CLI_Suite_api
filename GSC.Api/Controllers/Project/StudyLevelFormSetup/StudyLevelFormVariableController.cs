using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.GeneralConfig;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.Project.StudyLevelFormSetup;
using GSC.Helper;
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Respository.Project.GeneralConfig;
using GSC.Respository.Project.StudyLevelFormSetup;
using GSC.Shared.Extension;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Project.GeneralConfig
{
    [Route("api/[controller]")]
    public class StudyLevelFormVariableController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IStudyLevelFormRepository _studyLevelFormRepository;
        private readonly IStudyLevelFormVariableRepository _studyLevelFormVariableRepository;
        private readonly IVariableRepository _variableRepository;
        private readonly IStudyLevelFormVariableValueRepository _studyLevelFormVariableValueRepository;
        private readonly ICtmsMonitoringRepository _ctmsMonitoringRepository;
        public StudyLevelFormVariableController(
            IUnitOfWork uow, IMapper mapper, IStudyLevelFormRepository studyLevelFormRepository,
            IVariableTemplateRepository variableTemplateRepository,
            IStudyLevelFormVariableRepository studyLevelFormVariableRepository,
            IStudyLevelFormVariableValueRepository studyLevelFormVariableValueRepository,
            IStudyLevelFormVariableRemarksRepository studyLevelFormVariableRemarksRepository,
            IVariableRepository variableRepository,
            ICtmsMonitoringRepository ctmsMonitoringRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _studyLevelFormRepository = studyLevelFormRepository;
            _studyLevelFormVariableRepository = studyLevelFormVariableRepository;
            _studyLevelFormVariableValueRepository = studyLevelFormVariableValueRepository;
            _variableRepository = variableRepository;
            _ctmsMonitoringRepository = ctmsMonitoringRepository;
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var variable = _studyLevelFormVariableRepository.
                FindByInclude(t => t.Id == id, t => t.Values.Where(x => x.DeletedDate == null).OrderBy(x => x.SeqNo)).FirstOrDefault();
            var variableDto = _mapper.Map<StudyLevelFormVariableDto>(variable);
            if (variableDto != null && variableDto.Values != null)
            {
                variableDto.Values.ToList().ForEach(x =>
                {
                    if (variableDto.CollectionSource == CollectionSources.Table)
                        x.TableCollectionSourceName = x.TableCollectionSource.GetDescription();
                });
            }
            return Ok(variableDto);
        }

        [HttpGet]
        [Route("ChangeVariableDesignOrder/{id}/{index}")]
        public IActionResult ChangeVariableDesignOrder(int id, int index)
        {
            var variable = _studyLevelFormVariableRepository.Find(id);
            var templateId = variable.StudyLevelFormId;

            var orderedList = _studyLevelFormVariableRepository
                .FindBy(t => t.StudyLevelFormId == templateId && t.DeletedDate == null)
                .OrderBy(t => t.DesignOrder).ToList();
            orderedList.Remove(orderedList.First(t => t.Id == id));

            if (index != 0)
                index--;
            orderedList.Insert(index, variable);

            var i = 0;
            foreach (var item in orderedList)
            {
                if (item.InActiveVersion != null)
                    item.DesignOrder = i;
                else
                    item.DesignOrder = ++i;
                _studyLevelFormVariableRepository.Update(item);
            }

            _uow.Save();

            return Ok();
        }

        [HttpPost]
        public IActionResult Post([FromBody] StudyLevelFormVariableDto variableDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            variableDto.Id = 0;

            if (variableDto.Values != null)
                variableDto.Values = variableDto.Values.Where(x => !x.IsDeleted).ToList();

            var variable = _mapper.Map<StudyLevelFormVariable>(variableDto);
            variable.DesignOrder = 1;

            if (_studyLevelFormVariableRepository.All.Any(t => t.StudyLevelFormId == variable.StudyLevelFormId && t.DeletedDate == null))
                variable.DesignOrder = _studyLevelFormVariableRepository.FindBy(t => t.StudyLevelFormId == variable.StudyLevelFormId &&
                                           t.DeletedDate == null).Max(t => t.DesignOrder) + 1;

            var validate = _studyLevelFormVariableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var approval = _studyLevelFormRepository.CheckVerificationApproval(variableDto.StudyLevelFormId);
            if (!string.IsNullOrEmpty(approval))
            {
                ModelState.AddModelError("Message", approval);
                return BadRequest(ModelState);
            }

            _studyLevelFormVariableRepository.Add(variable);

            foreach (var item in variable.Values)
            {
                _studyLevelFormVariableValueRepository.Add(item);
            }

            _uow.Save();
            return Ok(variable.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyLevelFormVariableDto variableDto)
        {
            if (variableDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var validatemsg = _ctmsMonitoringRepository.StudyLevelFormAlreadyUse(variableDto.StudyLevelFormId);
            if (!string.IsNullOrEmpty(validatemsg))
            {
                ModelState.AddModelError("Message", validatemsg);
                return BadRequest(ModelState);
            }
            var approval = _studyLevelFormRepository.CheckVerificationApproval(variableDto.StudyLevelFormId);
            if (!string.IsNullOrEmpty(approval))
            {
                ModelState.AddModelError("Message", approval);
                return BadRequest(ModelState);
            }
            var variable = _mapper.Map<StudyLevelFormVariable>(variableDto);

            var validate = _studyLevelFormVariableRepository.Duplicate(variable);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _studyLevelFormVariableRepository.Update(variable);

            _studyLevelFormVariableValueRepository.UpdateVariableValues(variableDto, variableDto.CollectionValueDisable);

            _uow.Save();

            return Ok(variable.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyLevelFormVariableRepository.Find(id);

            if (record == null)
                return NotFound();

            var validatemsg = _ctmsMonitoringRepository.StudyLevelFormAlreadyUse(record.StudyLevelFormId);
            if (!string.IsNullOrEmpty(validatemsg))
            {
                ModelState.AddModelError("Message", validatemsg);
                return BadRequest(ModelState);
            }
            var approval = _studyLevelFormRepository.CheckVerificationApproval(record.StudyLevelFormId);
            if (!string.IsNullOrEmpty(approval))
            {
                ModelState.AddModelError("Message", approval);
                return BadRequest(ModelState);
            }

            if (record.VariableId != null)
            {
                var variable = _variableRepository.Find((int)record.VariableId);
                if (variable != null && variable.SystemType != null)
                {
                    ModelState.AddModelError("Message", "Can't delete record!");
                    return BadRequest(ModelState);
                }
            }
            _studyLevelFormVariableRepository.Delete(record);

            _uow.Save();

            if (_studyLevelFormVariableRepository.FindBy(t =>
                t.StudyLevelFormId == record.StudyLevelFormId && t.DeletedDate == null).Any())
            {
                var minOrder = _studyLevelFormVariableRepository.FindBy(t =>
                        t.StudyLevelFormId == record.StudyLevelFormId && t.DeletedDate == null)
                    .Min(t => t.DesignOrder);
                var firstId = _studyLevelFormVariableRepository.FindBy(t =>
                    t.StudyLevelFormId == record.StudyLevelFormId && t.DeletedDate == null &&
                    t.DesignOrder == minOrder).First().Id;
                ChangeVariableDesignOrder(firstId, 0);
            }

            return Ok();
        }

        [HttpGet]
        [Route("GetVariabeBasic/{studyLevelFormId}")]
        public IActionResult GetVariabeBasic(int studyLevelFormId)
        {
            return Ok(_studyLevelFormVariableRepository.GetVariabeBasic(studyLevelFormId));
        }

        [HttpGet]
        [Route("GetVariabeDropDown/{studyLevelFormId}")]
        public IActionResult GetVariabeDropDown(int studyLevelFormId)
        {
            return Ok(_studyLevelFormVariableRepository.GetVariableDropDown(studyLevelFormId));
        }

        [HttpGet]
        [Route("GetStudyLevelFormVariableRelation/{id}")]
        public IActionResult GetStudyLevelFormVariableRelation(int id)
        {
            var result = _studyLevelFormVariableRepository.GetStudyLevelFormVariableRelation(id);
            return Ok(result);
        }
    }
}
