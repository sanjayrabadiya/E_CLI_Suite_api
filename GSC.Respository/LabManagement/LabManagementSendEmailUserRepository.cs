using GSC.Common.GenericRespository;
using GSC.Data.Entities.LabManagement;
using GSC.Domain.Context;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.LabManagement
{
    public class LabManagementSendEmailUserRepository : GenericRespository<LabManagementSendEmailUser>, ILabManagementSendEmailUserRepository
    {
        private readonly IGSCContext _context;

        public LabManagementSendEmailUserRepository(IGSCContext context
            )
           : base(context)
        {
            _context = context;
        }
    }
}
