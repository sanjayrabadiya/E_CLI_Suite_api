using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class VersionEffectRepository : GenericRespository<ScreeningEntry>, IVersionEffectRepository
    {

        private readonly IProjectDesignVisitRepository _projectDesignVisitRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IScreeningVisitRepository _screeningVisitRepository;
        private readonly IScreeningTemplateValueQueryRepository _screeningTemplateValueQueryRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IGSCContext _context;
        private readonly IStudyVersionStatusRepository _studyVersionStatusRepository;
        private readonly IRandomizationRepository _randomizationRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public VersionEffectRepository(IGSCContext context,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IStudyVersionStatusRepository studyVersionStatusRepository,
            IRandomizationRepository randomizationRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IJwtTokenAccesser jwtTokenAccesser
        ) : base(context)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _context = context;
            _screeningVisitRepository = screeningVisitRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _studyVersionStatusRepository = studyVersionStatusRepository;
            _randomizationRepository = randomizationRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
        }

        public void ApplyNewVersion(int projectDesignId, bool isTrial, double versionNumber)
        {
            var patientStatusIds = _studyVersionStatusRepository.All.Where(x => x.StudyVerion.ProjectDesignId == projectDesignId
            && x.StudyVerion.VersionNumber == versionNumber).Select(t => t.PatientStatusId).ToList();

            if (patientStatusIds.Count > 0)
            {
                VisitProcess(projectDesignId, isTrial, versionNumber, patientStatusIds);
                TemplateProcess(projectDesignId, isTrial, versionNumber, patientStatusIds);
                VariableProcess(projectDesignId, isTrial, versionNumber, patientStatusIds);
                ScreeningEntryProcess(projectDesignId, isTrial, versionNumber, patientStatusIds);
            }

        }

        void ScreeningEntryProcess(int projectDesignId, bool isTrial, double versionNumber, List<ScreeningPatientStatus> patientStatuses)
        {
            var screeningEntrys = All.Where(t => t.DeletedDate == null &&
            t.Project.IsTestSite == isTrial && t.ProjectDesignId == projectDesignId &&
             patientStatuses.Contains(t.Randomization.PatientStatusId)).ToList();

            screeningEntrys.ForEach(screeningEntry =>
            {
                screeningEntry.StudyVersion = versionNumber;
                Update(screeningEntry);

            });

            _context.Save();
            _context.DetachAllEntities();

        }


        void VisitProcess(int projectDesignId, bool isTrial, double versionNumber, List<ScreeningPatientStatus> patientStatuses)
        {
            var deletedVisitIds = _projectDesignVisitRepository.All.Where(t => t.ProjectDesignPeriod.ProjectDesignId == projectDesignId
            && t.DeletedDate == null && t.InActiveVersion != null && t.InActiveVersion <= versionNumber).Select(t => t.Id).ToList();

            var screeningVisit = _screeningVisitRepository.All.Where(t => t.DeletedDate == null &&
            t.ScreeningEntry.Project.IsTestSite == isTrial && t.ScreeningEntry.ProjectDesignId == projectDesignId &&
             patientStatuses.Contains(t.ScreeningEntry.Randomization.PatientStatusId) &&
             deletedVisitIds.Contains(t.ProjectDesignVisitId)).
             Include(a => a.ScreeningTemplates).
             ThenInclude(a => a.ScreeningTemplateValues).ToList();

            screeningVisit.ForEach(x =>
            {
                x.ScreeningTemplates.ForEach(a =>
                {
                    _screeningTemplateRepository.Delete(a.Id);

                    a.ScreeningTemplateValues.ToList().ForEach(c =>
                    {
                        _screeningTemplateValueRepository.Delete(c.Id);
                    });

                });
                _screeningVisitRepository.Delete(x.Id);
            });

            _context.Save();
            _context.DetachAllEntities();

            var addVisits = _projectDesignVisitRepository.All.Where(t => t.ProjectDesignPeriod.ProjectDesignId == projectDesignId
              && t.DeletedDate == null && t.StudyVersion != null && t.StudyVersion <= versionNumber).Select(t => new
              {
                  t.Id,
                  t.IsSchedule,
                  Templates = t.Templates.Where(r => r.DeletedDate == null).Select(b => b.Id).ToList()
              }).ToList();

            var addVisitIds = addVisits.Select(t => t.Id).ToList();

           var screeningEntrys = All.Include(r => r.Randomization).Where(x => x.DeletedDate == null && x.ProjectDesignId == projectDesignId &&
                patientStatuses.Contains(x.Randomization.PatientStatusId) &&
                x.Project.IsTestSite == isTrial && x.ScreeningVisit.Any(t => t.DeletedDate == null && !addVisitIds.Contains(t.ProjectDesignVisitId))).ToList();


            screeningEntrys.ForEach(x =>
            {
                addVisits.ForEach(t =>
                {
                    var screeningVisit = new ScreeningVisit
                    {
                        ProjectDesignVisitId = t.Id,
                        ScreeningEntryId = x.Id,
                        Status = ScreeningVisitStatus.NotStarted,
                        IsSchedule = t.IsSchedule ?? false,
                        ScreeningTemplates = new List<ScreeningTemplate>()
                    };

                    t.Templates.ForEach(t =>
                    {
                        var screeningTemplate = new ScreeningTemplate
                        {
                            ProjectDesignTemplateId = t,
                            ScreeningVisit = screeningVisit,
                            Status = ScreeningTemplateStatus.Pending
                        };
                        _screeningTemplateRepository.Add(screeningTemplate);
                    });
                    _screeningVisitRepository.Add(screeningVisit);
                });

                if (x.Randomization != null && x.Randomization.PatientStatusId == ScreeningPatientStatus.Completed)
                {
                    x.Randomization.PatientStatusId = ScreeningPatientStatus.Screening;
                    _randomizationRepository.Update(x.Randomization);
                }
            });

            _context.Save();
            _context.DetachAllEntities();

        }


        void TemplateProcess(int projectDesignId, bool isTrial, double versionNumber, List<ScreeningPatientStatus> patientStatuses)
        {
            var deletedTemplatedIds = _projectDesignTemplateRepository.All.Where(t => t.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId
            && t.DeletedDate == null && t.InActiveVersion != null && t.InActiveVersion <= versionNumber).Select(t => t.Id).ToList();

            var screeningTemplates = _screeningTemplateRepository.All.AsNoTracking().
                Include(t => t.ScreeningTemplateValues).Where(x => x.DeletedDate == null && x.ScreeningVisit.ScreeningEntry.ProjectDesignId == projectDesignId &&
                patientStatuses.Contains(x.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId) &&
                x.ScreeningVisit.ScreeningEntry.Project.IsTestSite == isTrial && deletedTemplatedIds.Contains(x.ProjectDesignTemplateId)).ToList();

            screeningTemplates.ForEach(x =>
            {
                _screeningTemplateRepository.Delete(x.Id);
                x.ScreeningTemplateValues.ToList().ForEach(c =>
                {
                    _screeningTemplateValueRepository.Delete(c.Id);
                });

            });
            _context.Save();
            _context.DetachAllEntities();


            var addTemplateIds = _projectDesignTemplateRepository.All.AsNoTracking().Where(t => t.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId
              && t.DeletedDate == null && t.StudyVersion != null && t.StudyVersion <= versionNumber).Select(t => t.Id).ToList();


            var screeningVisits = _screeningVisitRepository.All.AsNoTracking().Where(x => x.DeletedDate == null && x.ScreeningEntry.ProjectDesignId == projectDesignId &&
            patientStatuses.Contains(x.ScreeningEntry.Randomization.PatientStatusId) &&
            x.ScreeningEntry.Project.IsTestSite == isTrial && x.ScreeningTemplates.Any(t => t.DeletedDate == null && !addTemplateIds.Contains(t.ProjectDesignTemplateId))).ToList();


            foreach (var x in screeningVisits)
            {
                foreach (var projectDesignTemplateId in addTemplateIds)
                {
                    var screeningTemplate = new ScreeningTemplate
                    {
                        ProjectDesignTemplateId = projectDesignTemplateId,
                        ScreeningVisitId = x.Id,
                        Id = 0,
                        Status = ScreeningTemplateStatus.Pending
                    };
                    _screeningTemplateRepository.Add(screeningTemplate);
                }

                if (x.Status == ScreeningVisitStatus.Completed)
                {
                    x.Status = ScreeningVisitStatus.InProgress;
                    _screeningVisitRepository.Update(x);
                }

            }

            _context.Save();
            _context.DetachAllEntities();

        }


        void VariableProcess(int projectDesignId, bool isTrial, double versionNumber, List<ScreeningPatientStatus> patientStatuses)
        {
            var deletedVariableIds = _projectDesignVariableRepository.All.Where(t => t.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId
            && t.DeletedDate == null && t.InActiveVersion != null && t.InActiveVersion <= versionNumber).Select(t => t.Id).ToList();

            var variables = _screeningTemplateValueRepository.All.Where(x => x.DeletedDate == null &&
            x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.ProjectDesignId == projectDesignId &&
                patientStatuses.Contains(x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId) &&
                x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.IsTestSite == isTrial && deletedVariableIds.Contains(x.ProjectDesignVariableId)).ToList();

            variables.ForEach(x =>
            {
                _screeningTemplateValueRepository.Delete(x.Id);

            });
            _context.Save();
            _context.DetachAllEntities();


            var addVariableIds = _projectDesignVariableRepository.All.AsNoTracking().Where(t => t.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId
              && t.DeletedDate == null && t.StudyVersion != null && t.StudyVersion <= versionNumber).Select(t => t.Id).ToList();


            var screeningTemplates = _screeningTemplateRepository.All.AsNoTracking().Where(x => x.DeletedDate == null &&
             x.ScreeningVisit.ScreeningEntry.ProjectDesignId == projectDesignId && x.Status > ScreeningTemplateStatus.InProcess &&
             patientStatuses.Contains(x.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId) &&
             x.ScreeningVisit.ScreeningEntry.Project.IsTestSite == isTrial && x.ScreeningTemplateValues.Any(t => !addVariableIds.Contains(t.ProjectDesignVariableId))).ToList();

            addVariableIds.ForEach(v =>
            {

                screeningTemplates.ForEach(x =>
                {
                    if (!_screeningTemplateValueRepository.All.Any(r => r.ProjectDesignVariableId == v && r.ScreeningTemplateId == x.Id))
                    {
                        var screeningTemplateValue = new ScreeningTemplateValue
                        {
                            ScreeningTemplateId = x.Id,
                            ProjectDesignVariableId = v,
                            Value = null,
                            ReviewLevel = x.ReviewLevel ?? 1,
                            IsSystem = true,
                            QueryStatus = QueryStatus.Open
                        };
                        _screeningTemplateValueRepository.Add(screeningTemplateValue);

                        var valueQuery = new ScreeningTemplateValueQuery();
                        valueQuery.QueryStatus = QueryStatus.Open;
                        valueQuery.IsSystem = true;
                        valueQuery.ScreeningTemplateValue = screeningTemplateValue;
                        valueQuery.QueryLevel = screeningTemplateValue.ReviewLevel;
                        valueQuery.QueryStatus = QueryStatus.Open;
                        screeningTemplateValue.AcknowledgeLevel = -1;
                        screeningTemplateValue.UserRoleId = _jwtTokenAccesser.RoleId;
                        _screeningTemplateValueQueryRepository.Add(valueQuery);
                    }

                });
            });


            _context.Save();
            _context.DetachAllEntities();

        }
    }
}
