using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignVisitRepository : GenericRespository<ProjectDesignVisit>,
        IProjectDesignVisitRepository
    {

        private readonly IGSCContext _context;
        private readonly IStudyVersionRepository _studyVersionRepository;
        public ProjectDesignVisitRepository(IGSCContext context, IStudyVersionRepository studyVersionRepository) : base(context)
        {
            _context = context;
            _studyVersionRepository = studyVersionRepository;
        }

        public ProjectDesignVisit GetVisit(int id)
        {
            var visit = _context.ProjectDesignVisit.Where(t => t.Id == id)
                 .Include(d => d.VisitLanguage.Where(x => x.DeletedBy == null))
                .Include(d => d.Templates)
                 .ThenInclude(d => d.TemplateLanguage.Where(x => x.DeletedBy == null))
                .Include(d => d.Templates)
                  .ThenInclude(d => d.ProjectDesignTemplateNote.Where(x => x.DeletedBy == null))
                .ThenInclude(d => d.TemplateNoteLanguage.Where(x => x.DeletedBy == null))
                 .Include(d => d.Templates)
                .ThenInclude(d => d.Variables)
                .ThenInclude(d => d.VariableLanguage.Where(x => x.DeletedBy == null))
                  .Include(d => d.Templates)
                .ThenInclude(d => d.Variables)
                  .ThenInclude(d => d.VariableNoteLanguage.Where(x => x.DeletedBy == null))
                   .Include(d => d.Templates)
                .ThenInclude(d => d.Variables)
                .ThenInclude(d => d.Values.OrderBy(c => c.SeqNo))
                .ThenInclude(d => d.VariableValueLanguage.Where(x => x.DeletedBy == null))
               .Include(d => d.Templates)
                .ThenInclude(d => d.Variables)
                .ThenInclude(d => d.Roles)
                //.Include(d=>d.Templates)
                //.ThenInclude(d=>d.Variables)
                //.ThenInclude(d=>d.Remarks)
                .AsNoTracking().FirstOrDefault();

            return visit;
        }

        public IList<DropDownDto> GetVisitsByProjectDesignId(int projectDesignId)
        {
            var periods = _context.ProjectDesignPeriod.Where(x => x.DeletedDate == null
                                                                 && x.ProjectDesignId == projectDesignId)
                .Include(t => t.VisitList).ToList();

            var visits = new List<DropDownDto>();
            periods.ForEach(period =>
            {
                period.VisitList.Where(x => x.DeletedDate == null).OrderBy(x => x.DesignOrder).ToList().ForEach(visit =>
                  {
                      visits.Add(new DropDownDto
                      {
                          Id = visit.Id,
                          Value = visit.DisplayName + " (" + period.DisplayName + ")"
                      });
                  });
            });

            return visits;
        }

        public IList<DropDownDto> GetVisitDropDown(int projectDesignPeriodId)
        {
            var visits = All.Where(x => x.DeletedDate == null
                                        && x.ProjectDesignPeriodId == projectDesignPeriodId).OrderBy(t => t.DesignOrder).Select(
                t => new DropDownDto
                {
                    Id = t.Id,
                    Value = t.DisplayName,
                    Code = t.StudyVersion != null || t.InActiveVersion != null ?
                    "( V : " + t.StudyVersion + (t.StudyVersion != null && t.InActiveVersion != null ? " - " : "" + t.InActiveVersion) + ")" : "",
                    ExtraData = t.IsNonCRF,
                }).ToList();

            return visits;
        }


        public IList<ProjectDesignVisitDto> GetVisitList(int projectDesignPeriodId)
        {
            var checkVersion = CheckStudyVersion(projectDesignPeriodId);

            var visits = All.Where(x => x.DeletedDate == null
                                        && x.ProjectDesignPeriodId == projectDesignPeriodId).OrderBy(t => t.DesignOrder).Select(
                t => new ProjectDesignVisitDto
                {
                    Id = t.Id,
                    DisplayName = t.DisplayName,
                    DisplayVersion = t.StudyVersion != null || t.InActiveVersion != null ?
                    "( V : " + t.StudyVersion + (t.StudyVersion != null && t.InActiveVersion != null ? " - " : "") + t.InActiveVersion + ")" : "",
                    IsNonCRF = t.IsNonCRF,
                    StudyVersion = t.StudyVersion,
                    InActiveVersion = t.InActiveVersion,
                    InActive = t.InActiveVersion != null,
                    AllowActive = checkVersion.VersionNumber == t.InActiveVersion && t.InActiveVersion != null
                }).ToList();

            return visits;
        }


        public List<ProjectDesignVisitBasicDto> GetVisitAndTemplateByPeriordId(int projectDesignPeriodId)
        {
            return All.Where(x => x.DeletedDate == null && x.DeletedDate == null && x.ProjectDesignPeriodId == projectDesignPeriodId)
                .Select(t => new ProjectDesignVisitBasicDto
                {
                    Id = t.Id,
                    IsRepeated = t.IsRepeated,
                    IsSchedule = t.IsSchedule,
                    StudyVersion = t.StudyVersion,
                    Templates = t.Templates.Where(a => a.DeletedDate == null).Select(b => new InsertScreeningTemplate { ProjectDesignTemplateId = b.Id, StudyVersion = b.StudyVersion }).ToList()
                }).ToList();

        }

        public string Duplicate(ProjectDesignVisit objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.DisplayName == objSave.DisplayName &&
                x.ProjectDesignPeriodId == objSave.ProjectDesignPeriodId && x.DeletedDate == null))
                return "Duplicate Visit Name : " + objSave.DisplayName;
            return "";
        }

        public CheckVersionDto CheckStudyVersion(int projectDesignPeriodId)
        {
            var result = new CheckVersionDto();
            var projectDesignId = All.Where(x => x.ProjectDesignPeriodId == projectDesignPeriodId).Select(t => t.ProjectDesignPeriod.ProjectDesignId).FirstOrDefault();
            result.AnyLive = _studyVersionRepository.AnyLive(projectDesignId);
            if (result.AnyLive)
                result.VersionNumber = _studyVersionRepository.GetOnTrialVersionByProjectDesign(projectDesignId);
            return result;
        }
    }
}