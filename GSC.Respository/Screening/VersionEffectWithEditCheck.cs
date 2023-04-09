using GSC.Domain.Context;
using GSC.Respository.Project.Design;
using GSC.Respository.Project.EditCheck;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class VersionEffectWithEditCheck : IVersionEffectWithEditCheck
    {
        private readonly IGSCContext _context;
        private readonly IStudyVersionRepository _studyVersionRepository;
        private readonly IEditCheckDetailRepository _editCheckDetailRepository;
        private readonly IScreeningTemplateRepository _screeningTemplateRepository;
        private readonly IStudyVersionStatusRepository _studyVersionStatusRepository;
        public VersionEffectWithEditCheck(IGSCContext context, IStudyVersionRepository studyVersionRepository,
            IEditCheckDetailRepository editCheckDetailRepository, IScreeningTemplateRepository screeningTemplateRepository,
            IStudyVersionStatusRepository studyVersionStatusRepository)
        {
            _context = context;
            _studyVersionRepository = studyVersionRepository;
            _editCheckDetailRepository = editCheckDetailRepository;
            _screeningTemplateRepository = screeningTemplateRepository;
            _studyVersionStatusRepository = studyVersionStatusRepository;
        }

        public void ApplyEditCheck(int projectDesignId, bool isTrial, double versionNumber)
        {
            var createdDate = _studyVersionRepository.All.Where(x => x.ProjectDesignId == projectDesignId && x.VersionNumber == versionNumber).Select(t => t.CreatedDate).FirstOrDefault();

            var projectDesignTemplateIds = _editCheckDetailRepository.All.Where(x => x.EditCheck.ProjectDesignId == projectDesignId
            && (x.CreatedDate >= createdDate || x.ModifiedDate >= createdDate)).Select(r => r.ProjectDesignTemplateId).Distinct().ToList();

            var targetTemplateIds = _editCheckDetailRepository.All.Where(x => x.EditCheck.ProjectDesignId == projectDesignId
            && projectDesignTemplateIds.Contains(x.ProjectDesignTemplateId) && x.IsTarget).Select(r => r.ProjectDesignTemplateId).Distinct().ToList();


            if (targetTemplateIds.Count < 1)
                return;


            var patientStatusIds = _studyVersionStatusRepository.All.Where(x => x.StudyVerion.ProjectDesignId == projectDesignId
            && x.StudyVerion.VersionNumber == versionNumber).Select(t => t.PatientStatusId).ToList();

            var screeningTemplateIds = _screeningTemplateRepository.All.Where(x => x.DeletedDate == null && x.ScreeningVisit.ScreeningEntry.ProjectDesignId == projectDesignId &&
             patientStatusIds.Contains(x.ScreeningVisit.ScreeningEntry.Randomization.PatientStatusId) &&
             x.ScreeningVisit.ScreeningEntry.Project.IsTestSite == isTrial
             && (x.Status == Helper.ScreeningTemplateStatus.Submitted || x.Status == Helper.ScreeningTemplateStatus.Reviewed)).Select(t => t.Id).Distinct().ToList();


            if (screeningTemplateIds.Count < 1)
                return;


            foreach (var id in screeningTemplateIds)
            {
                _screeningTemplateRepository.SubmitReviewTemplate(id, true);
            }

        }
    }
}
