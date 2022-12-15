using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.UserMgt;
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
        private readonly IVerificationApprovalTemplateHistoryRepository _verificationApprovalTemplateHistoryRepository;
        private readonly IVerificationApprovalTemplateValueChildRepository _verificationApprovalTemplateValueChildRepository;
        private readonly IVerificationApprovalTemplateValueAuditRepository _verificationApprovalTemplateValueAuditRepository;
        private readonly IProductReceiptRepository _productReceiptRepository;
        private readonly IUserRepository _userRepository;
        private readonly IProductVerificationDetailRepository _productVerificationDetail;
        public VerificationApprovalTemplateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IVerificationApprovalTemplateValueRepository verificationApprovalTemplateValueRepository,
            IMapper mapper, IVerificationApprovalTemplateHistoryRepository verificationApprovalTemplateHistoryRepository,
            IVerificationApprovalTemplateValueChildRepository verificationApprovalTemplateValueChildRepository,
            IVerificationApprovalTemplateValueAuditRepository verificationApprovalTemplateValueAuditRepository,
            IProductReceiptRepository productReceiptRepository, IUserRepository userRepository,
            IProductVerificationDetailRepository productVerificationDetail)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _verificationApprovalTemplateValueRepository = verificationApprovalTemplateValueRepository;
            _mapper = mapper;
            _verificationApprovalTemplateHistoryRepository = verificationApprovalTemplateHistoryRepository;
            _verificationApprovalTemplateValueChildRepository = verificationApprovalTemplateValueChildRepository;
            _verificationApprovalTemplateValueAuditRepository = verificationApprovalTemplateValueAuditRepository;
            _productReceiptRepository = productReceiptRepository;
            _userRepository = userRepository;
            _productVerificationDetail = productVerificationDetail;
        }



        public DesignVerificationApprovalTemplateDto GetVerificationApprovalTemplate(DesignVerificationApprovalTemplateDto designTemplateDto, int ProductVerificationDetailId)
        {
            int verificationApprovalTemplateId = All.Where(x => x.ProductVerificationDetailId == ProductVerificationDetailId && x.DeletedDate == null).FirstOrDefault().Id;

            var verificationApprovalTemplateBasic = GetScreeningTemplateBasic(verificationApprovalTemplateId);

            designTemplateDto.VerificationApprovalTemplateId = verificationApprovalTemplateId;

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

        public VerificationApprovalTemplate AddVerificationApprovalTemplateHistory(VerificationApprovalTemplate verificationApprovalTemplate, VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            verificationApprovalTemplate.VerificationApprovalTemplateHistory = new VerificationApprovalTemplateHistory();
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SendBy = _jwtTokenAccesser.UserId;
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SendOn = _jwtTokenAccesser.GetClientDate();
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.Status = Helper.ProductVerificationStatus.SentForApproval;
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SecurityRoleId = (int)verificationApprovalTemplateDto.SecurityRoleId;
            verificationApprovalTemplate.VerificationApprovalTemplateHistory.SendBySecurityRoleId = _jwtTokenAccesser.RoleId;
            _verificationApprovalTemplateHistoryRepository.Add(verificationApprovalTemplate.VerificationApprovalTemplateHistory);

            return verificationApprovalTemplate;
        }

        public void Addverificationhistory(VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            VerificationApprovalTemplateHistory history = new VerificationApprovalTemplateHistory();
            var detail = _verificationApprovalTemplateHistoryRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id).OrderByDescending(x => x.Id).LastOrDefault();
            history.VerificationApprovalTemplateId = detail.VerificationApprovalTemplateId;
            history.IsSendBack = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.IsSendBack;
            history.SendBy = _jwtTokenAccesser.UserId;
            history.SendOn = _jwtTokenAccesser.GetClientDate();
            _verificationApprovalTemplateHistoryRepository.Add(history);

        }

        public VerificationApprovalTemplateHistoryDto getverificationApprovalTemplateHistory(VerificationApprovalTemplateHistoryDto verificationApprovalTemplateDto)
        {
            var verification = _verificationApprovalTemplateHistoryRepository.Find(verificationApprovalTemplateDto.Id);
            verificationApprovalTemplateDto.Id = 0;
            verificationApprovalTemplateDto.IsSendBack = false;
            verificationApprovalTemplateDto.SendBy = _jwtTokenAccesser.UserId;
            verificationApprovalTemplateDto.SendOn = _jwtTokenAccesser.GetClientDate();
            verificationApprovalTemplateDto.SecurityRoleId = verification.SecurityRoleId;
            verificationApprovalTemplateDto.SendBySecurityRoleId = _jwtTokenAccesser.RoleId;
            verificationApprovalTemplateDto.VerificationApprovalTemplateId = verification.VerificationApprovalTemplateId;

            return verificationApprovalTemplateDto;
        }

        public void AddHistory(VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            VerificationApprovalTemplateHistory history = new VerificationApprovalTemplateHistory();
            if (!verificationApprovalTemplateDto.IsApprove)
            {
                var detail = _verificationApprovalTemplateHistoryRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id).OrderByDescending(x => x.Id).LastOrDefault();
                history.VerificationApprovalTemplateId = detail.VerificationApprovalTemplateId;
                history.IsSendBack = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.IsSendBack;
                history.AuditReasonId = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.AuditReasonId;
                history.ReasonOth = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.ReasonOth;
                history.Status = Helper.ProductVerificationStatus.Rejected;
                history.SendBy = _jwtTokenAccesser.UserId;
                history.SendOn = _jwtTokenAccesser.GetClientDate();
                history.SecurityRoleId = _jwtTokenAccesser.RoleId;
                _verificationApprovalTemplateHistoryRepository.Add(history);
            }
            else
            {
                var detail = _verificationApprovalTemplateHistoryRepository.All.Where(x => x.VerificationApprovalTemplateId == verificationApprovalTemplateDto.Id).OrderByDescending(x => x.Id).LastOrDefault();
                history.VerificationApprovalTemplateId = detail.VerificationApprovalTemplateId;
                history.IsSendBack = false;
                history.AuditReasonId = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.AuditReasonId;
                history.ReasonOth = verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.ReasonOth;
                history.Status = Helper.ProductVerificationStatus.Approved;
                history.SendBy = _jwtTokenAccesser.UserId;
                history.SendOn = _jwtTokenAccesser.GetClientDate();
                history.SecurityRoleId = _jwtTokenAccesser.RoleId;
                _verificationApprovalTemplateHistoryRepository.Add(history);
            }
        }

        public void Addvalues(VerificationApprovalTemplateDto verificationApprovalTemplateDto)
        {
            if (verificationApprovalTemplateDto.VerificationApprovalTemplateValueList != null)
            {
                foreach (var item in verificationApprovalTemplateDto.VerificationApprovalTemplateValueList)
                {
                    item.VerificationApprovalTemplateId = verificationApprovalTemplateDto.Id;
                    var value = _verificationApprovalTemplateValueRepository.GetValueForAudit(item);
                    var verificationApprovalTemplateValue = _mapper.Map<VerificationApprovalTemplateValue>(item);

                    var Exists = _verificationApprovalTemplateValueRepository.All.Where(x => x.DeletedDate == null && x.VerificationApprovalTemplateId == verificationApprovalTemplateValue.VerificationApprovalTemplateId && x.StudyLevelFormVariableId == item.StudyLevelFormVariableId).FirstOrDefault();

                    if (Exists == null)
                    {
                        verificationApprovalTemplateValue.Id = 0;
                        _verificationApprovalTemplateValueRepository.Add(verificationApprovalTemplateValue);

                        var aduit = new VerificationApprovalTemplateValueAudit
                        {
                            VerificationApprovalTemplateValue = verificationApprovalTemplateValue,
                            Value = value,
                            OldValue = item.OldValue,
                        };
                        _verificationApprovalTemplateValueAuditRepository.Save(aduit);
                        _verificationApprovalTemplateValueChildRepository.Save(verificationApprovalTemplateValue);
                    }
                    else
                    {
                        var aduit = new VerificationApprovalTemplateValueAudit
                        {
                            VerificationApprovalTemplateValueId = Exists.Id,
                            Value = value,
                            OldValue = item.OldValue,
                        };
                        _verificationApprovalTemplateValueAuditRepository.Save(aduit);
                        if (item.IsDeleted)
                            _verificationApprovalTemplateValueRepository.DeleteChild(Exists.Id);

                        _verificationApprovalTemplateValueChildRepository.Save(verificationApprovalTemplateValue);

                        verificationApprovalTemplateValue.Id = Exists.Id;
                        _verificationApprovalTemplateValueRepository.Update(verificationApprovalTemplateValue);
                    }
                }
            }
        }
        public void SendStatusApproval(VerificationApprovalTemplateDto verificationApprovalTemplateDto,int Id, VerificationApprovalTemplate verification)
        {
            var verificationDetail = _productVerificationDetail.Find(Id);
            var receipt = _productReceiptRepository.Find(verificationDetail.ProductReceiptId);
            if (receipt != null)
            {
                var email = _userRepository.All.Where(x=>x.Id == verification.CreatedBy).FirstOrDefault().Email;
                if (verificationApprovalTemplateDto.IsApprove)
                {
                    receipt.Status = Helper.ProductVerificationStatus.Approved;
                }
                else
                {
                    receipt.Status = Helper.ProductVerificationStatus.Rejected;

                }
                _productReceiptRepository.Update(receipt);
            }
        }
    }
}
