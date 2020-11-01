using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningVisitRepository : GenericRespository<ScreeningVisit, GscContext>, IScreeningVisitRepository
    {
        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IScreeningVisitHistoryRepository _screeningVisitHistoryRepository;
        private readonly IProjectDesignVisitStatusRepository _projectDesignVisitStatusRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        public ScreeningVisitRepository(IUnitOfWork<GscContext> uow,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IScreeningVisitHistoryRepository screeningVisitHistoryRepository,
            IRandomizationRepository randomizationRepository,
            IProjectDesignVisitStatusRepository projectDesignVisitStatusRepository,
            IJwtTokenAccesser jwtTokenAccesser,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IScreeningTemplateRepository screeningTemplateRepository)
            : base(uow, jwtTokenAccesser)
        {
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _screeningVisitHistoryRepository = screeningVisitHistoryRepository;
            _randomizationRepository = randomizationRepository;
            _projectDesignVisitStatusRepository = projectDesignVisitStatusRepository;
            _uow = uow;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
        }

        public void ScreeningVisitSave(ScreeningEntry screeningEntry, int projectDesignPeriodId, int projectDesignVisitId, DateTime visitDate)
        {
            var openVariable = _projectDesignVisitStatusRepository.All.Where(x => x.ProjectDesignVisitId == projectDesignVisitId && x.VisitStatusId == ScreeningVisitStatus.Open).
                Select(t => new { t.ProjectDesignVariable.Id, t.ProjectDesignVariable.ProjectDesignTemplateId }).FirstOrDefault();

            var designVisits = _projectDesignVisitRepository.GetVisitAndTemplateByPeriordId(projectDesignPeriodId);
            screeningEntry.ScreeningVisit = new List<ScreeningVisit>();
            designVisits.ForEach(r =>
            {
                var screeningVisit = new ScreeningVisit
                {

                    ProjectDesignVisitId = r.Id,
                    Status = projectDesignVisitId == r.Id ? ScreeningVisitStatus.Open : ScreeningVisitStatus.NotStarted,
                    IsSchedule = r.IsSchedule ?? false,
                    ScreeningTemplates = new List<ScreeningTemplate>()
                };

                if (screeningVisit.Status == ScreeningVisitStatus.Open)
                {
                    screeningVisit.VisitStartDate = visitDate;
                    _screeningVisitHistoryRepository.SaveByScreeningVisit(screeningVisit, ScreeningVisitStatus.Open, visitDate);
                }

                r.Templates.ForEach(t =>
                {
                    var screeningTemplate = new ScreeningTemplate
                    {
                        ProjectDesignTemplateId = t,
                        Status = ScreeningTemplateStatus.Pending
                    };
                    if (openVariable != null && openVariable.ProjectDesignTemplateId == t)
                    {
                        if (SaveVariableValue(visitDate.ToString(), screeningTemplate, openVariable.Id, openVariable.ProjectDesignTemplateId))
                        {
                            screeningVisit.Status = ScreeningVisitStatus.InProgress;
                            screeningTemplate.Status = ScreeningTemplateStatus.InProcess;
                            _screeningVisitHistoryRepository.SaveByScreeningVisit(screeningVisit, ScreeningVisitStatus.InProgress, visitDate);
                        }

                    }
                    screeningVisit.ScreeningTemplates.Add(screeningTemplate);

                });

                Add(screeningVisit);

                screeningEntry.ScreeningVisit.Add(screeningVisit);
            });
        }

        private bool SaveVariableValue(string value, ScreeningTemplate screeningTemplate, int projectDesignVariableId, int projectDesignTemplateId)
        {
            if (!_projectDesignVariableRepository.All.Any(x => x.ProjectDesignTemplateId == projectDesignTemplateId && x.Id == projectDesignVariableId))
                return false;

            var aduits = new List<ScreeningTemplateValueAudit>
            {
                new ScreeningTemplateValueAudit
                {
                    Value = value,
                    Note = "Save value from open visit"
                }
            };

            var screeningTemplateValue = new ScreeningTemplateValue
            {
                ScreeningTemplate = screeningTemplate,
                ProjectDesignVariableId = projectDesignVariableId,
                Value = value,
                Audits = aduits
            };

            _screeningTemplateValueRepository.Add(screeningTemplateValue);

            return true;

        }

        public void StatusUpdate(ScreeningVisitHistoryDto screeningVisitHistoryDto)
        {
            var visit = Find(screeningVisitHistoryDto.ScreeningVisitId);

            visit.Status = screeningVisitHistoryDto.VisitStatusId;

            Update(visit);

            _screeningVisitHistoryRepository.Save(screeningVisitHistoryDto);

            _uow.Save();

            PatientStatus(visit.ScreeningEntryId);
        }

        public bool IsPatientScreeningFailure(int screeningVisitId)
        {
            var visit = Find(screeningVisitId);
            return All.Any(t => t.ScreeningEntryId == visit.ScreeningEntryId && (
           t.ScreeningEntry.Randomization.PatientStatusId == ScreeningPatientStatus.ScreeningFailure ||
           t.ScreeningEntry.Randomization.PatientStatusId == ScreeningPatientStatus.Withdrawal));
        }

        public void OpenVisit(int screeningVisitId, DateTime visitDate)
        {

            var visit = Find(screeningVisitId);

            visit.Status = ScreeningVisitStatus.Open;
            visit.VisitStartDate = visitDate;

            Update(visit);

            _screeningVisitHistoryRepository.SaveByScreeningVisit(visit, ScreeningVisitStatus.Open, visitDate);

            _uow.Save();

            PatientStatus(visit.ScreeningEntryId);
        }


        public void PatientStatus(int screeningEntryId)
        {
            var visitStatus = All.Where(x => x.ScreeningEntryId == screeningEntryId).GroupBy(t => t.Status).Select(r => r.Key).ToList();
            var patientStatus = ScreeningPatientStatus.OnTrial;


            if (visitStatus.Any(x => x == ScreeningVisitStatus.Missed || x == ScreeningVisitStatus.OnHold))
                patientStatus = ScreeningPatientStatus.OnHold;

            if (visitStatus.Any(x => x == ScreeningVisitStatus.Withdrawal))
                patientStatus = ScreeningPatientStatus.Withdrawal;

            if (visitStatus.Any(x => x == ScreeningVisitStatus.ScreeningFailure))
                patientStatus = ScreeningPatientStatus.ScreeningFailure;

            if (!visitStatus.Any(x => x != ScreeningVisitStatus.Completed))
                patientStatus = ScreeningPatientStatus.Completed;

            _randomizationRepository.PatientStatus(patientStatus, screeningEntryId);
        }

        public void AutomaticStatusUpdate(int screeningTemplateId)
        {
            var screeningVisit = _screeningTemplateRepository.All.Where(x => x.Id == screeningTemplateId).Select(t => new
            {
                t.ProjectDesignTemplate.ProjectDesignVisitId,
                t.ScreeningVisitId,
                t.Status,
                t.ScreeningVisit.ScreeningEntryId
            }).FirstOrDefault();

            if (screeningVisit == null) return;


            var designVisitStatus = _projectDesignVisitStatusRepository.All.Where(x => x.DeletedDate == null && x.ProjectDesignVisitId == screeningVisit.ProjectDesignVisitId).Select(
                  t => new { t.ProjectDesignVariableId, t.VisitStatusId }).ToList();
            DateTime statusDate = System.DateTime.Now;

            if (designVisitStatus != null && designVisitStatus.Count() > 0)
            {
                var designVariable = designVisitStatus.FirstOrDefault(t => t.VisitStatusId > ScreeningVisitStatus.Scheduled);
                if (designVariable != null)
                {
                    var screeningValue = _screeningTemplateValueRepository.All.Where(t => t.ProjectDesignVariableId == designVariable.ProjectDesignVariableId
                      && t.ScreeningTemplateId == screeningTemplateId).Select(r => r.Value).FirstOrDefault();

                    if (!string.IsNullOrEmpty(screeningValue))
                    {
                        DateTime.TryParse(screeningValue, out statusDate);
                        StatusUpdate(new ScreeningVisitHistoryDto
                        {
                            VisitStatusId = designVariable.VisitStatusId,
                            ScreeningVisitId = screeningVisit.ScreeningVisitId,
                            StatusDate = statusDate
                        });
                    }
                }
            }

            if (screeningVisit.Status == ScreeningTemplateStatus.Submitted)
            {
                if (!_screeningTemplateRepository.All.Any(x => x.ScreeningVisit.ScreeningEntryId == screeningVisit.ScreeningEntryId
                && x.ScreeningVisitId == screeningVisit.ScreeningVisitId && x.Status < ScreeningTemplateStatus.Submitted))
                    StatusUpdate(new ScreeningVisitHistoryDto
                    {
                        VisitStatusId = ScreeningVisitStatus.Completed,
                        ScreeningVisitId = screeningVisit.ScreeningVisitId,
                        StatusDate = statusDate
                    });

            }

        }
        public void VisitRepeat(int projectDesignVisitId, int screeningEntryId)
        {
            var repeatedCount = 0;
            var projectVisit = All.Include(r => r.ScreeningTemplates).Where(x => x.ProjectDesignVisitId == projectDesignVisitId
                                              && x.ScreeningEntryId == screeningEntryId && x.ParentId == null).FirstOrDefault();

            if (projectVisit != null)
                repeatedCount = All.Where(x => x.ScreeningEntryId == screeningEntryId && x.ProjectDesignVisitId == projectDesignVisitId).Max(t => t.RepeatedVisitNumber) ?? 0;


            var screeningVisit = new ScreeningVisit
            {

                ProjectDesignVisitId = projectDesignVisitId,
                Status = ScreeningVisitStatus.NotStarted,
                ScreeningEntryId = screeningEntryId,
                RepeatedVisitNumber = repeatedCount + 1,
                ParentId = projectVisit.Id,
                ScreeningTemplates = new List<ScreeningTemplate>()
            };

            screeningVisit.ScreeningTemplates.ForEach(t =>
            {
                screeningVisit.ScreeningTemplates.Add(new ScreeningTemplate
                {
                    ProjectDesignTemplateId = t.Id,
                    Status = ScreeningTemplateStatus.Pending
                });
            });

            Add(screeningVisit);


        }
    }
}
