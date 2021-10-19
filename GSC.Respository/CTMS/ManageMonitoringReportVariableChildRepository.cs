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

namespace GSC.Respository.SupplyManagement
{
    public class ManageMonitoringReportVariableChildRepository : GenericRespository<ManageMonitoringReportVariableChild>, IManageMonitoringReportVariableChildRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        public ManageMonitoringReportVariableChildRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
        }
        public void Save(ManageMonitoringReportVariable manageMonitoringReportVariable)
        {
            if (manageMonitoringReportVariable.Children != null)
            {
                manageMonitoringReportVariable.Children.ForEach(x =>
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
