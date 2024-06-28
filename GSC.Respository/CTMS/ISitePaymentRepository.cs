using System.Collections.Generic;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.CTMS;

namespace GSC.Respository.Master
{
    public interface ISitePaymentRepository : IGenericRepository<SitePayment>
    {
        IList<SitePaymentGridDto> GetSitePaymentList(bool isDeleted, int studyId, int siteId);
        List<DropDownDto> GetVisitDropDown(int parentProjectId, int siteId);
        List<decimal> GetVisitAmount(int parentProjectId, int siteId, int visitId);
        List<DropDownDto> GetPassThroughCostActivity(int projectId, int siteId);
        List<decimal> GetPassthroughTotalAmount(int parentProjectId, int siteId, int passThroughCostActivityId);
    }
}
