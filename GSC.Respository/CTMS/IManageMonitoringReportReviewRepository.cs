﻿using GSC.Common.GenericRespository;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Entities.Etmf;
using GSC.Data.Dto.Etmf;
using System;
using System.Collections.Generic;
using System.Text;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;
using GSC.Data.Dto.CTMS;

namespace GSC.Respository.CTMS
{
    public interface IManageMonitoringReportReviewRepository : IGenericRepository<ManageMonitoringReportReview>
    {
        List<ManageMonitoringReportReviewDto> UserRoles(int Id, int ProjectId);
        void SaveTemplateReview(List<ManageMonitoringReportReviewDto> manageMonitoringReportReviewDto);
        void SendMailToReviewer(ManageMonitoringReportReviewDto ReviewDto);
        void SendMailToSendBack(ManageMonitoringReportReview ReviewDto);
        //List<ProjectArtificateDocumentReviewHistory> GetArtificateDocumentHistory(int Id);
        List<DashboardDto> GetSendTemplateList(int ProjectId);
        List<DashboardDto> GetSendBackTemplateList(int ProjectId);

    }
}