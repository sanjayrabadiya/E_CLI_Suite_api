using GSC.Common.GenericRespository;
using GSC.Data.Dto.InformConcent;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.InformConcent;
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
    }
}
