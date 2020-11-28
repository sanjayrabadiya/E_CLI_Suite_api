using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.UserMgt;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectArtificateDocumentCommentRepository : GenericRespository<ProjectArtificateDocumentComment>, IProjectArtificateDocumentCommentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUserRepository _userRepository;
        private readonly IGSCContext _context;
        public ProjectArtificateDocumentCommentRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository,
           IUserRepository userRepository)
           : base(context)
        {
            _context = context;
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _userRepository = userRepository;
        }

        public IList<ProjectArtificateDocumentCommentDto> GetComments(int documentId)
        {
            var comments = All.Where(x => x.ProjectWorkplaceArtificatedDocumentId == documentId
                                          && x.DeletedDate == null)
                .Select(t => new ProjectArtificateDocumentCommentDto
                {
                    Id = t.Id,
                    ProjectWorkplaceArtificatedDocumentId = t.ProjectWorkplaceArtificatedDocumentId,
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
