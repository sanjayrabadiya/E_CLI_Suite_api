using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
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

        public VerificationApprovalTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IVerificationApprovalTemplateValueRepository verificationApprovalTemplateValueRepository,
        IMapper mapper)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _verificationApprovalTemplateValueRepository = verificationApprovalTemplateValueRepository;
            _mapper = mapper;
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
                var variable = designTemplateDto.Variables.FirstOrDefault(v => v.VariableId == t.VariableId);
                if (variable != null)
                {
                    variable.VerificationApprovalValue = t.Value;
                    variable.VerificationApprovalValueOld = t.IsNa ? "N/A" : t.Value;
                    variable.VerificationApprovalTemplateValueId = t.Id;

                    variable.IsNaValue = t.IsNa;

                    if (variable.Values != null && (variable.CollectionSource == CollectionSources.CheckBox || variable.CollectionSource == CollectionSources.MultiCheckBox))
                        variable.Values.ToList().ForEach(val =>
                        {
                            var childValue = t.Children.FirstOrDefault(v => v.VariableValueId == val.Id);
                            if (childValue != null)
                            {
                                val.VerificationApprovalValue = childValue.Value;
                                val.VerificationApprovalValueOld = childValue.Value;
                                val.VerificationApprovalTemplateValueChildId = childValue.Id;
                            }
                        });
                }
            });

            return designTemplateDto;
        }

        private VerificationApprovalTemplateBasic GetScreeningTemplateBasic(int verificationApprovalTemplateId)
        {
            return All.AsNoTracking().Where(r => r.Id == verificationApprovalTemplateId).Select(
               c => new VerificationApprovalTemplateBasic
               {
                   Id = c.Id,
                   ProductVerificationDetailId = c.ProductVerificationDetailId,
                   VariableTemplateId = c.VariableTemplateId,
               }).FirstOrDefault();
        }

        private List<VerificationApprovalTemplateValueBasic> GetVerificationValues(int verificationApprovalTemplateId)
        {
            return _verificationApprovalTemplateValueRepository.All.AsNoTracking().Where(t => t.VerificationApprovalTemplateId == verificationApprovalTemplateId)
                    .ProjectTo<VerificationApprovalTemplateValueBasic>(_mapper.ConfigurationProvider).ToList();
        }
    }
}
