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
    public class ProjectArtificateDocumentHistoryRepository : GenericRespository<ProjectArtificateDocumentHistory>, IProjectArtificateDocumentHistoryRepository
    {
        private readonly IGSCContext _context;

        public ProjectArtificateDocumentHistoryRepository(IGSCContext context)
           : base(context)
        {
            _context = context;
        }

        public void AddHistory(ProjectWorkplaceArtificatedocument projectWorkplaceArtificatedocument, int? ReviewId, int? ApproverId)
        {
            var ProjectArtificateDocumentHistory = new ProjectArtificateDocumentHistory();
            ProjectArtificateDocumentHistory.ProjectWorkplaceArtificateDocumentId = projectWorkplaceArtificatedocument.Id;
            ProjectArtificateDocumentHistory.DocumentName = projectWorkplaceArtificatedocument.DocumentName;
            ProjectArtificateDocumentHistory.ProjectArtificateDocumentReviewId = ReviewId;
            ProjectArtificateDocumentHistory.ProjectArtificateDocumentApproverId = ApproverId;
            ProjectArtificateDocumentHistory.ExpiryDate = projectWorkplaceArtificatedocument.ExpiryDate;

            Add(ProjectArtificateDocumentHistory);
            _context.Save();
        }
    }
}
