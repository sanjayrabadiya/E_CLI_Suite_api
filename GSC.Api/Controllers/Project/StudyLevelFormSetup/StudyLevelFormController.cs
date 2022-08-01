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
using GSC.Respository.CTMS;
using GSC.Respository.Master;
using GSC.Respository.Project.GeneralConfig;
using GSC.Respository.Project.StudyLevelFormSetup;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace GSC.Api.Controllers.Project.GeneralConfig
{
    [Route("api/[controller]")]
    public class StudyLevelFormController : BaseController
    {

        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IStudyLevelFormRepository _studyLevelFormRepository;
        private readonly IStudyLevelFormVariableRepository _studyLevelFormVariableRepository;
        private readonly IVariableTemplateRepository _variableTemplateRepository;
        private readonly IStudyLevelFormVariableValueRepository _studyLevelFormVariableValueRepository;
        private readonly IStudyLevelFormVariableRemarksRepository _studyLevelFormVariableRemarksRepository;
        private readonly ICtmsMonitoringRepository _ctmsMonitoringRepository;
        public StudyLevelFormController(
            IUnitOfWork uow, IMapper mapper, IStudyLevelFormRepository studyLevelFormRepository,
            IVariableTemplateRepository variableTemplateRepository,
            IStudyLevelFormVariableRepository studyLevelFormVariableRepository,
            IStudyLevelFormVariableValueRepository studyLevelFormVariableValueRepository,
            IStudyLevelFormVariableRemarksRepository studyLevelFormVariableRemarksRepository,
            ICtmsMonitoringRepository ctmsMonitoringRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _studyLevelFormRepository = studyLevelFormRepository;
            _variableTemplateRepository = variableTemplateRepository;
            _studyLevelFormVariableRepository = studyLevelFormVariableRepository;
            _studyLevelFormVariableValueRepository = studyLevelFormVariableValueRepository;
            _studyLevelFormVariableRemarksRepository = studyLevelFormVariableRemarksRepository;
            _ctmsMonitoringRepository = ctmsMonitoringRepository;
        }

        // GET: api/<controller>
        [HttpGet("{isDeleted:bool?}")]
        public IActionResult Get(bool isDeleted)
        {
            var form = _studyLevelFormRepository.GetStudyLevelFormList(isDeleted);
            return Ok(form);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            var studyLevelForm = _studyLevelFormRepository.FindBy(x => x.Id == id).FirstOrDefault();
            var studyLevelFormDto = _mapper.Map<StudyLevelFormDto>(studyLevelForm);
            return Ok(studyLevelFormDto);
        }


        [HttpPost]
        public IActionResult Post([FromBody] StudyLevelFormDto studyLevelFormDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            studyLevelFormDto.Id = 0;
            var studyLevelForm = _mapper.Map<StudyLevelForm>(studyLevelFormDto);

            var validate = _studyLevelFormRepository.Duplicate(studyLevelForm);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            studyLevelForm.Variables = new List<StudyLevelFormVariable>();

            var variableTemplate = _variableTemplateRepository.GetTemplate(studyLevelFormDto.VariableTemplateId);

            if (variableTemplate == null) return NotFound();

            var variableOrder = 0;
            foreach (var variableDetail in variableTemplate.VariableTemplateDetails)
            {
                var studyLevelFormVariable = _mapper.Map<StudyLevelFormVariable>(variableDetail.Variable);
                studyLevelFormVariable.Id = 0;
                studyLevelFormVariable.StudyVersion = null;
                studyLevelFormVariable.VariableId = variableDetail.VariableId;
                if (studyLevelFormVariable.InActiveVersion != null)
                    studyLevelFormVariable.DesignOrder = variableOrder;
                else
                    studyLevelFormVariable.DesignOrder = ++variableOrder;

                studyLevelFormVariable.Note = variableDetail.Note;
                _studyLevelFormVariableRepository.Add(studyLevelFormVariable);
                studyLevelForm.Variables.Add(studyLevelFormVariable);

                studyLevelFormVariable.Values = new List<StudyLevelFormVariableValue>();

                var valueOrder = 0;
                foreach (var variableValue in variableDetail.Variable.Values)
                {
                    var studyLevelFormVariableValue = _mapper.Map<StudyLevelFormVariableValue>(variableValue);
                    studyLevelFormVariableValue.Id = 0;
                    studyLevelFormVariableValue.SeqNo = ++valueOrder;
                    _studyLevelFormVariableValueRepository.Add(studyLevelFormVariableValue);
                    studyLevelFormVariable.Values.Add(studyLevelFormVariableValue);
                }
            }
            _studyLevelFormRepository.Add(studyLevelForm);

            if (_uow.Save() <= 0) throw new Exception("Creating setup form failed on save.");
            return Ok(studyLevelForm.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyLevelFormDto studyLevelFormDto)
        {
            if (studyLevelFormDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var validatemsg = _ctmsMonitoringRepository.StudyLevelFormAlreadyUse(studyLevelFormDto.Id);
            if (!string.IsNullOrEmpty(validatemsg))
            {
                ModelState.AddModelError("Message", validatemsg);
                return BadRequest(ModelState);
            }

            var studyLevelForm = _mapper.Map<StudyLevelForm>(studyLevelFormDto);

            var validate = _studyLevelFormRepository.Duplicate(studyLevelForm);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            var approval = _studyLevelFormRepository.CheckVerificationApproval(studyLevelForm);
            if (!string.IsNullOrEmpty(approval))
            {
                ModelState.AddModelError("Message", approval);
                return BadRequest(ModelState);
            }
            _studyLevelFormRepository.Update(studyLevelForm);

            if (_uow.Save() <= 0) throw new Exception("Update setup form failed on save.");
            return Ok(studyLevelForm.Id);
        }

        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyLevelFormRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _ctmsMonitoringRepository.StudyLevelFormAlreadyUse(id);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var approval = _studyLevelFormRepository.CheckVerificationApproval(record);
            if (!string.IsNullOrEmpty(approval))
            {
                ModelState.AddModelError("Message", approval);
                return BadRequest(ModelState);
            }
            _studyLevelFormRepository.Delete(record);

            var studyLevelFormVariable = _studyLevelFormVariableRepository.FindByInclude(x => x.StudyLevelFormId == record.Id).ToList();

            studyLevelFormVariable.ForEach(temp =>
            {
                _studyLevelFormVariableRepository.Delete(temp);

                var studyLevelFormVariableValue = _studyLevelFormVariableValueRepository.FindByInclude(x => x.StudyLevelFormVariableId == temp.Id).ToList();
                studyLevelFormVariableValue.ForEach(variableValue =>
                {
                    _studyLevelFormVariableValueRepository.Delete(variableValue);
                });

                var studyLevelFormVariableRemarks = _studyLevelFormVariableRemarksRepository.FindByInclude(x => x.StudyLevelFormVariableId == temp.Id).ToList();
                studyLevelFormVariableRemarks.ForEach(remarks =>
                {
                    _studyLevelFormVariableRemarksRepository.Delete(remarks);
                });
            });
            _uow.Save();

            return Ok();
        }

        [HttpPatch("{id}")]
        public ActionResult Active(int id)
        {
            var record = _studyLevelFormRepository.Find(id);

            if (record == null)
                return NotFound();

            var validate = _studyLevelFormRepository.Duplicate(record);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }

            _studyLevelFormRepository.Active(record);

            var studyLevelFormVariable = _studyLevelFormVariableRepository.FindByInclude(x => x.StudyLevelFormId == record.Id).ToList();

            studyLevelFormVariable.ForEach(temp =>
            {
                _studyLevelFormVariableRepository.Active(temp);

                var studyLevelFormVariableValue = _studyLevelFormVariableValueRepository.FindByInclude(x => x.StudyLevelFormVariableId == temp.Id).ToList();
                studyLevelFormVariableValue.ForEach(variableValue =>
                {
                    _studyLevelFormVariableValueRepository.Active(variableValue);
                });

                var studyLevelFormVariableRemarks = _studyLevelFormVariableRemarksRepository.FindByInclude(x => x.StudyLevelFormVariableId == temp.Id).ToList();
                studyLevelFormVariableRemarks.ForEach(remarks =>
                {
                    _studyLevelFormVariableRemarksRepository.Active(remarks);
                });
            });
            _uow.Save();

            return Ok();
        }

        [HttpGet]
        [Route("GetTemplateDropDown/{projectId}")]
        public IActionResult GetTemplateDropDown(int projectId)
        {
            return Ok(_studyLevelFormRepository.GetTemplateDropDown(projectId));
        }
    }
}
