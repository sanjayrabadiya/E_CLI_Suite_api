using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectWorkplaceArtificateDocumentReviewRepository : GenericRespository<ProjectArtificateDocumentReview, GscContext>, IProjectWorkplaceArtificateDocumentReviewRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IUploadSettingRepository _uploadSettingRepository;
        private readonly IUnitOfWork<GscContext> _uow;
        public ProjectWorkplaceArtificateDocumentReviewRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser, IUploadSettingRepository uploadSettingRepository)
           : base(uow, jwtTokenAccesser)
        {
            _uploadSettingRepository = uploadSettingRepository;
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
        }

        public List<ProjectArtificateDocumentReviewDto> UserRoles(int Id)
        {
            var roles = Context.SecurityRole.Where(x => x.DeletedDate == null).Select(c => new ProjectArtificateDocumentReviewDto
            {
                RoleId = c.Id,
                Name = c.RoleName,
                users = Context.UserRole.Where(a => a.UserRoleId == c.Id && a.User.DeletedDate == null
                                                                         && a.DeletedDate == null).Select(r =>
                    new ProjectArtificateDocumentReviewDto
                    {
                        RoleId = c.Id,
                        UserId = r.UserId,
                        Name = r.User.UserName,
                        IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == r.UserId && b.DeletedDate == null && b.IsSendBack == false)
                    }).Where(x => x.IsSelected == false).ToList()
            }).ToList();

            return roles;
        }

        public void SaveDocumentReview(List<ProjectArtificateDocumentReviewDto> pojectArtificateDocumentReviewDto)
        {
            var send = pojectArtificateDocumentReviewDto.SelectMany(x =>
                x.users.Select(c => new ProjectArtificateDocumentReviewDto
                { UserId = c.UserId, RoleId = c.RoleId, IsSelected = c.IsSelected, ProjectWorkplaceArtificatedDocumentId = x.ProjectWorkplaceArtificatedDocumentId })).Distinct().ToList();

            send = send.Distinct().ToList();

            var userlist = send.Select(c => new { c.UserId, c.IsSelected, c.RoleId, c.ProjectWorkplaceArtificatedDocumentId }).Distinct();
            foreach (var userDto in userlist)
                if (userDto.IsSelected)
                {
                    Add(new ProjectArtificateDocumentReview
                    {
                        ProjectWorkplaceArtificatedDocumentId = userDto.ProjectWorkplaceArtificatedDocumentId,
                        UserId = userDto.UserId,
                        RoleId = userDto.RoleId,
                        IsSendBack = false,
                    });
                }
        }

        public List<int> GetProjectArtificateDocumentReviewList()
        {
            return All.Where(c => c.DeletedDate == null && c.UserId == _jwtTokenAccesser.UserId
                                  //  && c.RoleId == _jwtTokenAccesser.RoleId
                                  ).Select(x => x.ProjectWorkplaceArtificatedDocumentId).ToList();
        }

        public void SaveByDocumentIdInReview(int projectWorkplaceArtificateDocumentId)
        {
            Add(new ProjectArtificateDocumentReview
            {
                ProjectWorkplaceArtificatedDocumentId = projectWorkplaceArtificateDocumentId,
                UserId = _jwtTokenAccesser.UserId,
                RoleId = _jwtTokenAccesser.RoleId
            });

            _uow.Save();
        }
    }
}
