using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningVisitRepository : GenericRespository<ScreeningVisit, GscContext>, IScreeningVisitRepository
    {
        public ScreeningVisitRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }


        public void VisitRepeat(int projectDesignVisitId, int screeningEntryId)
        {
            var repeatedCount = 0;
            var projectVisit = All.Include(r => r.ScreeningTemplates).Where(x => x.ProjectDesignVisitId == projectDesignVisitId
                                              && x.ScreeningEntryId == screeningEntryId && x.ParentId == null).FirstOrDefault();

            //if (projectVisit.Count > 0)
            //    repeatedCount = projectVisit.Max(x => x.RepeatedVisit ?? 0);

            //var templates = Context.ProjectDesignTemplate
            //    .Where(t => t.DeletedDate == null && t.ProjectDesignVisitId == projectDesignVisitId).ToList();
            //templates.ForEach(t =>
            //{
            //    var oldTemplate = Context.ScreeningTemplate.FirstOrDefault(r =>
            //        r.ScreeningVisit.ScreeningEntryId == screeningEntryId &&
            //        r.ScreeningVisitId == projectDesignVisitId && r.ProjectDesignTemplateId == t.Id);
            //    Add(new ScreeningTemplate
            //    {
            //        ScreeningEntryId = screeningEntryId,
            //        ProjectDesignTemplateId = t.Id,
            //        EditCheckDetailId = oldTemplate != null ? oldTemplate.EditCheckDetailId : null,
            //        RepeatedVisit = repeatedCount + 1,
            //        ScreeningVisitId = t.ProjectDesignVisitId,
            //        IsEditChecked = false,
            //        IsDisable = false,
            //        Status = ScreeningStatus.Pending
            //    });
            //});
        }
    }
}
