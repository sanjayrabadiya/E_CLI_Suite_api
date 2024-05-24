using System;
using System.IO;
using System.Linq;
using System.Numerics;
using AutoMapper;
using GSC.Api.Controllers.Common;
using GSC.Common;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Location;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.CTMS;
using GSC.Shared.DocumentService;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;


namespace GSC.Api.Controllers.CTMS
{
    [Route("api/[controller]")]
    [ApiController]
    public class StudyPlanTaskController : BaseController
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        private readonly IGSCContext _context;
        private readonly IStudyPlanTaskRepository _studyPlanTaskRepository;
        private readonly IStudyPlanRepository _studyPlanRepository;
        private readonly IUploadSettingRepository _uploadSettingRepository;

        public StudyPlanTaskController(IUnitOfWork uow, IMapper mapper,
            IJwtTokenAccesser jwtTokenAccesser, IStudyPlanRepository studyPlanRepository, IGSCContext context, IStudyPlanTaskRepository studyPlanTaskRepository,
            IUploadSettingRepository uploadSettingRepository)
        {
            _uow = uow;
            _mapper = mapper;
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _studyPlanTaskRepository = studyPlanTaskRepository;
            _studyPlanRepository = studyPlanRepository;
            _uploadSettingRepository = uploadSettingRepository;
        }

        [HttpGet("{isDeleted:bool?}/{StudyPlanId:int}/{ProjectId:int}/{filterId:int}")]
        public IActionResult Get(bool isDeleted, int StudyPlanId, int ProjectId, CtmsStudyTaskFilter filterId)
        {
            var studyplan = _studyPlanTaskRepository.GetStudyPlanTaskList(isDeleted, StudyPlanId, ProjectId, filterId);
            return Ok(studyplan);
        }

        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            if (id <= 0) return BadRequest();
            var task = _studyPlanTaskRepository.FindByInclude(x => x.Id == id).FirstOrDefault();
            var taskDto = _mapper.Map<StudyPlanTaskDto>(task);

