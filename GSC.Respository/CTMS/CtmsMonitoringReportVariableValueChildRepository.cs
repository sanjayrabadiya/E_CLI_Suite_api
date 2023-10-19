using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportVariableValueChildRepository : GenericRespository<CtmsMonitoringReportVariableValueChild>, ICtmsMonitoringReportVariableValueChildRepository
    {

        public CtmsMonitoringReportVariableValueChildRepository(IGSCContext context)
            : base(context)
        {}
        public void Save(CtmsMonitoringReportVariableValue ctmsMonitoringReportVariableValue)
        {
            if (ctmsMonitoringReportVariableValue.Children != null)
            {
                ctmsMonitoringReportVariableValue.Children.ForEach(x =>
                {
                    if (x.Id == 0)
                        Add(x);
                    else
                        Update(x);
                });
            }
        }
    }
}
