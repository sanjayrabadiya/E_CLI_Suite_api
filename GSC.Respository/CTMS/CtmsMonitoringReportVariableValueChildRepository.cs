using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.CTMS;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportVariableValueChildRepository : GenericRespository<CtmsMonitoringReportVariableValueChild>, ICtmsMonitoringReportVariableValueChildRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public CtmsMonitoringReportVariableValueChildRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }
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
