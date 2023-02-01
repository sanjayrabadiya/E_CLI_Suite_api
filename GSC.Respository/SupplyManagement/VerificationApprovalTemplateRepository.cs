using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class VerificationApprovalTemplateRepository : GenericRespository<VerificationApprovalTemplate>, IVerificationApprovalTemplateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IVerificationApprovalTemplateValueRepository _verificationApprovalTemplateValueRepository;
        private readonly IEmailSenderRespository _emailSenderRespository;
        private readonly IGSCContext _context;
        public VerificationApprovalTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IVerificationApprovalTemplateValueRepository verificationApprovalTemplateValueRepository,
        IMapper mapper, IEmailSenderRespository emailSenderRespository)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _verificationApprovalTemplateValueRepository = verificationApprovalTemplateValueRepository;
            _mapper = mapper;
            _emailSenderRespository = emailSenderRespository;
            _context = context;
        }



        public DesignVerificationApprovalTemplateDto GetVerificationApprovalTemplate(DesignVerificationApprovalTemplateDto designTemplateDto, int ProductVerificationDetailId)
        {
            int verificationApprovalTemplateId = All.Where(x => x.ProductVerificationDetailId == ProductVerificationDetailId && x.DeletedDate == null).FirstOrDefault().Id;

            var verificationApprovalTemplateBasic = GetScreeningTemplateBasic(verificationApprovalTemplateId);

            designTemplateDto.VerificationApprovalTemplateId = verificationApprovalTemplateId;
            //        designTemplateDto.VerificationApprovalTemplateId = verificationApprovalTemplateBasic.Id;
            var values = GetVerificationValues(verificationApprovalTemplateBasic.Id);

            values.ForEach(t =>
            {
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.StudyLevelFormVariableId == t.StudyLevelFormVariableId);
                if (variable != null)
                {
                    variable.VariableValue = t.Value;
                    variable.VariableValueOld = t.IsNa ? "N/A" : t.Value;
                    variable.VerificationApprovalTemplateValueId = t.Id;
                    variable.IsNaValue = t.IsNa;
                    if (!string.IsNullOrWhiteSpace(variable.VariableValue) || variable.IsNaValue)
                        variable.IsValid = true;
                    if (variable.Values != null && (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox))
                        variable.Values.ToList().ForEach(val =>
                        {
                            var childValue = t.Children.FirstOrDefault(v => v.StudyLevelFormVariableValueId == val.Id);
                            if (childValue != null)
                            {
                                variable.IsValid = true;
                                val.VariableValue = childValue.Value;
                                val.VariableValueOld = childValue.Value;
                                val.VerificationApprovalTemplateValueChildId = childValue.Id;
                            }
                        });
                }
            });

            return designTemplateDto;
        }

        private VerificationApprovalTemplateBasic GetScreeningTemplateBasic(int verificationApprovalTemplateId)
        {
            return All.Include(x => x.StudyLevelForm).ThenInclude(x => x.VariableTemplate).Where(r => r.Id == verificationApprovalTemplateId).Select(
               c => new VerificationApprovalTemplateBasic
               {
                   Id = c.Id,
                   VariableTemplateId = c.StudyLevelForm.VariableTemplateId,
                   StudyLevelFormId = c.StudyLevelFormId,
               }).FirstOrDefault();
        }

        private List<VerificationApprovalTemplateValueBasic> GetVerificationValues(int verificationApprovalTemplateId)
        {
            return _verificationApprovalTemplateValueRepository.All.AsNoTracking().Where(t => t.VerificationApprovalTemplateId == verificationApprovalTemplateId)
                    .ProjectTo<VerificationApprovalTemplateValueBasic>(_mapper.ConfigurationProvider).ToList();
        }
        public void SendForApprovalEmail(VerificationApprovalTemplateDto verificationApprovalTemplateDto, ProductReceipt productReceipt)
        {
            IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
            var emailconfig = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == verificationApprovalTemplateDto.ProjectId && x.Triggers == SupplyManagementEmailTriggers.SendforApprovalVerificationTemplate).FirstOrDefault();
            if (emailconfig != null)
            {
                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).Select(x => x.Users.Email).ToList();
                if (details.Count() > 0)
                {
                    iWRSEmailModel.ProductType = productReceipt.ProductName;
                    iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == verificationApprovalTemplateDto.ProjectId).FirstOrDefault().ProjectCode;
                    iWRSEmailModel.ActionBy = _jwtTokenAccesser.UserName;
                    _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details, emailconfig);
                }
            }
        }
    }
}
