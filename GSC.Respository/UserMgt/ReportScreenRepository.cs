using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.UserMgt;
using GSC.Data.Entities.UserMgt;
using GSC.Domain.Context;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.UserMgt
{
    public class ReportScreenRepository : GenericRespository<ReportScreen>,
        IReportScreenRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public ReportScreenRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        public List<ReportScreenDto> GetReportScreen()
        {

            return All
                .Select(c => new ReportScreenDto { Id = c.Id, ReportCode = c.ReportCode, ReportName = c.ReportName, ReportGroup = c.ReportGroup, IsFavourite = false }).OrderBy(o => o.Id).ToList();
        }
    }
}
