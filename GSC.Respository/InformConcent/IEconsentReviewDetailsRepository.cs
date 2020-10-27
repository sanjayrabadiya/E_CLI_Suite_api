using GSC.Common.GenericRespository;
using GSC.Data.Dto.Etmf;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.InformConcent
{
    public interface IEconsentReviewDetailsRepository : IGenericRepository<EconsentReviewDetails>
    {
        string Duplicate(EconsentReviewDetailsDto objSave);
        IList<DropDownDto> GetPatientDropdown(int projectid);
        List<EconsentReviewDetailsDto> GetUnApprovedEconsentDocumentList(int patientid);
        List<EconsentReviewDetailsDto> GetApprovedEconsentDocumentList(int projectid);
        List<SectionsHeader> GetEconsentDocumentHeaders();
        List<SectionsHeader> GetEconsentDocumentHeadersByDocumentId(int documentId);
        string ImportSectionData(int id, int sectionno);
        string GetEconsentDocument(EconsentReviewDetailsDto econsentreviewdetails);
        List<DashboardDto> GetEconsentMyTaskList(int ProjectId);
        void downloadpdf(CustomParameter param);
    }
}
