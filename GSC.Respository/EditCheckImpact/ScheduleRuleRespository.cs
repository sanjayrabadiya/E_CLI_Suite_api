using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;


namespace GSC.Respository.EditCheckImpact
{
    public class ScheduleRuleRespository : GenericRespository<ScreeningTemplate>, IScheduleRuleRespository
    {
        private readonly IGSCContext _context;
        private readonly IImpactService _impactService;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        public ScheduleRuleRespository(IImpactService editCheckImpactService,
            IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository) : base(context)
        {
            _impactService = editCheckImpactService;
            _context = context;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
        }

        public List<ScheduleCheckValidateDto> ValidateByTemplate(List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic, bool isQuery)
        {
            var targetScheduleTemplate = _impactService.GetTargetSchedule(screeningTemplateBasic.ProjectDesignTemplateId, isQuery);

            if (targetScheduleTemplate == null || targetScheduleTemplate.Count == 0) return null;
            var projectScheduleId = targetScheduleTemplate.Select(t => t.ProjectScheduleId).ToList();
            var refrenceSchedule = _impactService.GetReferenceSchedule(projectScheduleId);
            if (refrenceSchedule == null || refrenceSchedule.Count == 0) return null;

            SetValue(refrenceSchedule, values, screeningTemplateBasic);
            SetValue(targetScheduleTemplate, values, screeningTemplateBasic);

            CheckValidationProcess(targetScheduleTemplate, refrenceSchedule, isQuery, "", screeningTemplateBasic.ScreeningVisitId);

            _context.Save();
            _context.DetachAllEntities();

            return targetScheduleTemplate;

        }

        void SetValue(List<ScheduleCheckValidateDto> scheduleCheckValidateDto, List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic)
        {
            var projectDesignTemplateIds = scheduleCheckValidateDto.Select(t => t.ProjectDesignTemplateId).Distinct().ToList();

            var screeningTempaltes = All.Where(x => projectDesignTemplateIds.Contains(x.ProjectDesignTemplateId) && x.ParentId == null &&
            x.ScreeningVisit.ScreeningEntryId == screeningTemplateBasic.ScreeningEntryId && x.ScreeningVisit.ParentId == null);

            scheduleCheckValidateDto.ForEach(x =>
            {
                x.ScreeningTemplate = screeningTempaltes.FirstOrDefault(t => t.ProjectDesignTemplateId == x.ProjectDesignTemplateId);
                if (screeningTemplateBasic.ProjectDesignTemplateId == x.ProjectDesignTemplateId)
                    x.Value = values.FirstOrDefault(t => t.ProjectDesignVariableId == x.ProjectDesignVariableId)?.Value;
                else if (x.ScreeningTemplate != null && (x.ScreeningTemplate.Status > ScreeningTemplateStatus.Pending || !x.IsTarget))
                    x.Value = _impactService.GetVariableValue(x.ScreeningTemplate.Id, x.ProjectDesignVariableId);
            });
        }

        void CheckValidationProcess(List<ScheduleCheckValidateDto> targetList, List<ScheduleCheckValidateDto> referenceList, bool isQuery, string targetSchDate, int screeningVisitId)
        {
            targetList.ForEach(x =>
            {
                var reference = referenceList.FirstOrDefault(t => t.ProjectScheduleId == x.ProjectScheduleId);
                x.AutoNumber = reference?.AutoNumber;

                if (string.IsNullOrEmpty(reference?.Value))
                    x.ValidateType = EditCheckValidateType.NotProcessed;
                else
                    x.ValidateType = Validate(x, x.Value, reference.Value) ? EditCheckValidateType.RuleValidated : EditCheckValidateType.Failed;

                if (isQuery && x.ValidateType == EditCheckValidateType.Failed && reference != null)
                {
                    if (x.ScreeningTemplate != null && x.ScreeningTemplate.Status > ScreeningTemplateStatus.Pending)
                        x.HasQueries = SystemQuery(x.ScreeningTemplate.Id, x.ProjectDesignVariableId, x.AutoNumber, x.Message);
                }

                InsertScheduleDate(x, targetSchDate);
                _context.Save();
                _context.DetachAllEntities();
            });

            if (targetList != null && targetList.Count > 0 && !string.IsNullOrEmpty(targetSchDate))
                VisitScheduleDate(screeningVisitId, targetList);

        }

