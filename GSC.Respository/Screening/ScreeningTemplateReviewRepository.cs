using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateReviewRepository : GenericRespository<ScreeningTemplateReview>,
        IScreeningTemplateReviewRepository
    {
        private readonly IGSCContext _context;
        public ScreeningTemplateReviewRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
        }

        public List<ScreeningTemplateReviewDto> GetTemplateReviewHistory(int id)
        {
            return All.Where(x => x.DeletedDate == null && x.ScreeningTemplateId == id).Select(r =>
                new ScreeningTemplateReviewDto
                {
                    Date = r.CreatedDate,
                    ReviewLevel = r.ReviewLevel,
                    Status = r.Status.ToString(),
                    OfficerName = r.CreatedByUser.UserName,
                    RoleName = r.Role.RoleName
                }).ToList();
        }

        public IList<ReviewDto> GetReviewLevel(int projectId)
        {
            var ParentProjectId = _context.Project.Where(x => x.Id == projectId).FirstOrDefault().ParentProjectId ?? projectId;
            var ProjectDesignId = _context.ProjectDesign.Where(x => x.ProjectId == ParentProjectId).FirstOrDefault().Id;

            var reviewdto = (from workflow in _context.ProjectWorkflow.Where(t => t.ProjectDesignId == ProjectDesignId)
                             join workflowlevel in _context.ProjectWorkflowLevel.Where(x => x.DeletedDate == null) on workflow.Id equals workflowlevel.ProjectWorkflowId
                             group workflowlevel by new
                             {
                                 ReviewLevel = workflowlevel.LevelNo,
                                 // Value = "Reviewed " + workflowlevel.LevelNo.ToString()
                                 Value = workflowlevel.SecurityRole.RoleShortName
                             }
                             into level
                             select new ReviewDto
                             {
                                 ReviewLevel = level.Key.ReviewLevel,
                                 Value = level.Key.Value
                             }).ToList();

            return reviewdto.OrderBy(x => x.ReviewLevel).ToList();
        }
    }
}