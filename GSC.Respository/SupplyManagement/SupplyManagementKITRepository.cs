using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Data;
using System.IO;
using System.Linq;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementKITRepository : GenericRespository<SupplyManagementKIT>, ISupplyManagementKITRepository
    {

        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IEmailSenderRespository _emailSenderRespository;
        public SupplyManagementKITRepository(IGSCContext context,
        IMapper mapper, IJwtTokenAccesser jwtTokenAccesser, IEmailSenderRespository emailSenderRespository)
            : base(context)
        {


            _mapper = mapper;
            _context = context;
            _jwtTokenAccesser = jwtTokenAccesser;
            _emailSenderRespository = emailSenderRespository;
        }

        public List<SupplyManagementKITGridDto> GetKITList(bool isDeleted, int ProjectId)
        {
            var data = _context.SupplyManagementKITDetail.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.SupplyManagementKIT.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementKITGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                if (x.RandomizationId > 0)
                    x.RandomizationNo = _context.Randomization.Where(z => z.Id == x.RandomizationId).FirstOrDefault().RandomizationNumber;
                if (x.SupplyManagementShipmentId > 0)
                {
                    var request = _context.SupplyManagementShipment.Include(r => r.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(z => z.Id == x.SupplyManagementShipmentId).FirstOrDefault();
                    if (request != null)
                    {
                        x.RequestFromSite = request.SupplyManagementRequest.FromProject.ProjectCode;

                        var tositeId = request.SupplyManagementRequest.IsSiteRequest ? request.SupplyManagementRequest.ToProjectId : request.SupplyManagementRequest.FromProject.ParentProjectId;
                        if (tositeId > 0)
                        {
                            x.RequestToSiteOrStudy = _context.Project.Where(s => s.Id == tositeId).FirstOrDefault().ProjectCode;
                        }

                    }
                }
            });
            return data;
        }

        public IList<DropDownDto> GetVisitDropDownByAllocation(int projectId)
        {
            var visits = _context.SupplyManagementKitAllocationSettings.Where(x => x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.Id == projectId
            && x.ProjectDesignVisit.DeletedDate == null
                         && x.DeletedDate == null)
                    .Select(x => new DropDownDto
                    {
                        Id = x.ProjectDesignVisit.Id,
                        Value = x.ProjectDesignVisit.DisplayName,
                    }).Distinct().ToList();
            return visits;

        }

        public List<KitListApproved> getApprovedKit(int id)
        {
            var obj = _context.SupplyManagementShipment.Include(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(x => x.Id == id).FirstOrDefault();
            if (obj == null)
                return new List<KitListApproved>();
            var data = new List<KitListApproved>();

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.SupplyManagementRequest.FromProject.ParentProjectId).FirstOrDefault();
            if (setting == null)
                return new List<KitListApproved>();
            if (setting.KitCreationType == KitCreationType.KitWise)
            {

                data = _context.SupplyManagementKITDetail.Where(x =>
                        x.SupplyManagementShipmentId == id
                        && x.Status == Helper.KitStatus.Shipped
                        && x.DeletedDate == null).Select(x => new KitListApproved
                        {
                            Id = x.Id,
                            KitNo = x.KitNo,
                            VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                            SiteCode = x.SupplyManagementKIT.Site.ProjectCode

                        }).OrderByDescending(x => x.KitNo).ToList();
                foreach (var item in data)
                {

                    var refrencetype = Enum.GetValues(typeof(KitStatus))
                                        .Cast<KitStatus>().Select(e => new DropDownStudyDto
                                        {
                                            Id = Convert.ToInt16(e),
                                            Value = e.GetDescription()
                                        }).Where(x => x.Id == 4 || x.Id == 5 || x.Id == 6 || x.Id == 7).ToList();
                    item.StatusList = refrencetype;
                }
            }
            if (setting.KitCreationType == KitCreationType.SequenceWise)
            {

                data = _context.SupplyManagementKITSeries.Where(x =>
                        x.SupplyManagementShipmentId == id
                        && x.Status == Helper.KitStatus.Shipped
                        && x.DeletedDate == null).Select(x => new KitListApproved
                        {
                            Id = x.Id,
                            KitNo = x.KitNo,
                            SiteCode = x.SiteId > 0 ? _context.Project.Where(z => z.Id == x.SiteId).FirstOrDefault().ProjectCode : ""
                        }).OrderByDescending(x => x.KitNo).ToList();
                foreach (var item in data)
                {

                    var refrencetype = Enum.GetValues(typeof(KitStatus))
                                        .Cast<KitStatus>().Select(e => new DropDownStudyDto
                                        {
                                            Id = Convert.ToInt16(e),
                                            Value = e.GetDescription()
                                        }).Where(x => x.Id == 4 || x.Id == 5 || x.Id == 6 || x.Id == 7).ToList();
                    item.StatusList = refrencetype;
                }
            }

            return data;
        }
        public string GenerateKitNo(SupplyManagementKitNumberSettings kitsettings, int noseriese)
        {
            var kitno = kitsettings.Prefix + noseriese.ToString().PadLeft((int)kitsettings.KitNumberLength, '0');
            return kitno;
        }

        public int GetAvailableRemainingkitCount(int ProjectId, int PharmacyStudyProductTypeId)
        {

            var RemainingQuantity = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == ProjectId
                 && x.ProductReceipt.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                 && x.ProductReceipt.Status == ProductVerificationStatus.Approved)
                 .Sum(z => z.RemainingQuantity);
            if (RemainingQuantity > 0)
            {
                var approvedQty = _context.SupplyManagementKITDetail.Where(x => x.DeletedDate == null
                 && x.SupplyManagementKIT.ProjectId == ProjectId && x.SupplyManagementKIT.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                 ).Sum(x => x.NoOfImp);

                var finalRemainingQty = RemainingQuantity - approvedQty;
                return (int)finalRemainingQty;

            }

            return 0;
        }

        public void InsertKitRandomizationDetail(SupplyManagementVisitKITDetailDto supplyManagementVisitKITDetailDto)
        {
            var supplyManagementVisitKITDetail = _mapper.Map<SupplyManagementVisitKITDetail>(supplyManagementVisitKITDetailDto);
            _context.SupplyManagementVisitKITDetail.Add(supplyManagementVisitKITDetail);
            _context.Save();
        }
        public void InsertKitSequenceRandomizationDetail(SupplyManagementVisitKITSequenceDetailDto supplyManagementVisitKITDetailDto)
        {
            var supplyManagementVisitKITDetail = _mapper.Map<SupplyManagementVisitKITSequenceDetail>(supplyManagementVisitKITDetailDto);
            _context.SupplyManagementVisitKITSequenceDetail.Add(supplyManagementVisitKITDetail);
            _context.Save();
        }
        public List<SupplyManagementVisitKITDetailGridDto> GetRandomizationKitNumberAssignList(int projectId, int siteId, int id)
        {
            List<SupplyManagementVisitKITDetailGridDto> data = new List<SupplyManagementVisitKITDetailGridDto>();
            SupplyManagementUploadFileDetail supplyManagementUploadFileDetail = new SupplyManagementUploadFileDetail();

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == projectId).FirstOrDefault();
            if (setting == null)
                new List<SupplyManagementVisitKITDetailGridDto>();

            var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.DeletedDate == null && x.ProjectId == projectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
            if (SupplyManagementUploadFile == null)
            {
                return data;
            }

            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
            {
                supplyManagementUploadFileDetail = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == siteId
               && x.DeletedDate == null && x.RandomizationId == id && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                if (supplyManagementUploadFileDetail == null)
                {
                    return data;
                }
            }
            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
            {
                var country = _context.Project.Where(x => x.Id == siteId).FirstOrDefault();
                var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == country.ManageSiteId).FirstOrDefault();
                if (site != null)
                {
                    supplyManagementUploadFileDetail = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                       && x.SupplyManagementUploadFile.ProjectId == projectId
                      && x.DeletedDate == null && x.RandomizationId == id && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                    if (supplyManagementUploadFileDetail == null)
                    {
                        return data;
                    }
                }
            }
            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
            {
                supplyManagementUploadFileDetail = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == projectId
                && x.DeletedDate == null && x.RandomizationId == id && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();


                if (supplyManagementUploadFileDetail == null)
                {
                    return data;
                }
            }


            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                data = _context.SupplyManagementVisitKITDetail.Where(x =>
                         x.DeletedDate == null
                         && x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
                         && x.Randomization.ProjectId == siteId
                         && x.RandomizationId == id).
                         ProjectTo<SupplyManagementVisitKITDetailGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

                var randomizationdata = _context.Randomization.Include(x => x.Project).Where(x => x.Id == id).FirstOrDefault();


                if (data.Count > 0 && supplyManagementUploadFileDetail != null)
                {
                    var othervisits = _context.SupplyManagementUploadFileVisit.Include(x => x.ProjectDesignVisit).ThenInclude(x => x.ProjectDesignPeriod).ThenInclude(x => x.ProjectDesign).ThenInclude(x => x.Project).Where(x =>
                                                                x.DeletedDate == null
                                                                && x.SupplyManagementUploadFileDetailId == supplyManagementUploadFileDetail.Id
                                                                && !data.Select(z => z.ProjectDesignVisitId).Contains(x.ProjectDesignVisitId)
                                                          ).ToList();

                    if (othervisits.Count > 0)
                    {
                        foreach (var item in othervisits)
                        {
                            SupplyManagementVisitKITDetailGridDto obj = new SupplyManagementVisitKITDetailGridDto();
                            obj.ProjectDesignVisitId = item.ProjectDesignVisitId;
                            obj.KitNo = null;
                            obj.RandomizationId = id;
                            obj.ScreeningNo = randomizationdata.ScreeningNumber;
                            obj.RandomizationNo = randomizationdata.RandomizationNumber;
                            obj.ProjectCode = item.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode;
                            obj.SiteCode = randomizationdata.Project.ProjectCode;
                            obj.VisitName = item.ProjectDesignVisit.DisplayName;
                            obj.ParentProjectId = item.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.Id;
                            obj.ProjectId = randomizationdata.Project.Id;
                            data.Add(obj);
                        }
                    }

                }
            }
            if (setting.KitCreationType == KitCreationType.SequenceWise)
            {
                data = _context.SupplyManagementVisitKITSequenceDetail.Where(x =>
                         x.DeletedDate == null
                         && x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
                         && x.Randomization.ProjectId == siteId
                         && x.RandomizationId == id).
                         ProjectTo<SupplyManagementVisitKITDetailGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

                var randomizationdata = _context.Randomization.Include(x => x.Project).Where(x => x.Id == id).FirstOrDefault();


                if (data.Count > 0 && supplyManagementUploadFileDetail != null)
                {
                    var othervisits = _context.SupplyManagementUploadFileVisit.Include(x => x.ProjectDesignVisit).ThenInclude(x => x.ProjectDesignPeriod).ThenInclude(x => x.ProjectDesign).ThenInclude(x => x.Project).Where(x =>
                                                                x.DeletedDate == null
                                                                && x.SupplyManagementUploadFileDetailId == supplyManagementUploadFileDetail.Id
                                                                && !data.Select(z => z.ProjectDesignVisitId).Contains(x.ProjectDesignVisitId)
                                                          ).ToList();

                    if (othervisits.Count > 0)
                    {
                        foreach (var item in othervisits)
                        {
                            SupplyManagementVisitKITDetailGridDto obj = new SupplyManagementVisitKITDetailGridDto();
                            obj.ProjectDesignVisitId = item.ProjectDesignVisitId;
                            obj.KitNo = null;
                            obj.RandomizationId = id;
                            obj.ScreeningNo = randomizationdata.ScreeningNumber;
                            obj.RandomizationNo = randomizationdata.RandomizationNumber;
                            obj.ProjectCode = item.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.ProjectCode;
                            obj.SiteCode = randomizationdata.Project.ProjectCode;
                            obj.VisitName = item.ProjectDesignVisit.DisplayName;
                            obj.ParentProjectId = item.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.Project.Id;
                            obj.ProjectId = randomizationdata.Project.Id;
                            data.Add(obj);
                        }
                    }

                }
            }
            return data;
        }
        public List<DropDownDto> GetRandomizationDropdownKit(int projectid)
        {
            return _context.Randomization.Where(a => a.DeletedDate == null && a.ProjectId == projectid && a.RandomizationNumber != null)
                .Select(x => new DropDownDto
                {
                    Id = x.Id,
                    Value = Convert.ToString(x.ScreeningNumber + " - " +
                                           x.Initial +
                                           (x.RandomizationNumber == null
                                               ? ""
                                               : " - " + x.RandomizationNumber))
                }).Distinct().ToList();
        }

        public SupplyManagementVisitKITDetailDto SetKitNumber(SupplyManagementVisitKITDetailDto obj)
        {
            int kitcount = 0;
            SupplyManagementUploadFileDetail data = new SupplyManagementUploadFileDetail();
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == obj.ParentProjectId).FirstOrDefault();
            if (setting == null)
                return obj;


            var SupplyManagementUploadFile = _context.SupplyManagementUploadFile.Where(x => x.ProjectId == obj.ParentProjectId && x.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
            if (SupplyManagementUploadFile == null)
                return obj;

            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Site)
            {
                data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.SiteId == obj.ProjectId
               && x.DeletedDate == null && x.RandomizationId == obj.RandomizationId && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                if (data == null)
                    return obj;

            }
            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Country)
            {
                var country = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                var site = _context.ManageSite.Include(x => x.City).ThenInclude(x => x.State).Where(x => x.Id == country.ManageSiteId).FirstOrDefault();
                if (site != null)
                {
                    data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.CountryId == site.City.State.CountryId
                       && x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                      && x.DeletedDate == null && x.RandomizationId == obj.RandomizationId && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();
                    if (data == null)
                        return obj;
                }
            }

            if (SupplyManagementUploadFile.SupplyManagementUploadFileLevel == SupplyManagementUploadFileLevel.Study)
            {
                data = _context.SupplyManagementUploadFileDetail.Where(x => x.SupplyManagementUploadFile.ProjectId == obj.ParentProjectId
                && x.DeletedDate == null && x.RandomizationId == obj.RandomizationId && x.SupplyManagementUploadFile.Status == LabManagementUploadStatus.Approve).FirstOrDefault();

                if (data == null)
                    return obj;
            }

            var visit = _context.SupplyManagementUploadFileVisit.Where(x => x.DeletedDate == null
            && x.ProjectDesignVisitId == obj.ProjectDesignVisitId && x.SupplyManagementUploadFileDetailId == data.Id).FirstOrDefault();
            if (visit == null)
                return obj;

            if (setting.KitCreationType == KitCreationType.KitWise)
            {

                var kitdata = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                  x.DeletedDate == null
                                  && x.SupplyManagementKIT.ProjectDesignVisitId == visit.ProjectDesignVisitId
                                  && x.SupplyManagementKIT.ProjectId == obj.ParentProjectId
                                  && x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode == visit.Value
                                  && x.SupplyManagementShipmentId != null
                                  && x.SupplyManagementKIT.DeletedDate == null
                                  && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ProjectId
                                  && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                  && x.RandomizationId == null).OrderBy(x => x.Id).ToList();
                kitcount = kitdata.Count;
                if (kitdata == null || kitdata.Count == 0)
                    return obj;
                var kit = kitdata.FirstOrDefault();
                kit.RandomizationId = obj.RandomizationId;
                kit.Status = KitStatus.Allocated;
                _context.SupplyManagementKITDetail.Update(kit);
                var supplyManagementVisitKITDetailDto = new SupplyManagementVisitKITDetailDto
                {
                    RandomizationId = obj.RandomizationId,
                    ProjectDesignVisitId = visit.ProjectDesignVisitId,
                    KitNo = kit.KitNo,
                    ProductCode = visit.Value,
                    ReasonOth = obj.ReasonOth,
                    AuditReasonId = obj.AuditReasonId,
                    SupplyManagementKITDetailId = kit.Id
                };
                InsertKitRandomizationDetail(supplyManagementVisitKITDetailDto);
                _context.Save();
                obj.KitNo = kit.KitNo;
                SendRandomizationThresholdEMail(obj, kitcount);
            }
            if (setting.KitCreationType == KitCreationType.SequenceWise)
            {

                var kitSequencedata = _context.SupplyManagementKITSeriesDetail.Include(x => x.SupplyManagementKITSeries).ThenInclude(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                            x.DeletedDate == null
                                            && x.ProjectDesignVisitId == visit.ProjectDesignVisitId
                                            && x.SupplyManagementKITSeries.ProjectId == obj.ParentProjectId
                                            && x.SupplyManagementKITSeries.Status == KitStatus.Allocated
                                            && x.SupplyManagementKITSeries.DeletedDate == null
                                            && x.SupplyManagementKITSeries.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ProjectId
                                            && x.SupplyManagementKITSeries.RandomizationId == obj.RandomizationId).FirstOrDefault();

                if (kitSequencedata == null)
                    return obj;

                kitSequencedata.RandomizationId = obj.RandomizationId;
                _context.SupplyManagementKITSeriesDetail.Update(kitSequencedata);


                var supplyManagementVisitKITDetailDto = new SupplyManagementVisitKITSequenceDetailDto
                {
                    RandomizationId = obj.RandomizationId,
                    ProjectDesignVisitId = kitSequencedata.ProjectDesignVisitId,
                    KitNo = kitSequencedata.SupplyManagementKITSeries.KitNo,
                    ProductCode = visit.Value,
                    SupplyManagementKITSeriesdetailId = kitSequencedata.Id
                };
                InsertKitSequenceRandomizationDetail(supplyManagementVisitKITDetailDto);
                obj.KitNo = kitSequencedata.SupplyManagementKITSeries.KitNo;
                _context.Save();

            }

            return obj;
        }
        public void SendRandomizationThresholdEMail(SupplyManagementVisitKITDetailDto obj, int kitcount)
        {
            var threshold = _context.SupplyManagementKitNumberSettings.Where(x => x.ProjectId == obj.ParentProjectId).FirstOrDefault();
            if (threshold != null && kitcount < threshold.ThresholdValue)
            {
                SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
                IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
                var study = _context.Project.Where(x => x.Id == obj.ParentProjectId).FirstOrDefault();
                if (study != null)
                {
                    var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == obj.ParentProjectId && x.Triggers == SupplyManagementEmailTriggers.Threshold).ToList();
                    if (emailconfiglist != null && emailconfiglist.Count > 0)
                    {
                        var siteconfig = emailconfiglist.Where(x => x.SiteId > 0).ToList();
                        if (siteconfig.Count > 0)
                        {
                            emailconfig = siteconfig.Where(x => x.SiteId == obj.ProjectId).FirstOrDefault();
                        }
                        else
                        {
                            emailconfig = emailconfiglist.FirstOrDefault();
                        }
                        var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                        if (details.Count() > 0)
                        {
                            iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == obj.ParentProjectId).FirstOrDefault().ProjectCode;

                            var site = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault();
                            if (site != null)
                            {
                                iWRSEmailModel.SiteCode = site.ProjectCode;
                                var managesite = _context.ManageSite.Where(x => x.Id == site.ManageSiteId).FirstOrDefault();
                                if (managesite != null)
                                {
                                    iWRSEmailModel.SiteName = managesite.SiteName;
                                }
                            }
                            iWRSEmailModel.ThresholdValue = (int)threshold.ThresholdValue;
                            iWRSEmailModel.RemainingKit = kitcount - 1;

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

        public void InsertKitHistory(SupplyManagementKITDetailHistory supplyManagementVisitKITDetailHistory)
        {
            _context.SupplyManagementKITDetailHistory.Add(supplyManagementVisitKITDetailHistory);
            _context.Save();
        }

        public void InsertKitSequenceHistory(SupplyManagementKITSeriesDetailHistory supplyManagementVisitKITDetailHistory)
        {
            _context.SupplyManagementKITSeriesDetailHistory.Add(supplyManagementVisitKITDetailHistory);
            _context.Save();
        }

        public List<SupplyManagementKITDetailHistoryDto> KitHistoryList(int id)
        {
            var data = _context.SupplyManagementKITDetailHistory.Where(x => x.SupplyManagementKITDetailId == id).
                  ProjectTo<SupplyManagementKITDetailHistoryDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            data.ForEach(x =>
            {
                x.KitNo = _context.SupplyManagementKITDetail.Where(z => z.Id == x.SupplyManagementKITDetailId).FirstOrDefault().KitNo;
                x.RoleName = _context.SecurityRole.Where(z => z.Id == x.RoleId).FirstOrDefault().RoleName;
            });

            return data;
        }

        public List<SupplyManagementKITReturnGridDto> GetKitReturnList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId)
        {
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == projectId).FirstOrDefault();

            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                var data = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Include(x => x.SupplyManagementKIT)
                                                             .ThenInclude(x => x.PharmacyStudyProductType)
                                                             .ThenInclude(x => x.ProductType)
                                                             .Where(x => x.SupplyManagementKIT.ProjectId == projectId
                                                              && x.DeletedDate == null
                                                              && x.Status != KitStatus.Discard
                                                              ).Select(x => new SupplyManagementKITReturnGridDto
                                                              {
                                                                  KitNo = x.KitNo,
                                                                  ProjectDesignVisitId = x.SupplyManagementKIT.ProjectDesignVisitId,
                                                                  ProductTypeName = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode,
                                                                  NoOfImp = x.NoOfImp,
                                                                  RandomizationId = x.RandomizationId,
                                                                  StudyCode = x.SupplyManagementKIT.Project.ProjectCode,
                                                                  SupplyManagementKITDetailId = x.Id,
                                                                  VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                                                  Status = x.Status,
                                                                  PrevStatus = x.PrevStatus,
                                                                  ReturnImp = x.ReturnImp,
                                                                  ReturnReason = x.ReturnReason,
                                                                  CreatedByUser = x.CreatedByUser.UserName,
                                                                  ModifiedByUser = x.ModifiedByUser.UserName,
                                                                  DeletedByUser = x.DeletedByUser.UserName,
                                                                  CreatedDate = x.CreatedDate,
                                                                  ModifiedDate = x.ModifiedDate,
                                                                  DeletedDate = x.DeletedDate,
                                                                  SiteId = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId : 0,
                                                                  SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : "",
                                                                  ActionBy = x.ReturnBy,
                                                                  ActionDate = x.ReturnDate,
                                                                  IsUnUsed = x.IsUnUsed
                                                              }).ToList();
                if (data.Count > 0)
                {
                    data.ForEach(x =>
                    {
                        var randomization = _context.Randomization.Where(z => z.Id == x.RandomizationId).FirstOrDefault();
                        if (randomization != null)
                        {
                            x.RandomizationNo = randomization.RandomizationNumber;
                            x.ScreeningNo = randomization.ScreeningNumber;
                        }
                        var returndata = _context.SupplyManagementKITReturn.Where(z => z.SupplyManagementKITDetailId == x.SupplyManagementKITDetailId).FirstOrDefault();
                        if (returndata != null)
                        {
                            x.SupplyManagementKITReturnId = returndata.Id;
                            x.ReturnDate = returndata.CreatedDate;
                            x.ReturnBy = _context.Users.Where(z => z.Id == returndata.CreatedBy).FirstOrDefault().UserName;
                            x.ReasonOth = returndata.ReasonOth;
                            x.Reason = returndata.AuditReasonId > 0 ? _context.AuditReason.Where(s => s.Id == returndata.AuditReasonId).FirstOrDefault().ReasonName : "";
                        }
                        var returnverificationdata = _context.SupplyManagementKITReturnVerification.Include(x => x.AuditReason).Where(z => z.SupplyManagementKITDetailId == x.SupplyManagementKITDetailId).FirstOrDefault();
                        if (returnverificationdata != null)
                        {
                            x.SupplyManagementKITReturnVerificationId = returnverificationdata.Id;
                            x.ReturnVerificationDate = returnverificationdata.CreatedDate;
                            x.ReturnVerificationBy = _context.Users.Where(z => z.Id == returnverificationdata.CreatedBy).FirstOrDefault().UserName;
                            x.ReturnVerificationReasonOth = returnverificationdata.ReasonOth;
                            x.ReturnVerificationReason = returnverificationdata.AuditReason.ReasonName;
                        }
                        x.ActionByName = x.ActionBy > 0 ? _context.Users.Where(z => z.Id == x.ActionBy).FirstOrDefault().UserName : "";
                        x.ActionDate = x.ActionDate;
                        if (kitType == KitStatusRandomization.Return)
                        {
                            if (x.Status == KitStatus.Returned && (x.PrevStatus == KitStatus.Missing || x.PrevStatus == KitStatus.Damaged))
                            {
                                x.StatusName = x.PrevStatus.ToString();
                            }
                            if (x.Status == KitStatus.Returned && (x.PrevStatus == KitStatus.WithoutIssue || x.PrevStatus == KitStatus.WithIssue) && x.IsUnUsed == true)
                            {
                                x.StatusName = "Unused";
                            }
                            if (x.Status == KitStatus.Returned && x.PrevStatus == KitStatus.Allocated && x.IsUnUsed == false)
                            {
                                x.StatusName = "Used";
                            }

                        }
                        else
                        {
                            x.StatusName = x.Status.GetDescription();
                        }

                    });
                    if (kitType == KitStatusRandomization.Used)
                    {
                        data = data.Where(x => x.Status == KitStatus.Allocated).ToList();
                    }
                    if (kitType == KitStatusRandomization.Damaged)
                    {
                        data = data.Where(x => x.Status == KitStatus.Damaged).ToList();
                    }
                    if (kitType == KitStatusRandomization.UnUsed)
                    {
                        data = data.Where(x => x.Status == KitStatus.WithoutIssue || x.Status == KitStatus.WithIssue).ToList();
                    }
                    if (kitType == KitStatusRandomization.Return)
                    {
                        data = data.Where(x => x.Status == KitStatus.Returned).ToList();
                    }
                    if (kitType == KitStatusRandomization.Missing)
                    {
                        data = data.Where(x => x.Status == KitStatus.Missing).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceive)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceive).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceiveDamaged)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceiveDamaged).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceiveMissing)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceiveMissing).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceiveWithIssue)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceiveWithIssue).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceiveWithoutIssue)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceiveWithoutIssue).ToList();
                    }
                    if (visitId > 0)
                    {
                        data = data.Where(x => x.ProjectDesignVisitId == visitId).ToList();
                    }
                    if (randomizationId > 0)
                    {
                        data = data.Where(x => x.RandomizationId == randomizationId).ToList();
                    }
                    if (siteId > 0)
                    {
                        data = data.Where(x => x.SiteId == siteId).ToList();
                    }
                }

                return data;
            }
            else
            {
                var data = _context.SupplyManagementKITSeries.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest)
                                                            .Where(x => x.ProjectId == projectId
                                                             && x.DeletedDate == null
                                                             && x.Status != KitStatus.Discard
                                                             ).Select(x => new SupplyManagementKITReturnGridDto
                                                             {
                                                                 KitNo = x.KitNo,
                                                                 RandomizationId = x.RandomizationId,
                                                                 StudyCode = x.Project.ProjectCode,
                                                                 SupplyManagementKITSeriesId = x.Id,
                                                                 Status = x.Status,
                                                                 PrevStatus = x.PrevStatus,
                                                                 CreatedByUser = x.CreatedByUser.UserName,
                                                                 ModifiedByUser = x.ModifiedByUser.UserName,
                                                                 DeletedByUser = x.DeletedByUser.UserName,
                                                                 CreatedDate = x.CreatedDate,
                                                                 ModifiedDate = x.ModifiedDate,
                                                                 DeletedDate = x.DeletedDate,
                                                                 SiteId = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId : 0,
                                                                 SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : "",
                                                                 IsUnUsed = x.IsUnUsed,
                                                                 RandomizationNo = x.Randomization != null ? x.Randomization.RandomizationNumber : "",
                                                                 ScreeningNo = x.Randomization != null ? x.Randomization.ScreeningNumber : "",
                                                             }).ToList();
                if (data.Count > 0)
                {
                    data.ForEach(x =>
                    {
                        var returndata = _context.SupplyManagementKITReturnSeries.Where(z => z.SupplyManagementKITSeriesId == x.SupplyManagementKITSeriesId).FirstOrDefault();
                        if (returndata != null)
                        {
                            x.SupplyManagementKITReturnSeriesId = returndata.Id;
                            x.ReturnDate = returndata.CreatedDate;
                            x.ReturnBy = _context.Users.Where(z => z.Id == returndata.CreatedBy).FirstOrDefault().UserName;
                            x.ReasonOth = returndata.ReasonOth;
                            x.Reason = returndata.AuditReasonId > 0 ? _context.AuditReason.Where(s => s.Id == returndata.AuditReasonId).FirstOrDefault().ReasonName : "";
                        }
                        var returnverificationdata = _context.SupplyManagementKITReturnVerificationSeries.Include(x => x.AuditReason).Where(z => z.SupplyManagementKITSeriesId == x.SupplyManagementKITSeriesId).FirstOrDefault();
                        if (returnverificationdata != null)
                        {
                            x.SupplyManagementKITReturnVerificationId = returnverificationdata.Id;
                            x.ReturnVerificationDate = returnverificationdata.CreatedDate;
                            x.ReturnVerificationBy = _context.Users.Where(z => z.Id == returnverificationdata.CreatedBy).FirstOrDefault().UserName;
                            x.ReturnVerificationReasonOth = returnverificationdata.ReasonOth;
                            x.ReturnVerificationReason = returnverificationdata.AuditReason.ReasonName;
                        }

                        if (kitType == KitStatusRandomization.Return)
                        {
                            if (x.Status == KitStatus.Returned && (x.PrevStatus == KitStatus.Missing || x.PrevStatus == KitStatus.Damaged))
                            {
                                x.StatusName = x.PrevStatus.ToString();
                            }
                            if (x.Status == KitStatus.Returned && (x.PrevStatus == KitStatus.WithoutIssue || x.PrevStatus == KitStatus.WithIssue) && x.IsUnUsed == true)
                            {
                                x.StatusName = "Unused";
                            }
                            if (x.Status == KitStatus.Returned && x.PrevStatus == KitStatus.Allocated && x.IsUnUsed == false)
                            {
                                x.StatusName = "Used";
                            }

                        }
                        else
                        {
                            x.StatusName = x.Status.GetDescription();
                        }

                    });
                    if (kitType == KitStatusRandomization.Used)
                    {
                        data = data.Where(x => x.Status == KitStatus.Allocated).ToList();
                    }
                    if (kitType == KitStatusRandomization.Damaged)
                    {
                        data = data.Where(x => x.Status == KitStatus.Damaged).ToList();
                    }
                    if (kitType == KitStatusRandomization.UnUsed)
                    {
                        data = data.Where(x => x.Status == KitStatus.WithoutIssue || x.Status == KitStatus.WithIssue).ToList();
                    }
                    if (kitType == KitStatusRandomization.Return)
                    {
                        data = data.Where(x => x.Status == KitStatus.Returned).ToList();
                    }
                    if (kitType == KitStatusRandomization.Missing)
                    {
                        data = data.Where(x => x.Status == KitStatus.Missing).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceive)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceive).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceiveDamaged)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceiveDamaged).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceiveMissing)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceiveMissing).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceiveWithIssue)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceiveWithIssue).ToList();
                    }
                    if (kitType == KitStatusRandomization.ReturnReceiveWithoutIssue)
                    {
                        data = data.Where(x => x.Status == KitStatus.ReturnReceiveWithoutIssue).ToList();
                    }
                    if (visitId > 0)
                    {
                        data = data.Where(x => x.ProjectDesignVisitId == visitId).ToList();
                    }
                    if (randomizationId > 0)
                    {
                        data = data.Where(x => x.RandomizationId == randomizationId).ToList();
                    }
                    if (siteId > 0)
                    {
                        data = data.Where(x => x.SiteId == siteId).ToList();
                    }
                }

                return data;
            }

        }
        public SupplyManagementKITReturnGridDto ReturnSave(SupplyManagementKITReturnGridDto obj)
        {
            var data = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT)
                                                        .ThenInclude(x => x.PharmacyStudyProductType)
                                                        .ThenInclude(x => x.ProductType)
                                                        .Where(x => x.Id == obj.SupplyManagementKITDetailId).FirstOrDefault();
            if (data != null)
            {
                data.ReturnImp = obj.ReturnImp;
                data.ReturnReason = obj.ReturnReason;
                data.ReturnBy = _jwtTokenAccesser.UserId;
                data.ReturnDate = DateTime.Now;
                _context.SupplyManagementKITDetail.Update(data);
                _context.Save();
            }

            return obj;
        }
        public void ReturnSaveAll(SupplyManagementKITReturnDtofinal data)
        {
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == data.ProjectId).FirstOrDefault();
            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                if (data != null && data.list.Count > 0)
                {
                    foreach (var obj in data.list)
                    {
                        var datakit = _context.SupplyManagementKITDetail.Where(x => x.Id == obj.SupplyManagementKITDetailId).FirstOrDefault();
                        if (datakit != null)
                        {
                            datakit.ReturnImp = obj.ReturnImp;
                            datakit.ReturnReason = obj.ReturnReason;
                            datakit.PrevStatus = datakit.Status;
                            datakit.Status = KitStatus.Returned;
                            datakit.IsUnUsed = data.IsUnUsed;
                            _context.SupplyManagementKITDetail.Update(datakit);

                            SupplyManagementKITReturn returnkit = new SupplyManagementKITReturn();
                            returnkit.ReturnImp = obj.ReturnImp;
                            returnkit.Commnets = obj.ReturnReason;
                            returnkit.ReasonOth = data.ReasonOth;
                            returnkit.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                            returnkit.AuditReasonId = data.AuditReasonId;
                            _context.SupplyManagementKITReturn.Add(returnkit);

                            SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                            history.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                            history.Status = KitStatus.Returned;
                            history.RoleId = _jwtTokenAccesser.RoleId;
                            _context.SupplyManagementKITDetailHistory.Add(history);

                            _context.Save();
                        }

                    }
                }
            }
            if (setting.KitCreationType == KitCreationType.SequenceWise)
            {
                if (data != null && data.list.Count > 0)
                {
                    foreach (var obj in data.list)
                    {
                        var datakit = _context.SupplyManagementKITSeries.Where(x => x.Id == obj.SupplyManagementKITSeriesId).FirstOrDefault();
                        if (datakit != null)
                        {
                            datakit.PrevStatus = datakit.Status;
                            datakit.Status = KitStatus.Returned;
                            datakit.IsUnUsed = data.IsUnUsed;
                            _context.SupplyManagementKITSeries.Update(datakit);

                            SupplyManagementKITReturnSeries returnkit = new SupplyManagementKITReturnSeries();
                            returnkit.ReasonOth = data.ReasonOth;
                            returnkit.SupplyManagementKITSeriesId = obj.SupplyManagementKITSeriesId;
                            returnkit.AuditReasonId = data.AuditReasonId;
                            _context.SupplyManagementKITReturnSeries.Add(returnkit);

                            SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                            history.SupplyManagementKITSeriesId = obj.SupplyManagementKITSeriesId;
                            history.Status = KitStatus.Returned;
                            history.RoleId = _jwtTokenAccesser.RoleId;
                            _context.SupplyManagementKITSeriesDetailHistory.Add(history);
                            _context.Save();
                        }

                    }
                }
            }
        }

        public List<SupplyManagementKITDiscardGridDto> GetKitDiscardList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId)
        {
            var data = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Include(x => x.SupplyManagementKIT)
                                                         .ThenInclude(x => x.PharmacyStudyProductType)
                                                         .ThenInclude(x => x.ProductType)
                                                         .Where(x => x.SupplyManagementKIT.ProjectId == projectId
                                                          && x.DeletedDate == null

                                                          ).Select(x => new SupplyManagementKITDiscardGridDto
                                                          {
                                                              KitNo = x.KitNo,
                                                              ProjectDesignVisitId = x.SupplyManagementKIT.ProjectDesignVisitId,
                                                              ProductTypeName = x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode,
                                                              NoOfImp = x.NoOfImp,
                                                              RandomizationId = x.RandomizationId,
                                                              StudyCode = x.SupplyManagementKIT.Project.ProjectCode,
                                                              SupplyManagementKITDetailId = x.Id,
                                                              VisitName = x.SupplyManagementKIT.ProjectDesignVisit.DisplayName,
                                                              Status = x.Status,
                                                              ReturnImp = x.ReturnImp,
                                                              ReturnReason = x.ReturnReason,
                                                              CreatedByUser = x.CreatedByUser.UserName,
                                                              ModifiedByUser = x.ModifiedByUser.UserName,
                                                              DeletedByUser = x.DeletedByUser.UserName,
                                                              CreatedDate = x.CreatedDate,
                                                              ModifiedDate = x.ModifiedDate,
                                                              DeletedDate = x.DeletedDate,
                                                              SiteId = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId : 0,
                                                              SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : "",
                                                              IsUnUsed = x.IsUnUsed
                                                          }).ToList();
            if (data.Count > 0)
            {
                var history = _context.SupplyManagementKITDetailHistory.Include(x => x.SupplyManagementKITDetail).Where(x => data.Select(s => s.SupplyManagementKITDetailId).Contains(x.SupplyManagementKITDetailId)).ToList();
                data.ForEach(x =>
                {
                    var randomization = _context.Randomization.Where(z => z.Id == x.RandomizationId).FirstOrDefault();
                    if (randomization != null)
                    {
                        x.RandomizationNo = randomization.RandomizationNumber;
                        x.ScreeningNo = randomization.ScreeningNumber;
                    }
                    var returndata = _context.SupplyManagementKITReturn.Where(z => z.SupplyManagementKITDetailId == x.SupplyManagementKITDetailId).FirstOrDefault();
                    if (returndata != null)
                    {
                        x.SupplyManagementKITReturnId = returndata.Id;
                        x.ReturnDate = returndata.CreatedDate;
                        x.ReturnBy = _context.Users.Where(z => z.Id == returndata.CreatedBy).FirstOrDefault().UserName;
                    }
                    var discarddata = _context.SupplyManagementKITDiscard.Where(z => z.SupplyManagementKITDetailId == x.SupplyManagementKITDetailId).FirstOrDefault();
                    if (discarddata != null)
                    {
                        x.SupplyManagementKITDiscardId = discarddata.Id;
                        x.DiscardDate = discarddata.CreatedDate;
                        x.DiscardBy = _context.Users.Where(z => z.Id == discarddata.CreatedBy).FirstOrDefault().UserName;
                    }

                });
                if (kitType == KitStatusRandomization.Used)
                {
                    data = data.Where(x => x.Status == KitStatus.Returned && x.RandomizationId != null).ToList();
                }
                if (kitType == KitStatusRandomization.UnUsed)
                {
                    data = data.Where(x => x.Status == KitStatus.Returned && x.IsUnUsed == true).ToList();
                }
                if (kitType == KitStatusRandomization.Discard)
                {
                    data = data.Where(x => x.Status == KitStatus.Discard).ToList();
                }
                if (kitType == KitStatusRandomization.Damaged)
                {
                    var damageddata = history.Where(x => x.Status == KitStatus.Damaged).ToList();
                    data = data.Where(x => damageddata.Select(z => z.SupplyManagementKITDetailId).Contains(x.SupplyManagementKITDetailId)).ToList();
                }
                if (kitType == KitStatusRandomization.Sendtosponser)
                {
                    data = data.Where(x => x.Status == KitStatus.Sendtosponser).ToList();
                }
                if (visitId > 0)
                {
                    data = data.Where(x => x.ProjectDesignVisitId == visitId).ToList();
                }
                if (randomizationId > 0)
                {
                    data = data.Where(x => x.RandomizationId == randomizationId).ToList();
                }
                if (siteId > 0)
                {
                    data = data.Where(x => x.SiteId == siteId).ToList();
                }
            }

            return data;

        }

        public void KitDiscard(SupplyManagementKITDiscardDtofinal data)
        {
            if (data != null && data.list.Count > 0)
            {
                foreach (var obj in data.list)
                {
                    var datakit = _context.SupplyManagementKITDetail.Where(x => x.Id == obj.SupplyManagementKITDetailId).FirstOrDefault();
                    if (datakit != null)
                    {
                        datakit.ReturnImp = obj.ReturnImp;
                        datakit.ReturnReason = obj.ReturnReason;
                        datakit.Status = KitStatus.Discard;
                        _context.SupplyManagementKITDetail.Update(datakit);

                        SupplyManagementKITDiscard returnkit = new SupplyManagementKITDiscard();
                        returnkit.ReasonOth = data.ReasonOth;
                        returnkit.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                        returnkit.AuditReasonId = data.AuditReasonId;
                        returnkit.Status = KitStatus.Discard;
                        returnkit.RoleId = _jwtTokenAccesser.RoleId;
                        _context.SupplyManagementKITDiscard.Add(returnkit);

                        SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                        history.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                        history.Status = KitStatus.Discard;
                        history.RoleId = _jwtTokenAccesser.RoleId;
                        _context.SupplyManagementKITDetailHistory.Add(history);
                        _context.Save();
                    }

                }
            }
        }

        public void KitSendtoSponser(SupplyManagementKITDiscardDtofinal data)
        {
            if (data != null && data.list.Count > 0)
            {
                foreach (var obj in data.list)
                {
                    var datakit = _context.SupplyManagementKITDetail.Where(x => x.Id == obj.SupplyManagementKITDetailId).FirstOrDefault();
                    if (datakit != null)
                    {
                        datakit.ReturnImp = obj.ReturnImp;
                        datakit.ReturnReason = obj.ReturnReason;
                        datakit.Status = KitStatus.Sendtosponser;
                        _context.SupplyManagementKITDetail.Update(datakit);

                        SupplyManagementKITDiscard returnkit = new SupplyManagementKITDiscard();
                        returnkit.ReasonOth = data.ReasonOth;
                        returnkit.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                        returnkit.AuditReasonId = data.AuditReasonId;
                        returnkit.Status = KitStatus.Sendtosponser;
                        returnkit.RoleId = _jwtTokenAccesser.RoleId;
                        _context.SupplyManagementKITDiscard.Add(returnkit);

                        SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                        history.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                        history.Status = KitStatus.Sendtosponser;
                        history.RoleId = _jwtTokenAccesser.RoleId;
                        _context.SupplyManagementKITDetailHistory.Add(history);
                        _context.Save();
                    }

                }
            }
        }

        public void SendKitReturnEmail(SupplyManagementKITReturnDtofinal obj)
        {
            SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
            IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();

            var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == obj.ProjectId && x.Triggers == SupplyManagementEmailTriggers.KitReturn).ToList();
            if (emailconfiglist != null && emailconfiglist.Count > 0)
            {

                emailconfig = emailconfiglist.FirstOrDefault();

                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                if (details.Count() > 0)
                {
                    iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault().ProjectCode;

                    if (obj.siteId > 0)
                    {
                        var site = _context.Project.Where(x => x.Id == obj.siteId).FirstOrDefault();
                        if (site != null)
                        {
                            iWRSEmailModel.SiteCode = site.ProjectCode;
                            var managesite = _context.ManageSite.Where(x => x.Id == site.ManageSiteId).FirstOrDefault();
                            if (managesite != null)
                            {
                                iWRSEmailModel.SiteName = managesite.SiteName;
                            }
                        }
                    }

                    iWRSEmailModel.TypeOfKitReturn = obj.TypeOfKitReturn.GetDescription();
                    iWRSEmailModel.NoOfKitReturn = obj.NoOfKitReturn;
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

        public void returnVerificationStatus(SupplyManagementKITReturnVerificationDto data)
        {
            var datakit = _context.SupplyManagementKITDetail.Where(x => x.Id == data.SupplyManagementKITDetailId).FirstOrDefault();
            if (datakit != null)
            {
                datakit.Status = data.Status;
                _context.SupplyManagementKITDetail.Update(datakit);

                SupplyManagementKITReturnVerification supplyManagementKITReturnVerification = new SupplyManagementKITReturnVerification();
                supplyManagementKITReturnVerification.SupplyManagementKITDetailId = data.SupplyManagementKITDetailId;
                supplyManagementKITReturnVerification.Status = data.Status;
                supplyManagementKITReturnVerification.AuditReasonId = data.AuditReasonId;
                supplyManagementKITReturnVerification.ReasonOth = data.ReasonOth;

                _context.SupplyManagementKITReturnVerification.Add(supplyManagementKITReturnVerification);

                SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                history.SupplyManagementKITDetailId = data.SupplyManagementKITDetailId;
                history.Status = data.Status;
                history.RoleId = _jwtTokenAccesser.RoleId;
                _context.SupplyManagementKITDetailHistory.Add(history);
                _context.Save();
            }
        }
        public void returnVerificationStatusSequence(SupplyManagementKITReturnVerificationSequenceDto data)
        {
            var datakit = _context.SupplyManagementKITSeries.Where(x => x.Id == data.SupplyManagementKITSeriesId).FirstOrDefault();
            if (datakit != null)
            {
                datakit.Status = data.Status;
                _context.SupplyManagementKITSeries.Update(datakit);

                SupplyManagementKITReturnVerificationSeries supplyManagementKITReturnVerification = new SupplyManagementKITReturnVerificationSeries();
                supplyManagementKITReturnVerification.SupplyManagementKITSeriesId = data.SupplyManagementKITSeriesId;
                supplyManagementKITReturnVerification.Status = data.Status;
                supplyManagementKITReturnVerification.AuditReasonId = data.AuditReasonId;
                supplyManagementKITReturnVerification.ReasonOth = data.ReasonOth;
                _context.SupplyManagementKITReturnVerificationSeries.Add(supplyManagementKITReturnVerification);

                SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                history.SupplyManagementKITSeriesId = data.SupplyManagementKITSeriesId;
                history.Status = data.Status;
                history.RoleId = _jwtTokenAccesser.RoleId;
                _context.SupplyManagementKITSeriesDetailHistory.Add(history);
                _context.Save();
            }
        }

        public List<SupplyManagementKITSeriesGridDto> GetKITSeriesList(bool isDeleted, int ProjectId)
        {
            var data = _context.SupplyManagementKITSeries.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementKITSeriesGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                if (x.SiteId > 0)
                {
                    x.SiteCode = _context.Project.Where(z => z.Id == x.SiteId).FirstOrDefault().ProjectCode;
                }
                if (x.SupplyManagementShipmentId > 0)
                {
                    var request = _context.SupplyManagementShipment.Include(r => r.SupplyManagementRequest).ThenInclude(x => x.FromProject).Where(z => z.Id == x.SupplyManagementShipmentId).FirstOrDefault();
                    if (request != null)
                    {
                        x.RequestFromSite = request.SupplyManagementRequest.FromProject.ProjectCode;

                        var tositeId = request.SupplyManagementRequest.IsSiteRequest ? request.SupplyManagementRequest.ToProjectId : request.SupplyManagementRequest.FromProject.ParentProjectId;
                        if (tositeId > 0)
                        {
                            x.RequestToSiteOrStudy = _context.Project.Where(s => s.Id == tositeId).FirstOrDefault().ProjectCode;
                        }

                    }
                }
            });
            return data;
        }

        public List<SupplyManagementKITSeriesDetailGridDto> GetKITSeriesDetailList(int id)
        {
            var data = _context.SupplyManagementKITSeriesDetail.Where(x => x.SupplyManagementKITSeriesId == id).
                   ProjectTo<SupplyManagementKITSeriesDetailGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            return data;
        }
        public List<SupplyManagementKITSeriesDetailHistoryGridDto> GetKITSeriesDetailHistoryList(int id)
        {
            var data = _context.SupplyManagementKITSeriesDetailHistory.Where(x => x.SupplyManagementKITSeriesId == id).
                   ProjectTo<SupplyManagementKITSeriesDetailHistoryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                x.RoleName = _context.SecurityRole.Where(z => z.Id == x.RoleId).FirstOrDefault().RoleName;
            });
            return data;
        }
        public string CheckAvailableQtySequenceKit(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto)
        {
            if (supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Count > 0)
            {
                var data = supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.GroupBy(x => x.PharmacyStudyProductTypeId).Select(x => new
                {
                    Id = x.Key
                }).ToList();

                if (data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        var Total = supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Where(x => x.PharmacyStudyProductTypeId == item.Id).Sum(z => z.NoOfImp * supplyManagementKITSeriesDto.NoofPatient);
                        var availableqty = GetAvailableRemainingkitSequenceCount(supplyManagementKITSeriesDto.ProjectId, item.Id);
                        if (availableqty < Total)
                        {
                            var PharmacyStudyProductType = _context.PharmacyStudyProductType.Include(x => x.ProductType).Where(x => x.DeletedDate == null && x.Id == item.Id && x.ProjectId == supplyManagementKITSeriesDto.ProjectId).FirstOrDefault();


                            return "Quantity is not available for " + PharmacyStudyProductType.ProductType.ProductTypeCode;
                        }
                    }

                }
            }


            return "";
        }

        public void UnblindTreatment(SupplyManagementUnblindTreatmentDto data)
        {
            if (data != null && data.list.Count > 0)
            {
                foreach (var item in data.list)
                {
                    var datakit = _context.SupplyManagementUnblindTreatment.Where(x => x.RandomizationId == item.RandomizationId && x.DeletedDate == null).FirstOrDefault();
                    if (datakit == null)
                    {
                        SupplyManagementUnblindTreatment supplyManagementUnblindTreatment = new SupplyManagementUnblindTreatment();
                        supplyManagementUnblindTreatment.RoleId = _jwtTokenAccesser.RoleId;
                        supplyManagementUnblindTreatment.ReasonOth = data.ReasonOth;
                        supplyManagementUnblindTreatment.AuditReasonId = data.AuditReasonId;
                        supplyManagementUnblindTreatment.RandomizationId = item.RandomizationId;
                        supplyManagementUnblindTreatment.TypeofUnblind = data.TypeofUnblind;
                        _context.SupplyManagementUnblindTreatment.Add(supplyManagementUnblindTreatment);
                        _context.Save();
                        data.UnblindDatetime = supplyManagementUnblindTreatment.CreatedDate;
                        data.RandomizationId = item.RandomizationId;
                        SendKitUnblindEmail(data);
                    }

                }
            }

        }

        public List<SupplyManagementUnblindTreatmentGridDto> GetUnblindList(int projectId, int? siteId, int? randomizationId)
        {
            var data = _context.Randomization.Include(x => x.Project).Where(x => x.DeletedDate == null && x.Project.ParentProjectId == projectId && x.RandomizationNumber != null).
                    ProjectTo<SupplyManagementUnblindTreatmentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            if (data == null || data.Count == 0)
                return new List<SupplyManagementUnblindTreatmentGridDto>();

            if (siteId > 0)
            {
                data = data.Where(x => x.ProjectId == siteId).ToList();
            }
            if (randomizationId > 0)
            {
                data = data.Where(x => x.Id == randomizationId).ToList();
            }

            data.ForEach(x =>
            {
                if (x.ParentProjectId > 0)
                {
                    x.StudyCode = _context.Project.Where(z => z.Id == x.ParentProjectId).FirstOrDefault().ProjectCode;
                }
                var unblind = _context.SupplyManagementUnblindTreatment.Include(x => x.AuditReason).Where(z => z.RandomizationId == x.RandomizationId).FirstOrDefault();
                if (unblind != null)
                {
                    x.TypeofUnblindName = unblind.TypeofUnblind.GetDescription();
                    x.ReasonOth = unblind.ReasonOth;
                    x.Reason = unblind.AuditReasonId > 0 ? unblind.AuditReason.ReasonName : "";
                    x.ActionBy = _context.Users.Where(z => z.Id == unblind.CreatedBy).FirstOrDefault().UserName;
                    x.ActionDate = unblind.CreatedDate;
                    x.ActionByRole = _context.SecurityRole.Where(s => s.Id == unblind.RoleId).FirstOrDefault().RoleName;

                    var setting = _context.SupplyManagementKitNumberSettings.Where(z => z.DeletedDate == null && z.ProjectId == x.ParentProjectId).FirstOrDefault();

                    if (setting != null)
                    {
                        if (setting.KitCreationType == KitCreationType.KitWise)
                        {
                            var visits = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(z => z.ProductType).Include(z => z.SupplyManagementKIT).ThenInclude(z => z.ProjectDesignVisit)
                             .Where(s => s.RandomizationId == x.RandomizationId && s.DeletedDate == null).Select(z => z.SupplyManagementKIT.ProjectDesignVisit.DisplayName).ToList();
                            if (visits.Count > 0)
                                x.VisitName = string.Join(",", visits);

                            var treatment = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(z => z.ProductType).Include(z => z.SupplyManagementKIT).ThenInclude(z => z.ProjectDesignVisit)
                            .Where(s => s.RandomizationId == x.RandomizationId && s.DeletedDate == null).Select(z => z.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode).ToList();
                            if (visits.Count > 0)
                                x.TreatmentType = string.Join(",", treatment.Distinct());
                        }
                        else
                        {
                            var visits = _context.SupplyManagementKITSeriesDetail.Include(x => x.SupplyManagementKITSeries).Include(z => z.ProjectDesignVisit).Include(x => x.PharmacyStudyProductType).ThenInclude(z => z.ProductType)
                             .Where(s => s.RandomizationId == x.RandomizationId && s.DeletedDate == null).Select(z => z.ProjectDesignVisit.DisplayName).ToList();
                            if (visits.Count > 0)
                                x.VisitName = string.Join(",", visits);

                            var treatment = _context.SupplyManagementKITSeriesDetail.Include(x => x.SupplyManagementKITSeries).Include(z => z.ProjectDesignVisit).Include(x => x.PharmacyStudyProductType).ThenInclude(z => z.ProductType)
                            .Where(s => s.RandomizationId == x.RandomizationId && s.DeletedDate == null).Select(z => z.PharmacyStudyProductType.ProductType.ProductTypeCode).ToList();
                            if (visits.Count > 0)
                                x.TreatmentType = string.Join(",", treatment.Distinct());
                        }
                    }
                }
            });

            return data;

        }
        public int GetAvailableRemainingkitSequenceCount(int ProjectId, int PharmacyStudyProductTypeId)
        {

            var RemainingQuantity = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == ProjectId
                 && x.ProductReceipt.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                 && x.ProductReceipt.Status == ProductVerificationStatus.Approved)
                 .Sum(z => z.RemainingQuantity);
            if (RemainingQuantity > 0)
            {
                var approvedQty = _context.SupplyManagementKITSeriesDetail.Include(x => x.SupplyManagementKITSeries).Where(x => x.DeletedDate == null
                   && x.SupplyManagementKITSeries.ProjectId == ProjectId && x.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                   && x.SupplyManagementKITSeries.DeletedDate == null
                 ).Sum(x => x.NoOfImp);

                var finalRemainingQty = RemainingQuantity - approvedQty;
                return (int)finalRemainingQty;
            }
            return 0;
        }
        public void SendKitUnblindEmail(SupplyManagementUnblindTreatmentDto obj)
        {

            SupplyManagementEmailConfiguration emailconfig = new SupplyManagementEmailConfiguration();
            IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();

            var emailconfiglist = _context.SupplyManagementEmailConfiguration.Where(x => x.DeletedDate == null && x.IsActive == true && x.ProjectId == obj.ProjectId && x.Triggers == SupplyManagementEmailTriggers.Unblind).ToList();
            if (emailconfiglist != null && emailconfiglist.Count > 0)
            {

                emailconfig = emailconfiglist.FirstOrDefault();

                var details = _context.SupplyManagementEmailConfigurationDetail.Include(x => x.Users).Include(x => x.Users).Where(x => x.DeletedDate == null && x.SupplyManagementEmailConfigurationId == emailconfig.Id).ToList();
                if (details.Count() > 0)
                {
                    iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == obj.ProjectId).FirstOrDefault().ProjectCode;

                    if (obj.SiteId > 0)
                    {
                        var site = _context.Project.Where(x => x.Id == obj.SiteId).FirstOrDefault();
                        if (site != null)
                        {
                            iWRSEmailModel.SiteCode = site.ProjectCode;
                            var managesite = _context.ManageSite.Where(x => x.Id == site.ManageSiteId).FirstOrDefault();
                            if (managesite != null)
                            {
                                iWRSEmailModel.SiteName = managesite.SiteName;
                            }
                        }
                    }

                    iWRSEmailModel.UnblindDatetime = (DateTime)obj.UnblindDatetime;
                    if (!string.IsNullOrEmpty(obj.ReasonOth))
                        iWRSEmailModel.ReasonForUnblind = obj.ReasonOth;
                    else
                        iWRSEmailModel.ReasonForUnblind = _context.AuditReason.Where(s => s.Id == obj.AuditReasonId).FirstOrDefault().ReasonName;
                    iWRSEmailModel.UnblindBy = _context.Users.Where(s => s.Id == _jwtTokenAccesser.UserId).FirstOrDefault().UserName;

                    var setting = _context.SupplyManagementKitNumberSettings.Where(z => z.DeletedDate == null && z.ProjectId == obj.ProjectId).FirstOrDefault();

                    if (setting != null)
                    {
                        if (setting.KitCreationType == KitCreationType.KitWise)
                        {
                            var treatment = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).ThenInclude(x => x.PharmacyStudyProductType).ThenInclude(z => z.ProductType).Include(z => z.SupplyManagementKIT).ThenInclude(z => z.ProjectDesignVisit)
                            .Where(s => s.RandomizationId == obj.RandomizationId && s.DeletedDate == null).Select(z => z.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode).ToList();
                            if (treatment.Count > 0)
                                iWRSEmailModel.Treatment = string.Join(",", treatment.Distinct());
                        }
                        else
                        {
                            var treatment = _context.SupplyManagementKITSeriesDetail.Include(x => x.SupplyManagementKITSeries).Include(z => z.ProjectDesignVisit).Include(x => x.PharmacyStudyProductType).ThenInclude(z => z.ProductType)
                            .Where(s => s.RandomizationId == obj.RandomizationId && s.DeletedDate == null).Select(z => z.PharmacyStudyProductType.ProductType.ProductTypeCode).ToList();
                            if (treatment.Count > 0)
                                iWRSEmailModel.Treatment = string.Join(",", treatment.Distinct());
                        }
                    }
                    if (obj.RandomizationId > 0)
                    {
                        iWRSEmailModel.RandomizationNo = _context.Randomization.Where(x => x.Id == obj.RandomizationId).FirstOrDefault().RandomizationNumber;
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