        public List<ScheduleCheckValidateDto> ValidateByVariable(int screeningEntryId, int screeningVisitId, string value, int projectDesignTemplateId, int projectDesignVariableId, bool isQuery)
        {
            var targetScheduleTemplate = _impactService.GetTargetScheduleByVariableId(projectDesignVariableId);

            if (targetScheduleTemplate == null || targetScheduleTemplate.Count == 0) return null;

            if (!isQuery)
                targetScheduleTemplate = targetScheduleTemplate.Where(x => x.ProjectDesignTemplateId == projectDesignTemplateId).ToList();

            if (targetScheduleTemplate.Count == 0) return null;

            var projectScheduleId = targetScheduleTemplate.Select(t => t.ProjectScheduleId).ToList();
            var refrenceSchedule = _impactService.GetReferenceSchedule(projectScheduleId);
            if (refrenceSchedule == null || refrenceSchedule.Count == 0) return null;
            string targetValue = "";

            var screeningTemplateBasic = new ScreeningTemplateBasic
            {
                ScreeningEntryId = screeningEntryId,
                ScreeningVisitId = screeningVisitId
            };

            SetValue(refrenceSchedule, null, screeningTemplateBasic);
            SetValue(targetScheduleTemplate, null, screeningTemplateBasic);

            targetValue = refrenceSchedule.FirstOrDefault(x => x.Value != null)?.Value;

          

            CheckValidationProcess(targetScheduleTemplate, refrenceSchedule, isQuery, targetValue, screeningVisitId);

            var currentTarget = targetScheduleTemplate.FirstOrDefault(x => x.ProjectDesignVariableId == projectDesignVariableId && x.ProjectDesignTemplateId == projectDesignTemplateId);
            if (currentTarget != null && !string.IsNullOrEmpty(value))
                TemplateActualDate(currentTarget.ScreeningTemplate, Convert.ToDateTime(value));

            VisitOpenDate(screeningVisitId, Convert.ToDateTime(value), projectDesignVariableId);

            return targetScheduleTemplate;
        }

        public List<EditCheckTargetValidationList> VariableResultProcess(List<EditCheckTargetValidationList> editCheckResult, List<ScheduleCheckValidateDto> scheduleResult)
        {
            if (scheduleResult == null || scheduleResult.Count == 0)
                return editCheckResult;

            if (editCheckResult == null)
                editCheckResult = new List<EditCheckTargetValidationList>();

            var projectDesignVariableIds = scheduleResult.Select(t => t.ProjectDesignVariableId).Distinct().ToList();
            projectDesignVariableIds.ForEach(x =>
            {
                var editcheck = editCheckResult.FirstOrDefault(r => r.ProjectDesignVariableId == x);
                if (editcheck == null)
                {
                    editcheck = new EditCheckTargetValidationList();
                    editcheck.isInfo = true;
                    editCheckResult.Add(editcheck);
                }

                if (scheduleResult.Any(c => c.ProjectDesignVariableId == x && c.ValidateType == EditCheckValidateType.Failed))
                    editcheck.isInfo = false;

                if (scheduleResult.Any(c => c.ProjectDesignVariableId == x && c.HasQueries))
                    editcheck.HasQueries = true;

                editcheck.ProjectDesignVariableId = x;
                var schMessage = scheduleResult.Where(c => c.ProjectDesignVariableId == x).Select(t => new EditCheckMessage
                {
                    AutoNumber = t.AutoNumber,
                    Message = t.Message,
                    ValidateType = t.ValidateType.GetDescription()
                }).ToList();
                editcheck.ScheduleDate = scheduleResult.Where(c => c.ProjectDesignVariableId == x).FirstOrDefault()?.ScheduleDate;
                editcheck.ScreeningTemplateValueId = scheduleResult.Where(c => c.ProjectDesignVariableId == x).Select(t => t.ScreeningTemplateValueId).FirstOrDefault();
                editcheck.EditCheckMsg.AddRange(schMessage);
            });

            return editCheckResult;
        }

