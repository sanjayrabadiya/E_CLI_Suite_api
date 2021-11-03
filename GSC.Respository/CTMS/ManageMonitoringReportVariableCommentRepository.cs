using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.CTMS
{
    public class ManageMonitoringReportVariableCommentRepository : GenericRespository<ManageMonitoringReportVariableComment>, IManageMonitoringReportVariableCommentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;


        public ManageMonitoringReportVariableCommentRepository(IGSCContext context,
           IJwtTokenAccesser jwtTokenAccesser
        )
           : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }
        public IList<ManageMonitoringReportVariableCommentDto> GetComments(int manageMonitoringReportVariableId)
        {
            var comments = All.Where(x => x.ManageMonitoringReportVariableId == manageMonitoringReportVariableId
                                          && x.DeletedDate == null)
                .Select(t => new ManageMonitoringReportVariableCommentDto
                {
                    Comment = t.Comment,
                    CreatedDate = t.CreatedDate,
                    CreatedByName = t.CreatedByUser.UserName,
                    RoleName = t.Role.RoleName
                }).ToList();

            return comments;
        }
    }
}