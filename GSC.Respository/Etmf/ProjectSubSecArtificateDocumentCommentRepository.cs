using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Entities.Etmf;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.UserMgt;
using GSC.Shared;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Etmf
{
    public class ProjectSubSecArtificateDocumentCommentRepository : GenericRespository<ProjectSubSecArtificateDocumentComment>, IProjectSubSecArtificateDocumentCommentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public ProjectSubSecArtificateDocumentCommentRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser)
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public IList<ProjectSubSecArtificateDocumentCommentDto> GetComments(int documentId)
        {
            var comments = All.Where(x => x.ProjectWorkplaceSubSecArtificateDocumentId == documentId
                                          && x.DeletedDate == null)
                .Select(t => new ProjectSubSecArtificateDocumentCommentDto
                {
                    Id = t.Id,
                    ProjectWorkplaceSubSecArtificateDocumentId = t.ProjectWorkplaceSubSecArtificateDocumentId,
                    Comment = t.Comment,
                    CreatedDate = t.CreatedDate,
                    CreatedByName = t.CreatedByUser.UserName,
                    Response = t.Response,
                    ResponseByName = !string.IsNullOrEmpty(t.Response) ? _context.Users.Where(x => x.Id == t.ResponseBy).FirstOrDefault().UserName : "",
                    ResponseBy = t.ResponseBy,
                    ResponseDate = t.ResponseDate,
                    ViewDelete = t.CreatedBy == _jwtTokenAccesser.UserId && string.IsNullOrEmpty(t.Response),
                    ViewClose = t.CreatedBy == _jwtTokenAccesser.UserId && !string.IsNullOrEmpty(t.Response),
                    IsClose = t.IsClose
                }).ToList();

            return comments;
        }

    }
}
