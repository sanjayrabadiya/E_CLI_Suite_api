using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class ProjectDesignReportSettingRepository : GenericRespository<ProjectDesignReportSetting, GscContext>, IProjectDesignReportSettingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly GscContext _context;
         
        public ProjectDesignReportSettingRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser
            ) : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = uow.Context;
           
        }

         
    }
}
