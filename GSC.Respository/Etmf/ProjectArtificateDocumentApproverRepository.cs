using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.UserMgt;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Etmf
{
    public class ProjectArtificateDocumentApproverRepository : GenericRespository<ProjectArtificateDocumentApprover, GscContext>, IProjectArtificateDocumentApproverRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;
        public ProjectArtificateDocumentApproverRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
           : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
            _mapper = mapper;
        }

        public List<ProjectArtificateDocumentReviewDto> UserNameForApproval(int Id)
        {
            var users = Context.Users.Where(x => x.DeletedDate == null && x.Id != _jwtTokenAccesser.UserId).Select(c => new ProjectArtificateDocumentReviewDto
            {
                UserId = c.Id,
                Name = c.UserName,
                IsSelected = All.Any(b => b.ProjectWorkplaceArtificatedDocumentId == Id && b.UserId == c.Id && b.DeletedDate == null),
            }).Where(x => x.IsSelected == false).ToList();

            return users;
        }
    }
}
