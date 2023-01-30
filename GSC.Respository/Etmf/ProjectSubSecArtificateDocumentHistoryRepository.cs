using AutoMapper;
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
    public class ProjectSubSecArtificateDocumentHistoryRepository : GenericRespository<ProjectSubSecArtificateDocumentHistory>, IProjectSubSecArtificateDocumentHistoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;

        public ProjectSubSecArtificateDocumentHistoryRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
        }

        public void AddHistory(ProjectWorkplaceSubSecArtificatedocument projectWorkplaceSubSecArtificatedocument, int? ReviewId, int? ApproverId)
        {
            var ProjectSubSecArtificateDocumentHistory = new ProjectSubSecArtificateDocumentHistory();
            ProjectSubSecArtificateDocumentHistory.ProjectWorkplaceSubSecArtificateDocumentId = projectWorkplaceSubSecArtificatedocument.Id;
            ProjectSubSecArtificateDocumentHistory.DocumentName = projectWorkplaceSubSecArtificatedocument.DocumentName;
            ProjectSubSecArtificateDocumentHistory.ProjectSubSecArtificateDocumentReviewId = ReviewId;
            ProjectSubSecArtificateDocumentHistory.ProjectSubSecArtificateDocumentApproverId = ApproverId;
            ProjectSubSecArtificateDocumentHistory.ExpiryDate = projectWorkplaceSubSecArtificatedocument.ExpiryDate;

            Add(ProjectSubSecArtificateDocumentHistory);
             _context.Save();
        }
    }
}
