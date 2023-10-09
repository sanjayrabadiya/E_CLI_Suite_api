using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Helper;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentReviewDetailsRepository : IGenericRepository<EconsentReviewDetails>
    {
        List<EConsentDocumentHeader> GetEconsentDocumentHeaders();
        List<SectionsHeader> GetEconsentSectionHeaders(int id);
        List<SectionsHeader> GetEconsentDocumentHeadersByDocumentId(int documentId);
        string ImportSectionData(int id, int sectionno);
        FileStreamResult GetEconsentDocument(int EconcentReviewId);
        List<DashboardDto> GetEconsentMyTaskList(int ProjectId);
        CustomParameter downloadpdf(int id);
        List<EconsentReviewDetailsDto> GetEconsentReviewDetailsForSubjectManagement(int patientid);
        List<EconsentDocumentDetailsDto> GetEconsentReviewDetailsForPatientDashboard();
        int UpdateDocument(EconsentReviewDetailsDto econsentReviewDetailsDto);
        int ApproveRejectEconsentDocument(EconsentReviewDetailsDto econsentReviewDetailsDto);
        int ApproveWithDrawPatient(EconsentDocumetViwerDto econsentReviewDetailsDto, bool isWithdraw);
        AppEConsentSection ImportSectionDataHtml(int id, int sectionno);
    }
}
