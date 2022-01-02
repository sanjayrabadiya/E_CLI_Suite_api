using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.Workflow;
using GSC.Respository.ProjectRight;
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
        public VersionEffectRepository(IGSCContext context,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectRightRepository projectRightRepository,
            IProjectWorkflowRepository projectWorkflowRepository,
            IProjectDesignVisitRepository projectDesignVisitRepository,
            IScreeningTemplateRepository screeningTemplateRepository,
            IProjectDesignPeriodRepository projectDesignPeriodRepository,
            IScreeningVisitRepository screeningVisitRepository,
            IScreeningTemplateValueQueryRepository screeningTemplateValueQueryRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
            IStudyVersionStatusRepository studyVersionStatusRepository,
            IRandomizationRepository randomizationRepository
        ) : base(context)
        {
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _screeningTemplateValueQueryRepository = screeningTemplateValueQueryRepository;
            _projectDesignVisitRepository = projectDesignVisitRepository;
            _context = context;
            _screeningVisitRepository = screeningVisitRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _studyVersionStatusRepository = studyVersionStatusRepository;
            _randomizationRepository = randomizationRepository;
        }

        public void ApplyNewVersion(int projectDesignId, bool isTrial, double versionNumber)
        {
            //var visits = _projectDesignVisitRepository.All.Where(t => t.ProjectDesignPeriod.ProjectDesignId == projectDesignId
            //&& t.DeletedDate == null && ((t.StudyVersion != null && t.StudyVersion <= versionNumber) || (t.InActiveVersion != null && t.InActiveVersion <= versionNumber)));

            //var entries = All.Where(x => x.ProjectDesignId == projectDesignId && x.Project.IsTestSite == isTrial).ToList();

            var patientStatusIds = _studyVersionStatusRepository.All.Where(x => x.StudyVerion.ProjectDesignId == projectDesignId
            && x.StudyVerion.VersionNumber == versionNumber).Select(t => t.PatientStatusId).ToList();

            VisitProcess(projectDesignId, isTrial, versionNumber, patientStatusIds);
            TemplateProcess(projectDesignId, isTrial, versionNumber, patientStatusIds);
        }


        void VisitProcess(int projectDesignId, bool isTrial, double versionNumber, List<ScreeningPatientStatus> patientStatuses)
        {
            var deletedVisitIds = _projectDesignVisitRepository.All.Where(t => t.ProjectDesignPeriod.ProjectDesignId == projectDesignId
            && t.DeletedDate == null && t.InActiveVersion != null && t.InActiveVersion <= versionNumber).Select(t => t.Id).ToList();

            var screeningVisits = _screeningVisitRepository.All.AsNoTracking().Include(t => t.ScreeningEntry).
                Include(t => t.ScreeningTemplates).
                ThenInclude(t => t.ScreeningTemplateValues).Where(x => x.DeletedDate == null && x.ScreeningEntry.ProjectDesignId == projectDesignId &&
                patientStatuses.Contains(x.ScreeningEntry.Randomization.PatientStatusId) &&
                x.ScreeningEntry.Project.IsTestSite == isTrial && deletedVisitIds.Contains(x.ProjectDesignVisitId)).ToList();

            screeningVisits.ForEach(x =>
            {
                if (x.ScreeningEntry != null)
                {
                    x.ScreeningEntry.StudyVersion = versionNumber;
                    Update(x.ScreeningEntry);
                }
                x.ScreeningTemplates.ForEach(a =>
                {
                    _screeningTemplateRepository.Delete(a);

                    a.ScreeningTemplateValues.ToList().ForEach(c =>
                    {
                        _screeningTemplateValueRepository.Delete(c);
                    });

                });
                x.ScreeningEntry = null;
                _screeningVisitRepository.Delete(x);

            });
            _context.SaveChangesAsync().Wait();
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
           x.Project.IsTestSite == isTrial && x.ScreeningVisit.Any(t => t.DeletedDate == null && !addVisitIds.Contains(t.ProjectDesignVisitId))).ToList();

            addVisits.ForEach(t =>
            {
                screeningEntrys.ForEach(x =>
                {
                    var screeningVisit = new ScreeningVisit
                    {
                        ProjectDesignVisitId = t.Id,
                        Status = ScreeningVisitStatus.NotStarted,
                        IsSchedule = t.IsSchedule ?? false,
                        ScreeningTemplates = new List<ScreeningTemplate>()
                    };

                    t.Templates.ForEach(t =>
                    {
                        var screeningTemplate = new ScreeningTemplate
                        {
                            ProjectDesignTemplateId = t,
                            Status = ScreeningTemplateStatus.Pending
                        };
                        _screeningTemplateRepository.Add(screeningTemplate);
                        screeningVisit.ScreeningTemplates.Add(screeningTemplate);

                    });

                    x.StudyVersion = versionNumber;
                    Update(x);

                });


            });

            _context.SaveChangesAsync().Wait();
            _context.DetachAllEntities();

            screeningEntrys.Where(t =>  t.Randomization != null && t.Randomization.PatientStatusId == ScreeningPatientStatus.Completed).ToList().ForEach(x =>
            {
                x.Randomization.PatientStatusId = ScreeningPatientStatus.Screening;
                _randomizationRepository.Update(x.Randomization);
            });


            _context.SaveChangesAsync().Wait();
        }


        void TemplateProcess(int projectDesignId, bool isTrial, double versionNumber, List<ScreeningPatientStatus> patientStatuses)
        {
            var deletedTemplatedIds = _projectDesignTemplateRepository.All.Where(t => t.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId
            && t.DeletedDate == null && t.InActiveVersion != null && t.InActiveVersion <= versionNumber).Select(t => t.Id).ToList();

            var screeningTemplates = _screeningTemplateRepository.All.AsNoTracking().Include(t => t.ScreeningVisit).
                ThenInclude(t => t.ScreeningEntry).
                Include(t => t.ScreeningTemplateValues).Where(x => x.DeletedDate == null && x.ScreeningVisit.ScreeningEntry.ProjectDesignId == projectDesignId &&
                patientStatuses.Contains(x.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId) &&
                x.ScreeningVisit.ScreeningEntry.Project.IsTestSite == isTrial && deletedTemplatedIds.Contains(x.ProjectDesignTemplateId)).ToList();

            screeningTemplates.ForEach(x =>
            {
                if (x.ScreeningVisit != null && x.ScreeningVisit.ScreeningEntry != null)
                {
                    x.ScreeningVisit.ScreeningEntry.StudyVersion = versionNumber;
                    Update(x.ScreeningVisit.ScreeningEntry);
                }

                _screeningTemplateRepository.Delete(x);
                x.ScreeningTemplateValues.ToList().ForEach(c =>
                {
                    _screeningTemplateValueRepository.Delete(c);
                });

            });
            _context.SaveChangesAsync().Wait();
            _context.DetachAllEntities();


            var addTemplateIds = _projectDesignTemplateRepository.All.Where(t => t.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesignId == projectDesignId
              && t.DeletedDate == null && t.StudyVersion != null && t.StudyVersion <= versionNumber).Select(t => t.Id).ToList();


            var screeningVisits = _screeningVisitRepository.All.Where(x => x.DeletedDate == null && x.ScreeningEntry.ProjectDesignId == projectDesignId &&
            x.ScreeningEntry.Project.IsTestSite == isTrial && x.ScreeningTemplates.Any(t => t.DeletedDate == null && !addTemplateIds.Contains(t.ProjectDesignTemplateId))).ToList();

            addTemplateIds.ForEach(t =>
            {
                screeningVisits.ForEach(x =>
                {
                    var screeningTemplate = new ScreeningTemplate
                    {
                        ProjectDesignTemplateId = t,
                        ScreeningVisitId = x.Id,
                        Status = ScreeningTemplateStatus.Pending
                    };
                    _screeningTemplateRepository.Add(screeningTemplate);

                });


            });

            _context.SaveChangesAsync().Wait();
            _context.DetachAllEntities();

            screeningVisits.Where(t => t.Status == ScreeningVisitStatus.Completed).ToList().ForEach(x =>
           {
               x.Status = ScreeningVisitStatus.InProgress;
               _screeningVisitRepository.Update(x);
           });


            _context.SaveChangesAsync().Wait();
        }
    }
}