            var taskDate = _studyPlanTaskRepository.GetChildStartEndDate(id);
            if (taskDate != null)
            {
                taskDto.ActualStartDate = taskDate.StartDate;
                taskDto.ActualEndDate = taskDate.EndDate;
            }
            return Ok(taskDto);
        }

        [HttpPost]
        public IActionResult Post([FromBody] StudyPlantaskParameterDto taskmasterDto)
        {
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            if (taskmasterDto.RefrenceType == RefrenceType.Sites)
            {
                var result = _studyPlanTaskRepository.AddSiteTask(taskmasterDto);
                if (string.IsNullOrEmpty(result))
                    return Ok(1);
                else
                {
                    ModelState.AddModelError("Message", result);
                    return BadRequest(ModelState);
                }
            }

            if (taskmasterDto.RefrenceType == RefrenceType.Country)
            {
                var result = _studyPlanTaskRepository.AddCountryTask(taskmasterDto);
                if (string.IsNullOrEmpty(result))
                    return Ok(1);
                else
                {
                    ModelState.AddModelError("Message", result);
                    return BadRequest(ModelState);
                }
            }

            taskmasterDto.Id = 0;
            var tastMaster = _mapper.Map<StudyPlanTask>(taskmasterDto);
            tastMaster.TaskOrder = _studyPlanTaskRepository.UpdateTaskOrder(taskmasterDto);
            var data = _studyPlanTaskRepository.UpdateDependentTaskDate(tastMaster);
            tastMaster.IsCountry = taskmasterDto.RefrenceType == RefrenceType.Country;
            var project = _context.StudyPlan.Where(x => x.Id == taskmasterDto.StudyPlanId).First();
            tastMaster.ProjectId = project.Id;
            if (data != null)
            {
                tastMaster.StartDate = data.StartDate;
                tastMaster.EndDate = data.EndDate;
                tastMaster.Percentage = data.Percentage;
            }
            if (taskmasterDto.TaskDocumentFileModel?.Base64?.Length > 0 && taskmasterDto.TaskDocumentFileModel?.Base64 != null)
            {
                tastMaster.TaskDocumentFilePath = DocumentService.SaveUploadDocument(taskmasterDto.TaskDocumentFileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "StudyPlanTask");
            }
            _studyPlanTaskRepository.Add(tastMaster);
            _uow.Save();

            _studyPlanTaskRepository.UpdateTaskOrderSequence(taskmasterDto.Id);
            if (project != null)
            {
                var ParentProject = _context.Project.Where(x => x.Id == project.ProjectId).FirstOrDefault();
                if (ParentProject != null)
                    _studyPlanRepository.PlanUpdate((int)(ParentProject.ParentProjectId != null ? ParentProject.ParentProjectId : project.ProjectId));
            }
            return Ok(tastMaster.Id);
        }

        [HttpPut]
        public IActionResult Put([FromBody] StudyPlantaskParameterDto taskmasterDto)
        {
            if (taskmasterDto.Id <= 0) return BadRequest();

            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);

            var tastMaster = _mapper.Map<StudyPlanTask>(taskmasterDto);          
            var data = _studyPlanTaskRepository.UpdateDependentTaskDate(tastMaster);
            if (data != null)
            {
                tastMaster.StartDate = data.StartDate;
                tastMaster.EndDate = data.EndDate;
            }
            var revertdata = _studyPlanTaskRepository.Find(taskmasterDto.Id);
            if (taskmasterDto.TaskDocumentFileModel?.Base64?.Length > 0 && taskmasterDto.TaskDocumentFileModel?.Base64 != null)
            {
                DocumentService.RemoveFile(_uploadSettingRepository.GetDocumentPath(), revertdata.TaskDocumentFilePath);
                tastMaster.TaskDocumentFilePath = DocumentService.SaveUploadDocument(taskmasterDto.TaskDocumentFileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "StudyPlanTask");
            }
            else
            {
                tastMaster.TaskDocumentFilePath = revertdata.TaskDocumentFilePath;
            }
            tastMaster.StudyPlanId = revertdata.StudyPlanId;
            tastMaster.ProjectId = revertdata.ProjectId;
            _studyPlanTaskRepository.Update(tastMaster);

            if (_uow.Save() <= 0) return Ok(new Exception("Updating Task failed on save."));
            string mvalidate = _studyPlanTaskRepository.UpdateDependentTask(taskmasterDto.Id);
            if (!string.IsNullOrEmpty(mvalidate))
            {
                ModelState.AddModelError("Message", mvalidate);
                _studyPlanTaskRepository.Update(revertdata);
                _uow.Save();
                return BadRequest(ModelState);
            }

            var project = _context.StudyPlan.Where(x => x.Id == taskmasterDto.StudyPlanId).FirstOrDefault();
            if (project != null)
            {
                var ParentProject = _context.Project.Where(x => x.Id == project.ProjectId).FirstOrDefault();
                if (ParentProject != null)
                    _studyPlanRepository.PlanUpdate((int)(ParentProject.ParentProjectId != null ? ParentProject.ParentProjectId : project.ProjectId));
            }
            return Ok(tastMaster.Id);
        }


        [HttpDelete("{id}")]
        public ActionResult Delete(int id)
        {
            var record = _studyPlanTaskRepository.Find(id);

            var parenttask = _studyPlanTaskRepository.FindBy(x => x.ParentId == id);
            foreach (var task in parenttask.Select(s => s.Id).ToList())
            {
                if (record == null)
                    return NotFound();
                var subtask = _studyPlanTaskRepository.FindBy(x => x.ParentId == task).ToList();
                foreach (var sub in subtask)
                    _studyPlanTaskRepository.Delete(sub.Id);
                _studyPlanTaskRepository.Delete(task);
            }
            _studyPlanTaskRepository.Delete(record.Id);


            var childTaskList = _studyPlanTaskRepository.All.Where(x => x.DeletedDate == null && x.DependentTaskId == id);
            foreach (var childTask in childTaskList)
            {
                childTask.DependentTaskId = null;
                _studyPlanTaskRepository.Update(childTask);
            }

            _uow.Save();
            _studyPlanTaskRepository.UpdateTaskOrderSequence(record.StudyPlanId);
            return Ok();
        }

        [HttpGet("ConverttoMileStone/{id:int}/{isPaymentMileStone:bool?}")]
        public IActionResult ConverttoMileStone(int id, bool isPaymentMileStone)
        {
            if (id <= 0) return BadRequest();
            var milestonetask = _studyPlanTaskRepository.Find(id);
            milestonetask.isMileStone = true;
            milestonetask.IsPaymentMileStone = isPaymentMileStone;
            milestonetask.EndDate = milestonetask.StartDate;
            milestonetask.Duration = 0;
            _studyPlanTaskRepository.Update(milestonetask);
            _uow.Save();
            _studyPlanTaskRepository.UpdateDependentTask(id);
            return Ok(id);
        }

        [HttpGet("ConverttoTask/{id}")]
        public IActionResult ConverttoTask(int id)
        {
            if (id <= 0) return BadRequest();
            var milestonetask = _studyPlanTaskRepository.Find(id);
            milestonetask.isMileStone = false;
            milestonetask.EndDate = WorkingDayHelper.GetNextWorkingDay(milestonetask.StartDate);
            milestonetask.Duration = 1;
            _studyPlanTaskRepository.Update(milestonetask);
            _uow.Save();
            _studyPlanTaskRepository.UpdateDependentTask(id);
            return Ok(id);
        }


        [HttpPost("GetNetxtworkingDate")]
        public IActionResult GetNetxtworkingDate([FromBody] NextWorkingDateParameterDto parameterDto)
        {
            if (parameterDto.StudyPlanId <= 0) return BadRequest();
            if (!ModelState.IsValid) return new UnprocessableEntityObjectResult(ModelState);
            var validate = _studyPlanTaskRepository.ValidateweekEnd(parameterDto);
            if (!string.IsNullOrEmpty(validate))
            {
                ModelState.AddModelError("Message", validate);
                return BadRequest(ModelState);
            }
            var nextworkingdate = _studyPlanTaskRepository.GetNextWorkingDate(parameterDto);
            return Ok(nextworkingdate);
        }


        [HttpGet("GetStudyPlanTaskHistory/{id}")]
        public IActionResult GetStudyPlanTaskHistory(int id)
        {
            if (id <= 0) return BadRequest();

            var result = _studyPlanTaskRepository.GetStudyPlanTaskHistory(id);
            return Ok(result);
        }

        [HttpGet("GetStudyPlanDependentTaskList/{StudyPlanTaskId:int?}/{ProjectId:int}")]
        public IActionResult GetStudyPlanDependentTaskList(int? StudyPlanTaskId, int ProjectId)
        {
            var studyplan = _studyPlanTaskRepository.GetStudyPlanDependentTaskList(StudyPlanTaskId, ProjectId);
            return Ok(studyplan);
        }

        [Route("GetDocChart/{projectId}/{filterId:int}")]
        [HttpGet]
        public IActionResult GetDocChart(int projectId, CtmsStudyTaskFilter filterId)
        {
            var result = _studyPlanTaskRepository.GetDocChart(projectId, filterId);
            return Ok(result);
        }

        [Route("GetChartReport/{projectId}/{chartType:int?}/{filterId:int}")]
        [HttpGet]
        public IActionResult GetChartReport(int projectId, CtmsChartType? chartType, CtmsStudyTaskFilter filterId)
        {
            var report = _studyPlanTaskRepository.GetChartReport(projectId, chartType, filterId);
            return Ok(report);
        }
        [HttpPut("AddPreApproval")]
        public IActionResult AddPreApproval([FromBody] PreApprovalStatusDto data)
        {
            if (data.Id <= 0) return BadRequest();
            var task = _studyPlanTaskRepository.FindByInclude(x => (x.Id == data.Id && x.DependentTaskId == null) || (x.Id == data.DependentTaskId && x.DependentTaskId == data.Id)).ToList();
            if (task.Count > 0)
            {
                //if (task.Exists(s => s.Id == data.DependentTaskId))
                //{
                //    ModelState.AddModelError("Message", "This Task All Ready PreApproval");
                //    return BadRequest(ModelState);
                //}
                foreach (var item in task)
                {
                    var tastMaster = _mapper.Map<StudyPlanTask>(item);
                    tastMaster.PreApprovalStatus = data.PreApprovalStatus;
                    _studyPlanTaskRepository.Update(tastMaster);
                    _uow.Save();
                }
            }
            else
            {
                ModelState.AddModelError("Message", "All Ready PreApproval Dependent Task.");
                return BadRequest(ModelState);
            }
            return Ok(data);
        }
        [HttpGet("GetPreApprovalList/{isDeleted:bool?}/{studyPlanTaskId}")]
        public IActionResult GetPreApprovalList(bool isDeleted, int studyPlanTaskId)
        {
            var task = _studyPlanTaskRepository.FindByInclude(x => (x.Id == studyPlanTaskId && x.PreApprovalStatus == true) || (x.DependentTaskId == studyPlanTaskId && x.PreApprovalStatus == true) && (x.DeletedBy == null)).ToList();
            return Ok(task);
        }

        [HttpPatch("patchPreApproval/{id}")]
        public ActionResult patchPreApproval(int id)
        {
            var record = _studyPlanTaskRepository.Find(id);
            if (record == null) return NotFound();
            var tastMaster = _mapper.Map<StudyPlanTask>(record);
            tastMaster.PreApprovalStatus = false;
            tastMaster.ApprovalStatus = false;
            tastMaster.FileName = null;
            tastMaster.DocumentPath = null;
            _studyPlanTaskRepository.Update(tastMaster);
            _uow.Save();
            return Ok();
        }

        [HttpPut("AddApproval")]
        public IActionResult AddApproval([FromBody] ApprovalStatusDto data)
        {
            if (data.Id <= 0) return BadRequest();

            var record = _studyPlanTaskRepository.Find(data.Id);
            var tastMaster = _mapper.Map<StudyPlanTask>(record);

            DocumentService.RemoveFile(_uploadSettingRepository.GetDocumentPath(), record.DocumentPath);
            if (data.FileModel?.Base64?.Length > 0)
            {
                tastMaster.DocumentPath = _uploadSettingRepository.GetWebDocumentUrl() + DocumentService.SaveUploadDocument(data.FileModel, _uploadSettingRepository.GetDocumentPath(), _jwtTokenAccesser.CompanyId.ToString(), FolderType.Ctms, "StudyPlanTask");
            }
            tastMaster.ApprovalStatus = data.ApprovalStatus;
            tastMaster.PreApprovalStatus = true;
            tastMaster.FileName = data.FileName;
            _studyPlanTaskRepository.Update(tastMaster);
            _uow.Save();
            return Ok();
        }
        [HttpPost]
        [Route("ResourceMgmtQuerySearch")]
        public IActionResult ResourceMgmtQuerySearch([FromBody] ResourceMgmtFilterDto search)
        {
            var volunteers = _studyPlanTaskRepository.ResourceMgmtSearch(search);
            return Ok(volunteers);
        }
        [HttpGet]
        [Route("GetRollDropDown/{studyplanId}")]
        public IActionResult getRollDropDown(int studyplanId)
        {
            return Ok(_studyPlanTaskRepository.GetRollDropDown(studyplanId));
        }
        [HttpGet]
        [Route("getUserDropDown/{studyplanId}")]
        public IActionResult getUserDropDown(int studyplanId)
        {
            return Ok(_studyPlanTaskRepository.GetUserDropDown(studyplanId));
        }
        [HttpGet]
        [Route("getDesignationStdDropDown/{studyplanId}")]
        public IActionResult getDesignationStdDropDown(int studyplanId)
        {
            return Ok(_studyPlanTaskRepository.GetDesignationStdDropDown(studyplanId));
        }

        [HttpGet]
        [Route("GetSubTaskList/{parentTaskId}")]
        public IActionResult GetSubTaskList(int parentTaskId)
        {
            return Ok(_studyPlanTaskRepository.GetSubTaskList(parentTaskId));
        }

        [HttpGet]
        [Route("DownloadDocument/{id}")]
        public IActionResult DownloadDocument(int id)
        {
            var file = _studyPlanTaskRepository.Find(id);
            var filepath = Path.Combine(_uploadSettingRepository.GetDocumentPath(),
                file.TaskDocumentFilePath);
            if (System.IO.File.Exists(filepath))
            {
                return File(System.IO.File.OpenRead(filepath), ObjectExtensions.GetMIMEType(file.TaskDocumentFileName), file.TaskDocumentFileName);
            }
            return NotFound();
        }
    }
}
