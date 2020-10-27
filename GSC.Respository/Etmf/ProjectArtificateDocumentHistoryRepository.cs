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
    public class ProjectArtificateDocumentHistoryRepository : GenericRespository<ProjectArtificateDocumentHistory, GscContext>, IProjectArtificateDocumentHistoryRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IUnitOfWork _uow;

        public ProjectArtificateDocumentHistoryRepository(IUnitOfWork<GscContext> uow,
           IJwtTokenAccesser jwtTokenAccesser, IMapper mapper)
           : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
            _mapper = mapper;
        }

        public void AddHistory(ProjectWorkplaceArtificatedocument projectWorkplaceArtificatedocument, int? ReviewId, int? ApproverId)
        {
            var ProjectArtificateDocumentHistory = new ProjectArtificateDocumentHistory();
            ProjectArtificateDocumentHistory.ProjectWorkplaceArtificateDocumentId = projectWorkplaceArtificatedocument.Id;
            ProjectArtificateDocumentHistory.DocumentName = projectWorkplaceArtificatedocument.DocumentName;
            ProjectArtificateDocumentHistory.ProjectArtificateDocumentReviewId = ReviewId;
            ProjectArtificateDocumentHistory.ProjectArtificateDocumentApproverId = ApproverId;

            Add(ProjectArtificateDocumentHistory);
            _uow.Save();
        }
    }
}
