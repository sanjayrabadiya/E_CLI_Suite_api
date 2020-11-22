using System;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.LogReport;
using GSC.Domain.Context;
using GSC.Shared;

namespace GSC.Respository.LogReport
{
    public class UserLoginReportRespository : GenericRespository<UserLoginReport, GscContext>,
        IUserLoginReportRespository
    {
        private readonly IUnitOfWork<GscContext> _uow;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public UserLoginReportRespository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _uow = uow;
        }


        public int SaveLog(string msg, int? userId, string userName)
        {
            var userLoginReport = new UserLoginReport();
            userLoginReport.UserId = userId;
            userLoginReport.LoginName = userName;
            userLoginReport.Note = msg;
            userLoginReport.LoginTime = DateTime.Now;
            userLoginReport.IpAddress = _jwtTokenAccesser.IpAddress;
            Add(userLoginReport);
            _uow.Save();
            return userLoginReport.Id;
        }
    }
}