        public bool Validate(ScheduleCheckValidateDto scheduleCheckValidateDto, string targetDate, string referenceDate)
        {
            if (string.IsNullOrEmpty(targetDate)) return false;
            if (string.IsNullOrEmpty(referenceDate)) return false;

            DateTime targetValue;
            DateTime.TryParse(targetDate, out targetValue);

            DateTime referenceValue;
            DateTime.TryParse(referenceDate, out referenceValue);

            switch (scheduleCheckValidateDto.Operator)
            {
                case ProjectScheduleOperator.Equal:
                    if (scheduleCheckValidateDto.CollectionSource == CollectionSources.Date)
                        return targetValue.Date == referenceValue.Date;
                    else
                        return targetValue == referenceValue;

                case ProjectScheduleOperator.Greater:
                    if (scheduleCheckValidateDto.CollectionSource == CollectionSources.Date)
                        return targetValue.Date > referenceValue.Date;
                    else
                        return targetValue > referenceValue;
                case ProjectScheduleOperator.GreaterEqual:
                    if (scheduleCheckValidateDto.CollectionSource == CollectionSources.Date)
                        return targetValue.Date >= referenceValue.Date;
                    else
                        return targetValue >= referenceValue;
                case ProjectScheduleOperator.Lessthen:
                    if (scheduleCheckValidateDto.CollectionSource == CollectionSources.Date)
                        return targetValue.Date < referenceValue.Date;
                    else
                        return targetValue < referenceValue;
                case ProjectScheduleOperator.LessthenEqual:
                    if (scheduleCheckValidateDto.CollectionSource == CollectionSources.Date)
                        return targetValue.Date <= referenceValue.Date;
                    else
                        return targetValue <= referenceValue;
                case ProjectScheduleOperator.Plus:
                    {
                        DateTime postiveSourceValue;
                        DateTime negativeSourceValue;

                        if (scheduleCheckValidateDto.CollectionSource == CollectionSources.Date)
                        {
                            if (scheduleCheckValidateDto.NoOfDay.HasValue)
                                referenceValue = referenceValue
                                    .AddDays(Convert.ToDouble(scheduleCheckValidateDto.NoOfDay));

                            postiveSourceValue = referenceValue
                                .AddDays(scheduleCheckValidateDto.PositiveDeviation);
                            negativeSourceValue = referenceValue
                                .AddDays(-scheduleCheckValidateDto.NegativeDeviation);
                        }
                        else
                        {
                            if (scheduleCheckValidateDto.HH.HasValue)
                                referenceValue = referenceValue
                                    .AddHours(Convert.ToDouble(scheduleCheckValidateDto.HH));

                            if (scheduleCheckValidateDto.MM.HasValue)
                                referenceValue = referenceValue
                                    .AddMinutes(Convert.ToDouble(scheduleCheckValidateDto.MM));

                            postiveSourceValue = referenceValue
                                .AddMinutes(scheduleCheckValidateDto.PositiveDeviation);
                            negativeSourceValue = referenceValue
                                .AddMinutes(-scheduleCheckValidateDto.NegativeDeviation);
                        }

                        if (scheduleCheckValidateDto.CollectionSource == CollectionSources.Date)
                        {
                            if (targetValue.Date >= Convert.ToDateTime(negativeSourceValue).Date &&
                                targetValue.Date <= Convert.ToDateTime(postiveSourceValue).Date)
                                return true;
                            return false;
                        }

                        if (targetValue >= Convert.ToDateTime(negativeSourceValue) &&
                            targetValue <= Convert.ToDateTime(postiveSourceValue))
                            return true;
                        return false;
                    }

                default:
                    return false;
            }
        }

