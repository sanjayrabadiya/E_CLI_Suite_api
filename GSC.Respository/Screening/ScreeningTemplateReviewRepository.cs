using System;
using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Report;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateReviewRepository : GenericRespository<ScreeningTemplateReview>,
        IScreeningTemplateReviewRepository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ScreeningTemplateReviewRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
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

        public void Save(int screeningTemplateId, ScreeningTemplateStatus status, short reviewLevel)
        {
            var screeningTemplateReview = new ScreeningTemplateReview();
            screeningTemplateReview.ScreeningTemplateId = screeningTemplateId;
            screeningTemplateReview.Status = status;
            screeningTemplateReview.ReviewLevel = reviewLevel;
            screeningTemplateReview.RoleId = _jwtTokenAccesser.RoleId;
            Add(screeningTemplateReview);
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


        public void RollbackReview(RollbackReviewTemplateDto rollbackReviewTemplateDto)
        {
            var templates = All.Where(x => x.DeletedDate == null && rollbackReviewTemplateDto.ScreeningTemplateIds.Contains(x.ScreeningTemplateId)
            && x.Status >= rollbackReviewTemplateDto.Status).ToList();
            templates.ForEach(x =>
            {
                x.IsRepeat = true;
                Update(x);
            });

            var screeningTemplates = _context.ScreeningTemplate.Where(x => x.DeletedDate == null && rollbackReviewTemplateDto.ScreeningTemplateIds.Contains(x.Id) && x.Status >= rollbackReviewTemplateDto.Status).ToList();
            screeningTemplates.ForEach(x =>
            {
                x.Status = rollbackReviewTemplateDto.Status;

                if (rollbackReviewTemplateDto.Status == ScreeningTemplateStatus.InProcess || rollbackReviewTemplateDto.Status == ScreeningTemplateStatus.Pending)
                    x.ReviewLevel = null;

                if (rollbackReviewTemplateDto.Status == ScreeningTemplateStatus.Submitted || rollbackReviewTemplateDto.Status == ScreeningTemplateStatus.Reviewed)
                    x.ReviewLevel = 1;

                _context.ScreeningTemplate.Update(x);
            });


        }
        // added for dynamic column 04/06/2023
        // Comment by vipul on 10/06/2024
        //public string ReviewerName(int LevelNo, int screeningtemplateId)
        //{
        //    var result = All.Where(s => s.ScreeningTemplateId == screeningtemplateId && !s.IsRepeat && s.ReviewLevel == LevelNo).Select(x => new { x.CreatedBy }).FirstOrDefault();
        //    if (result != null)
        //        return _context.Users.Find(result).UserName;
        //    return "";
        //}

        public List<ReviewDto> SetReviewHistory(List<ReviewDto> filters)
        {
            var result = new List<ReviewDto>();
            foreach (var item in filters)
            {
                item.WorkFlowReviewList = item.WorkFlowReviewList.Select(x => new WorkFlowReview
                {
                    ReviewerRole = x.ReviewerRole,
                    LevelNo = x.LevelNo,
                    ReviewerName = item.ScreeningTemplateReview.Where(s => s.ScreeningTemplateId == item.ScreeningTemplateId && !s.IsRepeat && s.ReviewLevel == x.LevelNo).Select(x => _context.Users.Find(x.CreatedBy).UserName).FirstOrDefault(),
                    ReviewedDate = item.ScreeningTemplateReview.Where(s => s.ScreeningTemplateId == item.ScreeningTemplateId && !s.IsRepeat && s.ReviewLevel == x.LevelNo).Select(x => x.CreatedDate).FirstOrDefault() ?? null
                }).ToList();

                result.Add(item);
            }
            return result;
        }
    }
}