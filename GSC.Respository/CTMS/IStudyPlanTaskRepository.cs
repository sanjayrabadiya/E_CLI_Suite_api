﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Audit;
using GSC.Data.Dto.CTMS;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.Location;
using GSC.Helper;
using System;
using System.Collections.Generic;

namespace GSC.Respository.CTMS
{
    public interface IStudyPlanTaskRepository : IGenericRepository<StudyPlanTask>
    {
        StudyPlanTaskGridDto GetStudyPlanTaskList(bool isDeleted, int StudyPlanId, int ProjectId, CtmsStudyTaskFilter filterType);
        int UpdateTaskOrder(StudyPlantaskParameterDto taskmasterDto);
        string ValidateTask(StudyPlanTask taskmasterDto);
        void UpdateParentDate(int? ParentId);
        void InsertDependentTask(List<DependentTaskParameterDto> dependentTasks, int StudyPlanTaskId);
        void UpdateTaskOrderSequence(int StudyPlanId);
        string UpdateDependentTask(int StudyPlanTaskId);
        StudyPlanTask UpdateDependentTaskDate(StudyPlanTask StudyPlanTask);
        DateTime GetNextWorkingDate(NextWorkingDateParameterDto parameterDto);
        string ValidateweekEnd(NextWorkingDateParameterDto parameterDto);
        List<StudyPlanTask> Save(StudyPlanTask taskData);
        List<AuditTrailDto> GetStudyPlanTaskHistory(int id);
        StudyPlanTaskGridDto GetStudyPlanDependentTaskList(int? StudyPlanTaskId, int ProjectId);
        StudyPlanTaskChartDto GetDocChart(int projectId, int? countryId);
        List<StudyPlanTaskChartReportDto> GetChartReport(int projectId, CtmsChartType? chartType, int? countryId);
        List<StudyPlanTaskDto> ResourceMgmtSearch(ResourceMgmtFilterDto search);
        List<DropDownDto> GetRollDropDown(int studyplanId);
        List<DropDownDto> GetUserDropDown(int studyplanId);
        List<DropDownDto> GetDesignationStdDropDown(int studyplanId);
        List<StudyPlanTaskDto> getBudgetPlaner(bool isDeleted, int studyId, int siteId, int countryId);
        List<StudyPlanTaskDto> GetSubTaskList(int parentTaskId);
        ParentTaskDate GetChildStartEndDate(int parentTaskId);
    }
}
