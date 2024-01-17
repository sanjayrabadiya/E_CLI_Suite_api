using AutoMapper;
using AutoMapper.QueryableExtensions;
using Azure.Core;
using DocumentFormat.OpenXml.Office2016.Excel;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Project.StudyLevelFormSetup;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.EmailSender;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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
                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                if (details.Count() > 0)
                {
                    if (productReceipt != null)
                    {
                        var pharmacyStudyProductType = _context.PharmacyStudyProductType.Where(x => x.Id == productReceipt.PharmacyStudyProductTypeId).FirstOrDefault();
                        if (pharmacyStudyProductType != null)
                        {
                            var product = _context.ProductType.Where(x => x.Id == pharmacyStudyProductType.ProductTypeId).FirstOrDefault();
                            if (product != null)
                                iWRSEmailModel.ProductType = product.ProductTypeCode;
                        }
                    }
                    iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == verificationApprovalTemplateDto.ProjectId).FirstOrDefault().ProjectCode;
                    iWRSEmailModel.ActionBy = _jwtTokenAccesser.UserName;
                    _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                    foreach (var item in details)
                    {
                        SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                        history.SupplyManagementEmailConfigurationDetailId = item.Id;
                        _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);
                        _context.Save();
                    }
                }
            }
        }

        public async Task SendForApprovalVerificationTemplateScheduleEmail()
        {
            int? projectId = 0;
            int? recordId = 0;
            string recurenceType = string.Empty;
            try
            {
                var templatedata = All.Include(s => s.ProductVerificationDetail).ThenInclude(s => s.ProductReceipt)
                    .Where(s => s.DeletedDate == null && s.ProductVerificationDetail.ProductReceipt.Status == ProductVerificationStatus.SentForApproval).ToList();
                if (templatedata != null && templatedata.Count > 0)
                {
                    foreach (var item in templatedata)
                    {
                        VerificationApprovalTemplate supplyManagementRequest = new VerificationApprovalTemplate();
                        IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
                        SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();
                        var emailconfig = await _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == item.ProductVerificationDetail.ProductReceipt.ProjectId && x.Triggers == SupplyManagementEmailTriggers.SendforApprovalVerificationTemplate).FirstOrDefaultAsync();
                        if (emailconfig != null)
                        {
                            supplyManagementEmailScheduleLog.ProjectId = item.ProductVerificationDetail.ProductReceipt.ProjectId;
                            supplyManagementEmailScheduleLog.TriggerType = emailconfig.Triggers.GetDescription();
                            supplyManagementEmailScheduleLog.RecurrenceType = emailconfig.RecurrenceType.GetDescription();
                            supplyManagementEmailScheduleLog.Message = "Send for approval template Schedule Start " + DateTime.Now;
                            supplyManagementEmailScheduleLog.RecordId = item.Id;
                            _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                            _context.Save();

                            projectId = item.ProductVerificationDetail.ProductReceipt.ProjectId;
                            recurenceType = emailconfig.RecurrenceType.GetDescription();
                            recordId = item.Id;

                            if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Daily)
                            {
                                supplyManagementRequest = item;
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.AlternateDay)
                            {
                                DateTime start = Convert.ToDateTime(item.CreatedDate);
                                DateTime end = DateTime.Now;
                                TimeSpan span = end.Date - start.Date;
                                double difference = span.TotalDays;
                                if (difference % 2 == 0)
                                    supplyManagementRequest = item;
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Weekly)
                            {
                                DateTime start = Convert.ToDateTime(item.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddDays(7);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = item;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.FifteenDays)
                            {
                                DateTime start = Convert.ToDateTime(item.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddDays(15);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = item;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Monthly)
                            {
                                DateTime start = Convert.ToDateTime(item.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddMonths(1);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = item;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.EveryTwoMonth)
                            {
                                DateTime start = Convert.ToDateTime(item.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddMonths(2);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = item;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Quarterly)
                            {
                                DateTime start = Convert.ToDateTime(item.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddMonths(3);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = item;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.EverySixMonth)
                            {
                                DateTime start = Convert.ToDateTime(item.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddMonths(6);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = item;
                                        start = end;
                                    }
                                }
                            }
                            else if (emailconfig.RecurrenceType == SupplyManagementEmailRecurrenceType.Yearly)
                            {
                                DateTime start = Convert.ToDateTime(item.CreatedDate);
                                DateTime end = DateTime.Now.Date;
                                while (start < end)
                                {
                                    start = start.AddYears(1);
                                    if (start.Date == end.Date)
                                    {
                                        supplyManagementRequest = item;
                                        start = end;
                                    }
                                }
                            }
                            if (supplyManagementRequest != null)
                            {
                                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                                if (details.Count() > 0)
                                {
                                    if (item.ProductVerificationDetail.ProductReceipt != null)
                                    {
                                        var pharmacyStudyProductType = _context.PharmacyStudyProductType.Where(x => x.Id == item.ProductVerificationDetail.ProductReceipt.PharmacyStudyProductTypeId).FirstOrDefault();
                                        if (pharmacyStudyProductType != null)
                                        {
                                            var product = _context.ProductType.Where(x => x.Id == pharmacyStudyProductType.ProductTypeId).FirstOrDefault();
                                            if (product != null)
                                                iWRSEmailModel.ProductType = product.ProductTypeCode;
                                        }
                                    }
                                    iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == item.ProductVerificationDetail.ProductReceipt.ProjectId).FirstOrDefault().ProjectCode;
                                    iWRSEmailModel.ActionBy = _jwtTokenAccesser.UserName;
                                    _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                                    foreach (var item1 in details)
                                    {
                                        SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                                        history.SupplyManagementEmailConfigurationDetailId = item1.Id;
                                        _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);

                                    }
                                }
                            }

                            var supplyManagementEmailScheduleLog1 = new SupplyManagementEmailScheduleLog();
                            supplyManagementEmailScheduleLog1.ProjectId = item.ProductVerificationDetail.ProductReceipt.ProjectId;
                            supplyManagementEmailScheduleLog1.TriggerType = emailconfig.Triggers.GetDescription();
                            supplyManagementEmailScheduleLog1.RecurrenceType = emailconfig.RecurrenceType.GetDescription();
                            supplyManagementEmailScheduleLog1.Message = "Send for approval template Schedule End " + DateTime.Now;
                            supplyManagementEmailScheduleLog1.RecordId = item.Id;
                            _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog1);
                            _context.Save();
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                SupplyManagementEmailScheduleLog supplyManagementEmailScheduleLog = new SupplyManagementEmailScheduleLog();
                supplyManagementEmailScheduleLog.Message = ex.Message.ToString();
                supplyManagementEmailScheduleLog.TriggerType = SupplyManagementEmailTriggers.SendforApprovalVerificationTemplate.GetDescription();
                supplyManagementEmailScheduleLog.ProjectId = projectId;
                supplyManagementEmailScheduleLog.RecurrenceType = recurenceType;
                supplyManagementEmailScheduleLog.RecordId = recordId;
                _context.SupplyManagementEmailScheduleLog.Add(supplyManagementEmailScheduleLog);
                _context.Save();
            }
        }
        public void SendTemplateApproveRejectEmail(VerificationApprovalTemplateDto verificationApprovalTemplateDto, ProductReceipt productReceipt)
        {
            IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();

            if (productReceipt != null)
            {
                var emailconfig = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == productReceipt.ProjectId && x.Triggers == SupplyManagementEmailTriggers.VerificationTemplateApproveReject).FirstOrDefault();
                if (emailconfig != null)
                {
                    var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                    if (details.Count() > 0)
                    {
                        if (productReceipt != null)
                        {
                            var pharmacyStudyProductType = _context.PharmacyStudyProductType.Where(x => x.Id == productReceipt.PharmacyStudyProductTypeId).FirstOrDefault();
                            if (pharmacyStudyProductType != null)
                            {
                                var product = _context.ProductType.Where(x => x.Id == pharmacyStudyProductType.ProductTypeId).FirstOrDefault();
                                if (product != null)
                                    iWRSEmailModel.ProductType = product.ProductTypeCode;
                            }
                        }

                        iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == productReceipt.ProjectId).FirstOrDefault().ProjectCode;
                        iWRSEmailModel.ActionBy = _jwtTokenAccesser.UserName;
                        iWRSEmailModel.Status = verificationApprovalTemplateDto.IsApprove ? "Approved" : "Rejected";
                        if (verificationApprovalTemplateDto.VerificationApprovalTemplateHistory != null)
                        {
                            var auditreason = _context.AuditReason.Where(x => x.Id == verificationApprovalTemplateDto.VerificationApprovalTemplateHistory.AuditReasonId).FirstOrDefault();
                            iWRSEmailModel.Reason = auditreason != null ? auditreason.ReasonName : "";

                        }

                        _emailSenderRespository.SendforApprovalEmailIWRS(iWRSEmailModel, details.Select(x => x.Users.Email).Distinct().ToList(), emailconfig);
                        foreach (var item in details)
                        {
                            SupplyManagementEmailConfigurationDetailHistory history = new SupplyManagementEmailConfigurationDetailHistory();
                            history.SupplyManagementEmailConfigurationDetailId = item.Id;
                            _context.SupplyManagementEmailConfigurationDetailHistory.Add(history);
                            _context.Save();
                        }
                    }
                }
            }

        }
    }
}
