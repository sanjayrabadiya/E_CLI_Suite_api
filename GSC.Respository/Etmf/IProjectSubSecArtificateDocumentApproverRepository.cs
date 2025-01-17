﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace GSC.Respository.Etmf
{
    public interface IProjectSubSecArtificateDocumentApproverRepository : IGenericRepository<ProjectSubSecArtificateDocumentApprover>
    {
        List<ProjectSubSecArtificateDocumentApproverHistory> GetArtificateDocumentApproverHistory(int Id);
        List<ProjectSubSecArtificateDocumentReviewDto> UserNameForApproval(int Id, int ProjectId, int ProjectDetailsId);
        void SendMailForApprover(ProjectSubSecArtificateDocumentApproverDto ProjectSubSecArtificateDocumentApproverDto);
        List<DashboardDto> GetEtmfMyTaskList(int ProjectId);
        bool GetApprovePending(int documentId);
        List<ProjectSubSecArtificateDocumentReviewDto> GetUsers(int Id, int ProjectId);
        int ReplaceUser(int documentId, int actualUserId, int replaceUserId);
        void SendMailForApprovedRejected(ProjectSubSecArtificateDocumentApprover ProjectSubSecArtificateDocumentApproverDto);
        DateTime? GetMaxDueDate(int documentId);
        void SaveByDocumentIdInApprove(int projectWorkplaceArtificateDocumentId);
        int SkipDocumentApproval(int documentId, bool isApproval);
        Task SendDueApproveEmail();
    }
}