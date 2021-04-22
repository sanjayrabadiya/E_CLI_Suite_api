using System;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.LogReport;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.LogReport
{
    public class UserLoginReportRespository : GenericRespository<UserLoginReport>,
        IUserLoginReportRespository
    {
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public UserLoginReportRespository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }


        public int SaveLog(string msg, int? userId, string userName, int? roleId)
        {
            var userLoginReport = new UserLoginReport();
            userLoginReport.UserId = userId;
            userLoginReport.LoginName = userName;
            userLoginReport.SecurityRoleId = roleId;
            userLoginReport.Note = msg;
            userLoginReport.LoginTime = DateTime.Now;
            userLoginReport.IpAddress = _jwtTokenAccesser.IpAddress;
            Add(userLoginReport);
             _context.Save();
            return userLoginReport.Id;
        }
    }
}