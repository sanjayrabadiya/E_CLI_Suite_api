using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Project.Generalconfig;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Project.GeneralConfig
{
    public class SendEmailOnVariableChangeSettingRepository : GenericRespository<SendEmailOnVariableChangeSetting>, ISendEmailOnVariableChangeSettingRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        public SendEmailOnVariableChangeSettingRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser, IMapper mapper) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
        }

        public List<SendEmailOnVariableChangeSettingGridDto> GetList(int ProjectDesignId)
        {
            return All.Where(x => x.ProjectDesignVariable.ProjectDesignTemplate.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Id == ProjectDesignId).
   ProjectTo<SendEmailOnVariableChangeSettingGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

        }

        public string Duplicate(SendEmailOnVariableChangeSetting objSave)
        {
            if (All.Any(x => x.Id != objSave.Id && x.ProjectDesignVariableId == objSave.ProjectDesignVariableId && x.DeletedDate == null))
                return "Variable already set for send email.";

            return "";
        }


    }
}
