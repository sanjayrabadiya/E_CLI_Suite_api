using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.GeneralConfig
{
    public class SendEmailOnVariableValueRepository : GenericRespository<SendEmailOnVariableValue>, ISendEmailOnVariableValueRepository
    {
        public SendEmailOnVariableValueRepository(IGSCContext context) : base(context)
        {
        }
        public void test() { }
    }
}
