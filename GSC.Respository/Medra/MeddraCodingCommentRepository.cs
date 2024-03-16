using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MeddraCodingCommentRepository : GenericRespository<MeddraCodingComment>, IMeddraCodingCommentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public MeddraCodingCommentRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public MeddraCodingComment GetLatest(int MeddraCodingId)
        {
            return All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && x.MeddraCodingId == MeddraCodingId && x.CommentStatus != CommentStatus.SelfCorrection).OrderByDescending(o => o.Id).FirstOrDefault();
        }

        public MeddraCodingComment CheckWhileScopingVersionUpdate(int MeddraCodingId)
        {
            var data = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && x.MeddraCodingId == MeddraCodingId).OrderByDescending(o => o.Id).FirstOrDefault();
            if (data != null)
            {
                if (data.CommentStatus != CommentStatus.SelfCorrection && data.CommentStatus != CommentStatus.Closed)
                {
                    var meddraCodingComment = new MeddraCodingComment();
                    meddraCodingComment.UserRole = _jwtTokenAccesser.RoleId;
                    meddraCodingComment.MeddraCodingId = data.MeddraCodingId;
                    meddraCodingComment.Value = null;
                    meddraCodingComment.OldValue = null;
                    meddraCodingComment.OldPTCode = 0;
                    meddraCodingComment.NewPTCode = 0;
                    meddraCodingComment.CommentStatus = CommentStatus.Closed;
                    meddraCodingComment.Note = "Comment auto closed due to version update.";
                    meddraCodingComment.CreatedDate = DateTime.Now;
                    meddraCodingComment.CreatedBy = _jwtTokenAccesser.UserId;
                    Add(meddraCodingComment);
                }
            }
            return data;
        }

        public IList<MeddraCodingCommentDto> GetData(int MeddraCodingId)
        {
            var queryDtos =
                      (from query in _context.MeddraCodingComment.Where(t => t.MeddraCodingId == MeddraCodingId)
                       join reasonTemp in _context.AuditReason on query.ReasonId equals reasonTemp.Id into reasonDt
                       from reason in reasonDt.DefaultIfEmpty()
                       join userTemp in _context.Users on query.CreatedBy equals userTemp.Id into userDto
                       from user in userDto.DefaultIfEmpty()
                       join roleTemp in _context.SecurityRole on query.UserRole equals roleTemp.Id into roleDto
                       from role in roleDto.DefaultIfEmpty()
                       select new MeddraCodingCommentDto
                       {
                           Id = query.Id,
                           Value = query.Value,
                           ReasonName = reason.ReasonName,
                           ReasonOth = query.ReasonOth,
                           OldValue = query.OldValue,
                           Note = string.IsNullOrEmpty(query.Note) ? query.ReasonOth : query.Note,
                           CreatedDate = query.CreatedDate,
                           CreatedByName = role == null || string.IsNullOrEmpty(role.RoleName)
                               ? user.UserName
                               : user.UserName + "(" + role.RoleName + ")",
                           StatusName = query.CommentStatus.GetDescription(),
                           CommentStatus = query.CommentStatus,
                           OldPTCode = query.OldPTCode == 0 ? (long?)null : query.OldPTCode,
                           NewPTCode = query.NewPTCode == 0 ? (long?)null : query.NewPTCode
                       }).OrderByDescending(o => o.Id).ToList();

            var coding = _context.MeddraCoding.Where(t => t.Id == MeddraCodingId)
                        .Include(s => s.ScreeningTemplateValue).AsNoTracking().FirstOrDefault();
            ButtonShow button = new ButtonShow();
            var ProfileData = _context.StudyScoping.Where(t => t.DeletedDate == null && t.ProjectDesignVariableId == coding.ScreeningTemplateValue.ProjectDesignVariableId).FirstOrDefault();
            var commentLatest = All.Where(t => t.MeddraCodingId == MeddraCodingId).OrderByDescending(o => o.Id).FirstOrDefault();
            if (ProfileData != null)
            {
                if (ProfileData.CoderApprover == _jwtTokenAccesser.RoleId)
                {
                    if (commentLatest == null)
                    {
                        button.ShowCommentButton = true;
                        button.ShowRespondButton = false;
                    }
                    else
                    {
                        if (commentLatest.CommentStatus == CommentStatus.Open && _jwtTokenAccesser.RoleId == commentLatest.UserRole ||
                            commentLatest.CommentStatus == CommentStatus.Answered ||
                            commentLatest.CommentStatus == CommentStatus.Resolved)
                        {
                            button.ShowCommentButton = false;
                            button.ShowRespondButton = true;
                        }
                        else if (commentLatest.CommentStatus == CommentStatus.Open && _jwtTokenAccesser.RoleId != commentLatest.UserRole)
                        {
                            button.ShowCommentButton = false;
                            button.ShowRespondButton = false;
                        }
                        else if (commentLatest.CommentStatus == CommentStatus.SelfCorrection || commentLatest.CommentStatus == CommentStatus.Closed)
                        {
                            button.ShowCommentButton = true;
                            button.ShowRespondButton = false;
                        }
                        else
                        {
                            button.ShowCommentButton = false;
                            button.ShowRespondButton = false;
                        }
                    }
                }
                if (ProfileData.CoderProfile == _jwtTokenAccesser.RoleId)
                {
                    if (commentLatest == null)
                    {
                        button.ShowCommentButton = true;
                        button.ShowRespondButton = false;
                    }
                    else
                    {
                        if (commentLatest.CommentStatus == CommentStatus.Open && (_jwtTokenAccesser.RoleId == commentLatest.UserRole || _jwtTokenAccesser.RoleId != commentLatest.UserRole))
                        {
                            button.ShowCommentButton = false;
                            button.ShowRespondButton = true;
                        }
                        else if (commentLatest.CommentStatus == CommentStatus.SelfCorrection || commentLatest.CommentStatus == CommentStatus.Closed)
                        {
                            button.ShowCommentButton = true;
                            button.ShowRespondButton = false;
                        }
                        else if (commentLatest.CommentStatus == CommentStatus.Answered || commentLatest.CommentStatus == CommentStatus.Resolved)
                        {
                            button.ShowCommentButton = false;
                            button.ShowRespondButton = false;
                        }
                        else
                        {
                            button.ShowCommentButton = false;
                            button.ShowRespondButton = false;
                        }
                    }
                }
            }
            foreach (var item in queryDtos)
            {
                item.ShowButton = button;
            }
            
            return queryDtos;
        }
    }
}
