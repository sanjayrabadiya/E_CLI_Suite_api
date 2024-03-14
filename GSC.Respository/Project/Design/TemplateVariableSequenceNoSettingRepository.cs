using AutoMapper;
using GSC.Common.GenericRespository;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Project.Design
{
    public class TemplateVariableSequenceNoSettingRepository : GenericRespository<TemplateVariableSequenceNoSetting>, ITemplateVariableSequenceNoSettingRepository
    {
        public TemplateVariableSequenceNoSettingRepository(IGSCContext context) : base(context)
        {
        }
    }
}
