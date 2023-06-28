using AutoMapper;
using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Project.Workflow;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.Project.Generalconfig;
using GSC.Data.Entities.Project.Workflow;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Respository.EmailSender;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using Syncfusion.Licensing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.Project.Design
{
    public class VisitEmailConfigurationRepository : GenericRespository<VisitEmailConfiguration>, IVisitEmailConfigurationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        private readonly IMapper _mapper;
        private readonly IEmailSenderRespository _emailSenderRespository;
        public VisitEmailConfigurationRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IMapper mapper,
            IEmailSenderRespository emailSenderRespository) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
            _mapper = mapper;
            _emailSenderRespository = emailSenderRespository;
        }

        public List<VisitEmailConfigurationGridDto> GetVisitEmailConfigurationList(bool isDeleted, int projectDesignVisitId)
        {
            return All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectDesignVisitId == projectDesignVisitId).
                   ProjectTo<VisitEmailConfigurationGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
        }

        public string Duplicate(VisitEmailConfiguration objSave)
        {
            if (All.Any(x =>
                x.Id != objSave.Id && x.VisitStatusId == objSave.VisitStatusId && x.ProjectDesignVisitId == objSave.ProjectDesignVisitId && x.DeletedDate == null))
                return "Duplicate visit status.";

            return "";
        }

        public void SendEmailForVisitStatus(ScreeningVisit screeningVisit)
        {
            var result = All.Where(x => x.ProjectDesignVisitId == screeningVisit.ProjectDesignVisitId
            && x.VisitStatusId == screeningVisit.Status && x.DeletedDate == null).FirstOrDefault();

            if (result != null)
            {
                var emmailrole = _context.VisitEmailConfigurationRoles.Where(x => x.VisitEmailConfigurationId == result.Id && x.DeletedDate == null).Select(s => s.SecurityRoleId).ToList();

                if (emmailrole.Count > 0)
                {
                    var randomization = _context.Randomization.AsNoTracking()
                        .Where(x => x.ScreeningEntry.Id == screeningVisit.ScreeningEntryId).FirstOrDefault();

                    var roleusers = _context.ProjectRight.Include(s => s.User).Where(s => s.DeletedDate == null
                    && emmailrole.Contains(s.RoleId) && s.ProjectId == randomization.ProjectId).ToList();

                    var emaildata = new VisitEmailConfigurationGridDto();

                    emaildata.Subject = result.Subject;
                    emaildata.EmailBody = result.EmailBody;
                    emaildata.VisitName = screeningVisit.ScreeningVisitName;
                    emaildata.VisitStatus = screeningVisit.Status.ToString();

                    foreach (var item in roleusers)
                    {
                        _emailSenderRespository.SendEmailonVisitStatus(emaildata, item, randomization);

                    }
                }
            }
        }

    }
}