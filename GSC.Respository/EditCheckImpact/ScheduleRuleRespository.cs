using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;


namespace GSC.Respository.EditCheckImpact
{
    public class ScheduleRuleRespository : GenericRespository<ScreeningTemplate, GscContext>, IScheduleRuleRespository
    {
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IImpactService _impactService;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        public ScheduleRuleRespository(IImpactService editCheckImpactService,
            IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository) : base(uow, jwtTokenAccesser)
        {
            _impactService = editCheckImpactService;
            _uow = uow;
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

            CheckValidationProcess(targetScheduleTemplate, refrenceSchedule, isQuery, "", screeningTemplateBasic.ScreeningEntryId);

            _uow.Save();
            Context.DetectionAll();

            return targetScheduleTemplate;

        }

        void SetValue(List<ScheduleCheckValidateDto> scheduleCheckValidateDto, List<Data.Dto.Screening.ScreeningTemplateValueBasic> values, ScreeningTemplateBasic screeningTemplateBasic)
        {
            scheduleCheckValidateDto.ForEach(x =>
            {
                if (screeningTemplateBasic.ProjectDesignTemplateId == x.ProjectDesignTemplateId)
                {
                    x.Value = values.FirstOrDefault(t => t.ProjectDesignVariableId == x.ProjectDesignVariableId)?.Value;
                    x.ScreeningTemplateId = screeningTemplateBasic.Id;
                    x.Status = screeningTemplateBasic.Status;
                }
                else
                {
                    var scheduleTemplate = _impactService.GetScreeningTemplateId(x.ProjectDesignTemplateId, screeningTemplateBasic.ScreeningEntryId);
                    if (scheduleTemplate != null)
                    {
                        x.Status = scheduleTemplate.Status;
                        x.ScreeningTemplateId = scheduleTemplate.ScreeningTemplateId;
                    }
                    x.Value = _impactService.GetVariableValue(x.ScreeningTemplateId, x.ProjectDesignVariableId);
                }
            });
        }

        void CheckValidationProcess(List<ScheduleCheckValidateDto> targetList, List<ScheduleCheckValidateDto> referenceList, bool isQuery, string targetSchDate, int screeningEntryId)
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
                    if ((int)reference.Status > 2)
                        x.HasQueries = SystemQuery(x.ScreeningTemplateId, x.ProjectDesignVariableId,
                               x.AutoNumber, x.Message, "");
                }

                InsertScheduleDate(x, screeningEntryId, targetSchDate);

            });
        }


        public List<ScheduleCheckValidateDto> ValidateByVariable(int screeningEntryId, int screeningTemplateId, string value, int projectDesignTemplateId, int projectDesignVariableId, bool isQuery)
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

            SetValue(refrenceSchedule, null, new ScreeningTemplateBasic { ScreeningEntryId = screeningEntryId });
            SetValue(targetScheduleTemplate, null, new ScreeningTemplateBasic { ScreeningEntryId = screeningEntryId });

            targetValue = refrenceSchedule.FirstOrDefault(x => x.Value != null)?.Value;

            CheckValidationProcess(targetScheduleTemplate, refrenceSchedule, isQuery, targetValue, screeningEntryId);

            _uow.Save();

            Context.DetectionAll();

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


        private bool Validate(ScheduleCheckValidateDto scheduleCheckValidateDto, string targetDate, string referenceDate)
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

        void InsertScheduleDate(ScheduleCheckValidateDto target, int screeningEntryId, string targetSchDate)
        {
            if (string.IsNullOrEmpty(targetSchDate) || target.Operator != ProjectScheduleOperator.Plus)
                return;

            DateTime scheduleDate;
            DateTime.TryParse(targetSchDate, out scheduleDate);

            Context.DetectionAll();

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

            var screeningTemplate = _impactService.GetScreeningTemplateId(target.ProjectDesignTemplateId, screeningEntryId);
            var screeningTemplateValue = _screeningTemplateValueRepository.All.AsNoTracking().Where(x =>
                x.ProjectDesignVariableId == target.ProjectDesignVariableId &&
                x.ScreeningTemplateId == screeningTemplate.ScreeningTemplateId).FirstOrDefault();

            if (screeningTemplateValue == null)
            {
                screeningTemplateValue = new ScreeningTemplateValue
                {
                    ScreeningTemplateId = screeningTemplate.ScreeningTemplateId,
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
            _uow.Save();

            target.ScreeningTemplateValueId = screeningTemplateValue.Id;

            if (target.ScheduleDate != null)
                VisitScheduleDate(screeningTemplate.ScreeningVisitId, (DateTime)target.ScheduleDate);
        }

        void VisitScheduleDate(int screeningVisitId, DateTime ScheduleDate)
        {
            var screeningVisit = _uow.Context.ScreeningVisit.Find(screeningVisitId);
            if (screeningVisit == null) return;

            if (_projectDesignVisitStatusRepository.All.Any(x => x.ProjectDesignVisitId == screeningVisit.ProjectDesignVisitId
           && x.VisitStatusId == ScreeningVisitStatus.Open))
            {
                screeningVisit.VisitStartDate = ScheduleDate;
                if (screeningVisit.IsSchedule && screeningVisit.Status > ScreeningVisitStatus.ReSchedule)
                    screeningVisit.ScheduleDate = ScheduleDate;
                _uow.Context.ScreeningVisit.Update(screeningVisit);
            }


        }

        bool SystemQuery(int screeningTemplateId, int projectDesignVariableId, string autoNumber, string message, string sampleResult)
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
                        EditCheckRefValue = sampleResult,
                        ScreeningTemplateValueId = screeningTemplateValue.Id
                    }, screeningTemplateValue);

                _uow.Save();
                Context.DetectionAll();

                return true;
            }

            return false;
        }


    }
}
