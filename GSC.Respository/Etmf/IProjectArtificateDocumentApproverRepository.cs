﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Dto.Etmf;
using System;
using System.Collections.Generic;
using System.Text;
using GSC.Data.Dto.Master;

namespace GSC.Respository.Etmf
{
    public interface IProjectArtificateDocumentApproverRepository : IGenericRepository<ProjectArtificateDocumentApprover>
    {
        List<ProjectArtificateDocumentReviewDto> UserNameForApproval(int Id, int ProjectId, int ProjectDetailsId);
        void SendMailForApprover(ProjectArtificateDocumentApproverDto ProjectArtificateDocumentApproverDto);
        List<DashboardDto> GetEtmfMyTaskList(int ProjectId);
        List<ProjectArtificateDocumentApproverHistory> GetArtificateDocumentApproverHistory(int Id);
        void IsApproveDocument(int Id);
        bool GetApprovePending(int documentId);
    }
}