        void InsertScheduleDate(ScheduleCheckValidateDto target, string targetSchDate)
        {
            if (string.IsNullOrEmpty(targetSchDate) || target.Operator != ProjectScheduleOperator.Plus)
                return;

            DateTime scheduleDate;
            DateTime.TryParse(targetSchDate, out scheduleDate);

            if (target.CollectionSource == CollectionSources.Date)
            {
                if (target.NoOfDay.HasValue)
                    scheduleDate = scheduleDate.AddDays(Convert.ToDouble(target.NoOfDay));

            }
            else
            {
                if (target.HH.HasValue)
                    scheduleDate = scheduleDate.AddHours(Convert.ToDouble(target.HH));

                if (target.MM.HasValue)
                    scheduleDate = scheduleDate.AddMinutes(Convert.ToDouble(target.MM));
            }

            target.ScheduleDate = scheduleDate;

            var screeningTemplateValue = _screeningTemplateValueRepository.All.AsNoTracking().Where(x =>
                x.ProjectDesignVariableId == target.ProjectDesignVariableId &&
                x.ScreeningTemplateId == target.ScreeningTemplate.Id).FirstOrDefault();

            if (screeningTemplateValue == null)
            {
                screeningTemplateValue = new ScreeningTemplateValue
                {
                    ScreeningTemplateId = target.ScreeningTemplate.Id,
                    ProjectDesignVariableId = target.ProjectDesignVariableId,
                    ScheduleDate = scheduleDate
                };
                _screeningTemplateValueRepository.Add(screeningTemplateValue);
            }
            else
            {
                screeningTemplateValue.ScheduleDate = scheduleDate;
                _screeningTemplateValueRepository.Update(screeningTemplateValue);
            }

            if (!target.ScreeningTemplate.IsProcess)
            {
                target.ScreeningTemplate.ScheduleDate = scheduleDate;
                target.ScreeningTemplate.IsProcess = true;
                Update(target.ScreeningTemplate);
            }


            target.ScreeningTemplateValueId = screeningTemplateValue.Id;
        }

        void VisitScheduleDate(int screeningVisitId, List<ScheduleCheckValidateDto> targetList)
        {
            var screeningVisit = _context.ScreeningVisit.Find(screeningVisitId);
            if (screeningVisit == null) return;

            if (screeningVisit.IsSchedule && screeningVisit.Status != ScreeningVisitStatus.ReSchedule)
            {
                screeningVisit.ScheduleDate = targetList.Min(x => x.ScheduleDate);
                _context.ScreeningVisit.Update(screeningVisit);
                _context.Save();
            }
        }

        void TemplateActualDate(ScreeningTemplate screeningTemplate, DateTime date)
        {
            if (screeningTemplate != null && !screeningTemplate.IsProcess)
            {
                screeningTemplate.ActualDate = date;
                screeningTemplate.IsProcess = true;
                Update(screeningTemplate);
            }
        }

        void VisitOpenDate(int screeningVisitId, DateTime dateTime, int projectDesignVariableId)
        {
            var screeningVisit = _context.ScreeningVisit.Find(screeningVisitId);
            if (screeningVisit == null) return;

            if (_projectDesignVisitStatusRepository.All.Any(x => x.ProjectDesignVisitId == screeningVisit.ProjectDesignVisitId
           && x.VisitStatusId == ScreeningVisitStatus.Open && x.ProjectDesignVariableId == projectDesignVariableId && x.DeletedDate == null))
            {
                screeningVisit.VisitStartDate = dateTime;
                _context.ScreeningVisit.Update(screeningVisit);
                _context.Save();
            }
        }

        bool SystemQuery(int screeningTemplateId, int projectDesignVariableId, string autoNumber, string message)
        {
            var screeningTemplateValue = _screeningTemplateValueRepository.All.AsNoTracking().Where
            (t => t.ScreeningTemplateId == screeningTemplateId
                  && t.ProjectDesignVariableId == projectDesignVariableId).FirstOrDefault();

            if (screeningTemplateValue != null)
            {
                if (screeningTemplateValue.IsSystem)
                    return false;

                var screeningTemplate = All.AsNoTracking().Where(x => x.Id == screeningTemplateId).FirstOrDefault();
                if ((int)screeningTemplate.Status < 3)
                    return false;


                string note = $"{"Edit Check by"} {autoNumber} {message}";
                screeningTemplateValue.IsSystem = true;
                _screeningTemplateValueQueryRepository.GenerateQuery(
                    new ScreeningTemplateValueQueryDto
                    {
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        Note = note,
                        ScreeningTemplateValueId = screeningTemplateValue.Id
                    },
                    new ScreeningTemplateValueQuery
                    {
                        ScreeningTemplateValue = screeningTemplateValue,
                        QueryStatus = QueryStatus.Open,
                        IsSystem = true,
                        Note = note,
                        ScreeningTemplateValueId = screeningTemplateValue.Id
                    }, screeningTemplateValue);

                _context.Save();

                return true;
            }

            return false;
        }


    }
}
