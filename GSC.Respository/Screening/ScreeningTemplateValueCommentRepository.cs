﻿using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueCommentRepository :
        GenericRespository<ScreeningTemplateValueComment>, IScreeningTemplateValueCommentRepository
    {
        public ScreeningTemplateValueCommentRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }

        public IList<ScreeningTemplateValueCommentDto> GetComments(int screeningTemplateValueId)
        {
            var comments = All.Where(x => x.ScreeningTemplateValueId == screeningTemplateValueId
                                          && x.DeletedDate == null)
                .Select(t => new ScreeningTemplateValueCommentDto
                {
                    Comment = t.Comment,
                    CreatedDate = t.CreatedDate,
                    CreatedByName = t.CreatedByUser.UserName,
                    RoleName = t.Role.RoleName
                }).ToList();

            return comments;
        }
    }
}