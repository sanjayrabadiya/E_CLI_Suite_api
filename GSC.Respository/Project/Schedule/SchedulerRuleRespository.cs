using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Schedule;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Screening;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.Schedule
{
    public class SchedulerRuleRespository : GenericRespository<ProjectScheduleTemplate, GscContext>,
        ISchedulerRuleRespository
    {
        private List<int> _projectDesignVariableId;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IScreeningTemplateValueScheduleRepository _screeningTemplateValueScheduleRepository;

        public SchedulerRuleRespository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IScreeningTemplateValueScheduleRepository screeningTemplateValueScheduleRepository) : base(uow,
            jwtTokenAccesser)
        {
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _screeningTemplateValueScheduleRepository = screeningTemplateValueScheduleRepository;
        }

        public void ValidateRuleByTemplate(int screeningTemplateId, int projectDesignTemplateId, int screeningEntryId,
            bool isFromQuery, ref List<int> variableList)
        {
            _projectDesignVariableId = variableList;

            var refrenceSchedule = Context.ProjectSchedule.Where(r =>
                (r.Templates.Any(c => c.ProjectDesignTemplateId == projectDesignTemplateId
                                      && c.DeletedDate == null)
                 || r.ProjectDesignTemplateId == projectDesignTemplateId) && r.DeletedDate == null).ToList();

            var targetScheduleTemplate = All.Where(x =>
                    (x.ProjectDesignTemplateId == projectDesignTemplateId ||
                     x.ProjectSchedule.ProjectDesignTemplateId == projectDesignTemplateId)
                    && x.ProjectSchedule.DeletedDate == null && x.DeletedDate == null)
                .Include(r => r.ProjectDesignVariable)
                .ToList();
            _projectDesignVariableId = variableList;
            ValidateSchedule(screeningEntryId, screeningTemplateId, projectDesignTemplateId, refrenceSchedule,
                targetScheduleTemplate, isFromQuery);
        }

        public void SchedulerRuleByVariable(VariableEditCheckDto variableEditCheckDto, ref List<int> variableList)
        {
            _projectDesignVariableId = variableList;

            var refrenceSchedule = Context.ProjectSchedule.Where(r =>
                (r.Templates.Any(c =>
                     c.ProjectDesignTemplateId == variableEditCheckDto.ProjectDesignTemplateId &&
                     c.ProjectDesignVariableId == variableEditCheckDto.ProjectDesignVariableId &&
                     c.DeletedDate == null)
                 || r.ProjectDesignTemplateId == variableEditCheckDto.ProjectDesignTemplateId &&
                 r.ProjectDesignVariableId == variableEditCheckDto.ProjectDesignVariableId)
                && r.DeletedDate == null).ToList();

            var targetScheduleTemplate = All.Where(x =>
                    (x.ProjectDesignTemplateId == variableEditCheckDto.ProjectDesignTemplateId &&
                     x.ProjectDesignVariableId == variableEditCheckDto.ProjectDesignVariableId ||
                     x.ProjectSchedule.ProjectDesignTemplateId == variableEditCheckDto.ProjectDesignTemplateId &&
                     x.ProjectSchedule.ProjectDesignVariableId == variableEditCheckDto.ProjectDesignVariableId) &&
                    x.ProjectSchedule.DeletedDate == null && x.DeletedDate == null)
                .Include(r => r.ProjectDesignVariable)
                .ToList();

            ValidateSchedule(variableEditCheckDto.ScreeningEntryId, variableEditCheckDto.ScreeningTemplateId,
                variableEditCheckDto.ProjectDesignTemplateId, refrenceSchedule, targetScheduleTemplate,
                variableEditCheckDto.IsFromQuery);
        }

        private void ValidateSchedule(int screeningEntryId, int screeningTemplateId, int projectDesignTemplateId,
            List<ProjectSchedule> refrenceSchedule, List<ProjectScheduleTemplate> targetScheduleTemplate,
            bool isFromQuery)
        {
            if (targetScheduleTemplate == null || targetScheduleTemplate.Count() == 0) return;

            if (refrenceSchedule == null || refrenceSchedule.Count() == 0) return;


            targetScheduleTemplate.ForEach(x =>
            {
                var templateId = screeningTemplateId;

                if (x.ProjectDesignTemplateId != projectDesignTemplateId)
                    templateId = Context.ScreeningTemplate.FirstOrDefault(c =>
                                         c.ScreeningEntryId == screeningEntryId &&
                                         c.ProjectDesignTemplateId == x.ProjectDesignTemplateId && c.ParentId == null)
                                     ?.Id ?? 0;

                if (templateId == 0) return;

                var screeningTargetValue = VariableValue(x.ProjectDesignVariableId, screeningTemplateId);
                var validateRule = false;

                if (!string.IsNullOrEmpty(screeningTargetValue))
                {
                    var refrenceTemplate = refrenceSchedule.Where(r => r.Id == x.ProjectScheduleId).FirstOrDefault();
                    if (refrenceTemplate != null)
                    {
                        var refrenceTemplateId = screeningTemplateId;

                        if (refrenceTemplate.ProjectDesignTemplateId != projectDesignTemplateId)
                            refrenceTemplateId = Context.ScreeningTemplate.FirstOrDefault(c =>
                                c.ScreeningEntryId == screeningEntryId &&
                                c.ProjectDesignTemplateId == refrenceTemplate.ProjectDesignTemplateId &&
                                c.ParentId == null).Id;

                        var screeningSourceValue =
                            VariableValue(refrenceTemplate.ProjectDesignVariableId, refrenceTemplateId);
                        if (!string.IsNullOrEmpty(screeningSourceValue))
                        {
                            validateRule = ValidateRule(x, screeningSourceValue, screeningTargetValue,
                                x.ProjectDesignVariable.CollectionSource);

                            if (!validateRule && isFromQuery)
                                SystemQuery(templateId, x.ProjectDesignVariableId, x.Message);
                        }
                    }
                }


                _screeningTemplateValueScheduleRepository.InsertUpdate(
                    new ScreeningTemplateValueScheduleDto
                    {
                        ScreeningTemplateId = screeningTemplateId,
                        ProjectDesignVariableId = x.ProjectDesignVariableId,
                        IsVerify = validateRule,
                        Message = x.Message,
                        IsStarted = string.IsNullOrEmpty(screeningTargetValue),
                        ScreeningEntryId = screeningEntryId
                    });
            });
        }

        private string VariableValue(int projectDesignVariableId, int screeningTemplateId)
        {
            var screeningValue = Context.ScreeningTemplateValue.AsNoTracking().Where(t =>
                t.ProjectDesignVariableId == projectDesignVariableId
                && t.ScreeningTemplate.Id == screeningTemplateId).Select(v => new ScreeningTemplateValue
            {
                Value = v.Value,
                IsNa = v.IsNa,
                ProjectDesignVariable = v.ProjectDesignVariable
            }).FirstOrDefault();

            return screeningValue?.Value;
        }

        private bool ValidateRule(ProjectScheduleTemplate projectScheduleTemplate, string sourceValue,
            string targetValue, CollectionSources collectionSources)
        {
            if (string.IsNullOrEmpty(targetValue)) return false;

            switch (projectScheduleTemplate.Operator)
            {
                case ProjectScheduleOperator.Equal:
                    if (collectionSources == CollectionSources.Date)
                        return Convert.ToDateTime(targetValue).Date == Convert.ToDateTime(sourceValue).Date;
                    else
                        return Convert.ToDateTime(targetValue) == Convert.ToDateTime(sourceValue);

                case ProjectScheduleOperator.Greater:
                    if (collectionSources == CollectionSources.Date)
                        return Convert.ToDateTime(targetValue).Date > Convert.ToDateTime(sourceValue).Date;
                    else
                        return Convert.ToDateTime(targetValue) > Convert.ToDateTime(sourceValue);
                case ProjectScheduleOperator.GreaterEqual:
                    if (collectionSources == CollectionSources.Date)
                        return Convert.ToDateTime(targetValue).Date >= Convert.ToDateTime(sourceValue).Date;
                    else
                        return Convert.ToDateTime(targetValue) >= Convert.ToDateTime(sourceValue);
                case ProjectScheduleOperator.Lessthen:
                    if (collectionSources == CollectionSources.Date)
                        return Convert.ToDateTime(targetValue).Date < Convert.ToDateTime(sourceValue).Date;
                    else
                        return Convert.ToDateTime(targetValue) < Convert.ToDateTime(sourceValue);
                case ProjectScheduleOperator.LessthenEqual:
                    if (collectionSources == CollectionSources.Date)
                        return Convert.ToDateTime(targetValue).Date <= Convert.ToDateTime(sourceValue).Date;
                    else
                        return Convert.ToDateTime(targetValue) <= Convert.ToDateTime(sourceValue);
                case ProjectScheduleOperator.Plus:
                {
                    string postiveSourceValue;
                    string negativeSourceValue;

                    if (projectScheduleTemplate.ProjectDesignVariable.CollectionSource == CollectionSources.Date)
                    {
                        if (projectScheduleTemplate.NoOfDay.HasValue)
                            sourceValue = Convert.ToDateTime(sourceValue)
                                .AddDays(Convert.ToDouble(projectScheduleTemplate.NoOfDay))
                                .ToString(CultureInfo.InvariantCulture);

                        postiveSourceValue = Convert.ToDateTime(sourceValue)
                            .AddDays(projectScheduleTemplate.PositiveDeviation).ToString(CultureInfo.InvariantCulture);
                        negativeSourceValue = Convert.ToDateTime(sourceValue)
                            .AddDays(-projectScheduleTemplate.NegativeDeviation).ToString(CultureInfo.CurrentCulture);
                    }
                    else
                    {
                        if (projectScheduleTemplate.HH.HasValue)
                            sourceValue = Convert.ToDateTime(sourceValue)
                                .AddHours(Convert.ToDouble(projectScheduleTemplate.HH))
                                .ToString(CultureInfo.InvariantCulture);

                        if (projectScheduleTemplate.MM.HasValue)
                            sourceValue = Convert.ToDateTime(sourceValue)
                                .AddMinutes(Convert.ToDouble(projectScheduleTemplate.MM))
                                .ToString(CultureInfo.InvariantCulture);

                        postiveSourceValue = Convert.ToDateTime(sourceValue)
                            .AddMinutes(projectScheduleTemplate.PositiveDeviation)
                            .ToString(CultureInfo.InvariantCulture);
                        negativeSourceValue = Convert.ToDateTime(sourceValue)
                            .AddMinutes(-projectScheduleTemplate.NegativeDeviation)
                            .ToString(CultureInfo.InvariantCulture);
                    }

                    if (collectionSources == CollectionSources.Date)
                    {
                        if (Convert.ToDateTime(targetValue).Date >= Convert.ToDateTime(negativeSourceValue).Date &&
                            Convert.ToDateTime(targetValue).Date <= Convert.ToDateTime(postiveSourceValue).Date)
                            return true;
                        return false;
                    }

                    if (Convert.ToDateTime(targetValue) >= Convert.ToDateTime(negativeSourceValue) &&
                        Convert.ToDateTime(targetValue) <= Convert.ToDateTime(postiveSourceValue))
                        return true;
                    return false;
                }

                default:
                    return false;
            }
        }

        public void SystemQuery(int screeningTemplateId, int projectDesignVariableId, string message)
        {
            var screeningTemplateValue = Context.ScreeningTemplateValue.AsNoTracking().FirstOrDefault
                                         (t => t.ScreeningTemplateId == screeningTemplateId
                                               && t.ProjectDesignVariableId == projectDesignVariableId) ??
                                         new ScreeningTemplateValue();

            var screeningTemplate = Context.ScreeningTemplate.Find(screeningTemplateId);
            if (screeningTemplate == null || (int) screeningTemplate.Status < 3)
                return;

            if (screeningTemplateValue.IsSystem && screeningTemplateValue.QueryStatus == QueryStatus.Open)
                return;

            if (_projectDesignVariableId != null &&
                _projectDesignVariableId.Any(c => c == screeningTemplateValue.ProjectDesignVariableId))
                return;

            _projectDesignVariableId.Add(screeningTemplateValue.ProjectDesignVariableId);
            screeningTemplateValue.ProjectDesignVariableId = projectDesignVariableId;
            screeningTemplateValue.IsSystem = true;
            _screeningTemplateValueQueryRepository.GenerateQuery(
                new ScreeningTemplateValueQueryDto
                {
                    QueryStatus = QueryStatus.Open,
                    IsSystem = true,
                    Note = message,
                    ScreeningTemplateValueId = screeningTemplateValue.Id
                },
                new ScreeningTemplateValueQuery
                {
                    ScreeningTemplateValue = screeningTemplateValue,
                    QueryStatus = QueryStatus.Open,
                    IsSystem = true,
                    Note = message,
                    ScreeningTemplateValueId = screeningTemplateValue.Id
                }, screeningTemplateValue);
        }
    }
}