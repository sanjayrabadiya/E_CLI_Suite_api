using GSC.Common;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.CTMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GSC.Respository.CTMS
{
    public class CtmsStudyPlanTaskCommentRepository : GenericRespository<CtmsStudyPlanTaskComment>, ICtmsStudyPlanTaskCommentRepository
    {
        public CtmsStudyPlanTaskCommentRepository(IContext context) : base(context)
        {

        }
    }
}
