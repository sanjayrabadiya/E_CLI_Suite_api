using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Configuration;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Master;
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
using System.Web.Http.ModelBinding;

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

        public List<SupplyManagementKITGridDto> GetKITList(bool isDeleted, int ProjectId, int siteId)
        {
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                         Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == ProjectId
                         && s.RoleId == _jwtTokenAccesser.RoleId);
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == ProjectId).FirstOrDefault();

            var data = _context.SupplyManagementKITDetail.
                Include(s => s.SupplyManagementShipment).
                ThenInclude(s => s.SupplyManagementRequest).
                Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.SupplyManagementKIT.ProjectId == ProjectId).
                ProjectTo<SupplyManagementKITGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            if (siteId > 0 && data.Count > 0)
            {
                data = data.Where(s => s.SupplyManagementShipment != null && s.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId).ToList();
            }
            data.ForEach(x =>
            {
                if (x.RandomizationId > 0)
                    x.RandomizationNo = _context.Randomization.Where(z => z.Id == x.RandomizationId).FirstOrDefault().RandomizationNumber;
                if (x.ToSiteId > 0)
                {
                    x.SiteCode = _context.Project.Where(z => z.Id == x.ToSiteId).FirstOrDefault().ProjectCode;
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
                var kit = _context.SupplyManagementKIT.Where(s => s.Id == x.SupplyManagementKITId).FirstOrDefault();

                if (kit != null && kit.ProductReceiptId > 0)
                {
                    var productreciept = _context.ProductVerification.Include(a => a.ProductReceipt).Where(a => a.ProductReceiptId == kit.ProductReceiptId).FirstOrDefault();
                    if (productreciept != null)
                    {
                        x.ExpiryDate = productreciept.RetestExpiryDate;
                        x.LotBatchNo = productreciept.BatchLotNumber;
                    }
                }
                x.ProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : x.ProductTypeName;

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
                            SiteCode = x.SupplyManagementKIT.Site.ProjectCode,
                            RetestExpiry = x.SupplyManagementKIT.ProductReceiptId > 0 ? _context.ProductVerification.Where(s => s.ProductReceiptId == x.SupplyManagementKIT.ProductReceiptId).FirstOrDefault().RetestExpiryDate : null,
                            LotBatchNo = x.SupplyManagementKIT.ProductReceiptId > 0 ? _context.ProductVerification.Where(s => s.ProductReceiptId == x.SupplyManagementKIT.ProductReceiptId).FirstOrDefault().BatchLotNumber : "",
                            Dose = x.SupplyManagementKIT.Dose,
                            IsRetension = x.IsRetension,
                            IsDisable = true
                        }).OrderByDescending(x => x.KitNo).ToList();
                foreach (var item in data)
                {

                    var refrencetype = Enum.GetValues(typeof(KitStatus))
                                        .Cast<KitStatus>().Select(e => new DropDownStudyDto
                                        {
                                            Id = Convert.ToInt16(e),
                                            Value = Convert.ToInt16(e) == 6 ? e.GetDescription() + " (With issue)" :
                                            Convert.ToInt16(e) == 7 ? e.GetDescription() + " (Without issue) " : e.GetDescription()
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
                            SiteCode = x.SiteId > 0 ? _context.Project.Where(z => z.Id == x.SiteId).FirstOrDefault().ProjectCode : "",
                            KitValidity = x.KitExpiryDate,
                            IsRetension = x.IsRetension,
                            IsDisable = true
                        }).OrderByDescending(x => x.KitNo).ToList();


                foreach (var item in data)
                {

                    var refrencetype = Enum.GetValues(typeof(KitStatus))
                                        .Cast<KitStatus>().Select(e => new DropDownStudyDto
                                        {
                                            Id = Convert.ToInt16(e),
                                            Value = Convert.ToInt16(e) == 6 ? e.GetDescription() + " (With issue)" :
                                            Convert.ToInt16(e) == 7 ? e.GetDescription() + " (Without issue) " : e.GetDescription()
                                        }).Where(x => x.Id == 4 || x.Id == 5 || x.Id == 6 || x.Id == 7).ToList();
                    item.StatusList = refrencetype;
                }
            }

            return data;
        }
        public string GenerateKitNo(SupplyManagementKitNumberSettings kitsettings, int noseriese)
        {
            var isnotexist = false;
            string kitno1 = string.Empty;
            while (!isnotexist)
            {
                var kitno = kitsettings.Prefix + kitsettings.KitNoseries.ToString().PadLeft((int)kitsettings.KitNumberLength, '0');
                if (!string.IsNullOrEmpty(kitno))
                {
                    ++kitsettings.KitNoseries;
                    _context.SupplyManagementKitNumberSettings.Update(kitsettings);
                    _context.Save();
                    var data = _context.SupplyManagementKITDetail.Where(x => x.KitNo == kitno).FirstOrDefault();
                    if (data == null)
                    {
                        isnotexist = true;
                        kitno1 = kitno;
                        break;

                    }


                }
            }
            return kitno1;
        }

        public int GetAvailableRemainingkitCount(int ProjectId, int PharmacyStudyProductTypeId, int productReceiptId)
        {
            var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == productReceiptId).FirstOrDefault();
            if (productreciept == null)
                return 0;
            var verification = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.BatchLotNumber == productreciept.BatchLotNumber
            && x.ProductReceipt.ProjectId == ProjectId && x.DeletedDate == null
            && x.ProductReceipt.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId && x.ProductReceipt.Status == ProductVerificationStatus.Approved).Select(x => x.ProductReceiptId).ToList();
            if (verification.Count == 0)
                return 0;
            var RemainingQuantity = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == ProjectId
                 && x.ProductReceipt.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                 && verification.Contains(x.ProductReceiptId)
                 && x.ProductReceipt.Status == ProductVerificationStatus.Approved)
                 .Sum(z => z.RemainingQuantity);
            if (RemainingQuantity > 0)
            {
                var approvedQty = _context.SupplyManagementKITDetail.Where(x => x.DeletedDate == null
                 && x.SupplyManagementKIT.ProjectId == ProjectId && x.SupplyManagementKIT.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                 && verification.Contains(x.SupplyManagementKIT.ProductReceiptId)
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
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
             Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == projectId
             && s.RoleId == _jwtTokenAccesser.RoleId);

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

            var randdata = _context.Randomization.Include(s => s.Project).Where(x => x.Id == obj.RandomizationId).FirstOrDefault();
            if (randdata == null)
            {
                obj.ExpiryMesage = "Randomization not found";
                return obj;
            }
            //if (randdata.Project != null && (randdata.Project.Status == Helper.MonitoringSiteStatus.CloseOut || randdata.Project.Status == Helper.MonitoringSiteStatus.Terminated || randdata.Project.Status == Helper.MonitoringSiteStatus.OnHold || randdata.Project.Status == Helper.MonitoringSiteStatus.Rejected))
            //{
            //    obj.ExpiryMesage = "Selected site is " + randdata.Project.Status.GetDescription() + "!";
            //    return obj;
            //}
            if (randdata != null && randdata.PatientStatusId != ScreeningPatientStatus.Screening && randdata.PatientStatusId != ScreeningPatientStatus.OnTrial)
            {
                obj.ExpiryMesage = "Patient status is not eligible for randomization";
                return obj;
            }
            var screeningentry = _context.ScreeningEntry.Where(x => x.RandomizationId == obj.RandomizationId).FirstOrDefault();
            if (screeningentry != null)
            {
                var screeningvisit = _context.ScreeningVisit.Where(x => x.ScreeningEntryId == screeningentry.Id && x.ProjectDesignVisitId == obj.ProjectDesignVisitId && x.Status == ScreeningVisitStatus.Missed).FirstOrDefault();
                if (screeningvisit != null)
                {
                    obj.ExpiryMesage = "Patient Visit status is not eligible for randomization";
                    return obj;
                }
            }


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
                var kitdata = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementKIT).Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                                    x.DeletedDate == null
                                    && x.SupplyManagementKIT.ProjectDesignVisitId == visit.ProjectDesignVisitId
                                    && x.SupplyManagementKIT.ProjectId == obj.ParentProjectId
                                    && x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode == visit.Value
                                    && x.SupplyManagementKIT.DeletedDate == null
                                    && !x.IsRetension
                                    && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ProjectId
                                    && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                                    && x.RandomizationId == null).OrderBy(x => x.Id).ToList();

                kitcount = kitdata.Count;
                if (kitdata == null || kitdata.Count == 0)
                    return obj;


                foreach (var kit in kitdata)
                {
                    if (string.IsNullOrEmpty(obj.KitNo))
                    {
                        var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == kit.SupplyManagementKIT.ProductReceiptId).FirstOrDefault();
                        if (productreciept != null)
                        {
                            var expiry = Convert.ToDateTime(productreciept.RetestExpiryDate).Date;
                            var date = expiry.AddDays((int)kit.SupplyManagementKIT.Days);
                            var currentdate = DateTime.Now.Date;
                            if (currentdate.Date < date.Date)
                            {
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
                                    SupplyManagementKITDetailId = kit.Id,
                                    SupplyManagementShipmentId = kit.SupplyManagementShipmentId,
                                    IpAddress = _jwtTokenAccesser.IpAddress,
                                    TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")

                                };
                                InsertKitRandomizationDetail(supplyManagementVisitKITDetailDto);
                                _context.Save();
                                obj.KitNo = kit.KitNo;
                                SendRandomizationThresholdEMail(obj, kitcount);
                                return obj;
                            }
                            else
                            {
                                obj.ExpiryMesage = "Kit is expired";
                                return obj;
                            }
                        }
                    }
                }
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

                var currentdate = DateTime.Now.Date;
                var kitExpiryDate = kitSequencedata.SupplyManagementKITSeries.KitExpiryDate;
                if (currentdate < kitExpiryDate.Value.Date)
                {
                    kitSequencedata.RandomizationId = obj.RandomizationId;
                    _context.SupplyManagementKITSeriesDetail.Update(kitSequencedata);

                    var supplyManagementVisitKITDetailDto = new SupplyManagementVisitKITSequenceDetailDto
                    {
                        RandomizationId = obj.RandomizationId,
                        ProjectDesignVisitId = kitSequencedata.ProjectDesignVisitId,
                        KitNo = kitSequencedata.SupplyManagementKITSeries.KitNo,
                        ProductCode = visit.Value,
                        SupplyManagementKITSeriesdetailId = kitSequencedata.Id,
                        SupplyManagementShipmentId = kitSequencedata.SupplyManagementKITSeries.SupplyManagementShipmentId,
                        IpAddress = _jwtTokenAccesser.IpAddress,
                        TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone")
                    };
                    InsertKitSequenceRandomizationDetail(supplyManagementVisitKITDetailDto);
                    obj.KitNo = kitSequencedata.SupplyManagementKITSeries.KitNo;
                    _context.Save();
                }
                else
                {
                    obj.ExpiryMesage = "Kit is expired";
                    return obj;
                }

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
                if (x.SupplyManagementShipmentId > 0)
                {
                    var shipment = _context.SupplyManagementShipment.Include(a => a.SupplyManagementRequest).ThenInclude(s => s.FromProject).Where(s => s.Id == x.SupplyManagementShipmentId).FirstOrDefault();
                    if (shipment != null)
                    {
                        if (shipment.SupplyManagementRequest.IsSiteRequest)
                        {
                            x.FromProjectCode = shipment.SupplyManagementRequest.FromProject.ProjectCode;

                            if (shipment.SupplyManagementRequest.ToProjectId > 0)
                            {
                                var project = _context.Project.Where(a => a.Id == shipment.SupplyManagementRequest.ToProjectId).FirstOrDefault();
                                if (project != null)
                                {
                                    x.ToProjectCode = project.ProjectCode;
                                }
                            }

                        }
                        else
                        {
                            x.FromProjectCode = shipment.SupplyManagementRequest.FromProject.ProjectCode;
                            var project = _context.Project.Where(a => a.Id == shipment.SupplyManagementRequest.FromProjectId).FirstOrDefault();
                            if (project != null)
                            {
                                var parentproject = _context.Project.Where(a => a.Id == project.ParentProjectId).FirstOrDefault();
                                if (parentproject != null)
                                {
                                    x.ToProjectCode = parentproject.ProjectCode;

                                }
                            }
                        }
                    }
                }
            });

            return data;
        }

        public List<SupplyManagementKITReturnGridDto> GetKitReturnList(int projectId, KitStatusRandomization kitType, int? siteId, int? visitId, int? randomizationId)
        {
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                         Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == projectId
                         && s.RoleId == _jwtTokenAccesser.RoleId);

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == projectId).FirstOrDefault();
            if (setting == null)
                return new List<SupplyManagementKITReturnGridDto>();
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
                                                                  ProductTypeName = setting.IsBlindedStudy == true && isShow ? "" : x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode,
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
                                                                  SiteId = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? (x.SupplyManagementShipment.SupplyManagementRequest.IsSiteRequest == false ? x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId : x.SupplyManagementShipment.SupplyManagementRequest.ToProjectId) : 0,
                                                                  SiteCode = x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest != null ? (x.SupplyManagementShipment.SupplyManagementRequest.IsSiteRequest == false ? x.SupplyManagementShipment.SupplyManagementRequest.FromProject.ProjectCode : x.SupplyManagementShipment.SupplyManagementRequest.ToProject.ProjectCode) : "",
                                                                  ActionBy = x.ReturnBy,
                                                                  ActionDate = x.ReturnDate,
                                                                  IsUnUsed = x.IsUnUsed,
                                                                  Isdisable = setting.IsBarcodeScan,
                                                                  Barcode = x.Barcode
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
                            x.IpAddressReturn = returndata.IpAddress;
                            x.TimeZoneReturn = returndata.TimeZone;
                        }
                        var returnverificationdata = _context.SupplyManagementKITReturnVerification.Include(x => x.AuditReason).Where(z => z.SupplyManagementKITDetailId == x.SupplyManagementKITDetailId).FirstOrDefault();
                        if (returnverificationdata != null)
                        {
                            x.SupplyManagementKITReturnVerificationId = returnverificationdata.Id;
                            x.ReturnVerificationDate = returnverificationdata.CreatedDate;
                            x.ReturnVerificationBy = _context.Users.Where(z => z.Id == returnverificationdata.CreatedBy).FirstOrDefault().UserName;
                            x.ReturnVerificationReasonOth = returnverificationdata.ReasonOth;
                            x.ReturnVerificationReason = returnverificationdata.AuditReason.ReasonName;
                            x.IpAddressVerification = returnverificationdata.IpAddress;
                            x.TimeZoneVerification = returnverificationdata.TimeZone;
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
                                                                 Isdisable = setting.IsBarcodeScan,
                                                                 Barcode = x.Barcode
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
                            x.IpAddressReturn = returndata.IpAddress;
                            x.TimeZoneReturn = returndata.TimeZone;
                        }
                        var returnverificationdata = _context.SupplyManagementKITReturnVerificationSeries.Include(x => x.AuditReason).Where(z => z.SupplyManagementKITSeriesId == x.SupplyManagementKITSeriesId).FirstOrDefault();
                        if (returnverificationdata != null)
                        {
                            x.SupplyManagementKITReturnVerificationId = returnverificationdata.Id;
                            x.ReturnVerificationDate = returnverificationdata.CreatedDate;
                            x.ReturnVerificationBy = _context.Users.Where(z => z.Id == returnverificationdata.CreatedBy).FirstOrDefault().UserName;
                            x.ReturnVerificationReasonOth = returnverificationdata.ReasonOth;
                            x.ReturnVerificationReason = returnverificationdata.AuditReason.ReasonName;
                            x.IpAddressVerification = returnverificationdata.IpAddress;
                            x.TimeZoneVerification = returnverificationdata.TimeZone;
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

        public string ValidateReturnAllsave(SupplyManagementKITReturnDtofinal data)
        {
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == data.ProjectId).FirstOrDefault();
            if (setting.KitCreationType == KitCreationType.KitWise)
            {
                if (data != null && data.list.Count > 0)
                {
                    foreach (var obj in data.list)
                    {
                        var datakit = _context.SupplyManagementKITDetail.Include(s => s.SupplyManagementShipment).ThenInclude(s => s.SupplyManagementRequest).ThenInclude(s => s.FromProject).
                            Where(x => x.Id == obj.SupplyManagementKITDetailId).FirstOrDefault();
                        if (datakit != null && datakit.SupplyManagementShipment != null && datakit.SupplyManagementShipment.SupplyManagementRequest != null && datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject != null)
                        {
                            if (datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status == Helper.MonitoringSiteStatus.CloseOut || datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status == Helper.MonitoringSiteStatus.Terminated || datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status == Helper.MonitoringSiteStatus.OnHold || datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status == Helper.MonitoringSiteStatus.Rejected)
                            {
                                return "You can't return record, selected recode which have site is " + datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status.GetDescription() + "!";
                            }
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
                        var datakit = _context.SupplyManagementKITSeries.Include(s => s.SupplyManagementShipment).ThenInclude(s => s.SupplyManagementRequest).ThenInclude(s => s.FromProject).Where(x => x.Id == obj.SupplyManagementKITSeriesId).FirstOrDefault();
                        if (datakit != null && datakit.SupplyManagementShipment != null && datakit.SupplyManagementShipment.SupplyManagementRequest != null && datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject != null)
                        {
                            if (datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status == Helper.MonitoringSiteStatus.CloseOut || datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status == Helper.MonitoringSiteStatus.Terminated || datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status == Helper.MonitoringSiteStatus.OnHold || datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status == Helper.MonitoringSiteStatus.Rejected)
                            {
                                return "You can't return record, selected recode which have site is " + datakit.SupplyManagementShipment.SupplyManagementRequest.FromProject.Status.GetDescription() + "!";
                            }
                        }
                    }
                }
            }
            return "";
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
                            returnkit.IpAddress = _jwtTokenAccesser.IpAddress;
                            returnkit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                            _context.SupplyManagementKITReturn.Add(returnkit);

                            SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                            history.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                            history.SupplyManagementShipmentId = datakit.SupplyManagementShipmentId;
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
                            returnkit.IpAddress = _jwtTokenAccesser.IpAddress;
                            returnkit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                            _context.SupplyManagementKITReturnSeries.Add(returnkit);

                            SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                            history.SupplyManagementKITSeriesId = obj.SupplyManagementKITSeriesId;
                            history.SupplyManagementShipmentId = datakit.SupplyManagementShipmentId;
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
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                         Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == projectId
                         && s.RoleId == _jwtTokenAccesser.RoleId);
            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == projectId).FirstOrDefault();
            if (setting == null)
                return new List<SupplyManagementKITDiscardGridDto>();

            var data = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).ThenInclude(x => x.FromProject).Include(x => x.SupplyManagementKIT)
                                                         .ThenInclude(x => x.PharmacyStudyProductType)
                                                         .ThenInclude(x => x.ProductType)
                                                         .Where(x => x.SupplyManagementKIT.ProjectId == projectId
                                                          && x.DeletedDate == null

                                                          ).Select(x => new SupplyManagementKITDiscardGridDto
                                                          {
                                                              KitNo = x.KitNo,
                                                              ProjectDesignVisitId = x.SupplyManagementKIT.ProjectDesignVisitId,
                                                              ProductTypeName = setting != null && setting.IsBlindedStudy == true && isShow ? "" : x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode,
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
                                                              IsUnUsed = x.IsUnUsed,
                                                              Isdisable = setting.IsBarcodeScan,
                                                              Barcode = x.Barcode
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
                        x.IpAddress = _jwtTokenAccesser.IpAddress;
                        x.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
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
                        returnkit.IpAddress = _jwtTokenAccesser.IpAddress;
                        returnkit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                        _context.SupplyManagementKITDiscard.Add(returnkit);

                        SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                        history.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                        history.SupplyManagementShipmentId = datakit.SupplyManagementShipmentId;
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
                        returnkit.IpAddress = _jwtTokenAccesser.IpAddress;
                        returnkit.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                        _context.SupplyManagementKITDiscard.Add(returnkit);

                        SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                        history.SupplyManagementKITDetailId = obj.SupplyManagementKITDetailId;
                        history.SupplyManagementShipmentId = datakit.SupplyManagementShipmentId;
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
                supplyManagementKITReturnVerification.IpAddress = _jwtTokenAccesser.IpAddress;
                supplyManagementKITReturnVerification.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                _context.SupplyManagementKITReturnVerification.Add(supplyManagementKITReturnVerification);

                SupplyManagementKITDetailHistory history = new SupplyManagementKITDetailHistory();
                history.SupplyManagementKITDetailId = data.SupplyManagementKITDetailId;
                history.SupplyManagementShipmentId = datakit.SupplyManagementShipmentId;
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
                supplyManagementKITReturnVerification.IpAddress = _jwtTokenAccesser.IpAddress;
                supplyManagementKITReturnVerification.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
                _context.SupplyManagementKITReturnVerificationSeries.Add(supplyManagementKITReturnVerification);

                SupplyManagementKITSeriesDetailHistory history = new SupplyManagementKITSeriesDetailHistory();
                history.SupplyManagementKITSeriesId = data.SupplyManagementKITSeriesId;
                history.SupplyManagementShipmentId = datakit.SupplyManagementShipmentId;
                history.Status = data.Status;
                history.RoleId = _jwtTokenAccesser.RoleId;
                _context.SupplyManagementKITSeriesDetailHistory.Add(history);
                _context.Save();
            }
        }

        public List<SupplyManagementKITSeriesGridDto> GetKITSeriesList(bool isDeleted, int ProjectId, int siteId)
        {
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                         Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == ProjectId
                         && s.RoleId == _jwtTokenAccesser.RoleId);

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == ProjectId).FirstOrDefault();

            var data = _context.SupplyManagementKITSeries
                .Include(s => s.SupplyManagementShipment)
                .ThenInclude(s => s.SupplyManagementRequest)
                .Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementKITSeriesGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            if (siteId > 0 && data.Count > 0)
            {
                data = data.Where(x => x.SupplyManagementShipment != null && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == siteId).ToList();
            }
            data.ForEach(x =>
            {
                if (x.ToSiteId > 0)
                {
                    x.SiteCode = _context.Project.Where(z => z.Id == x.ToSiteId).FirstOrDefault().ProjectCode;
                }
                else if (x.SiteId > 0)
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
                x.TreatmentType = setting != null && setting.IsBlindedStudy == true && isShow ? "" : x.TreatmentType;
            });
            return data;
        }

        public List<SupplyManagementKITSeriesDetailGridDto> GetKITSeriesDetailList(int id)
        {
            var supplyManagementKITSeries = _context.SupplyManagementKITSeries.Where(s => s.Id == id).FirstOrDefault();

            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                         Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == supplyManagementKITSeries.ProjectId
                         && s.RoleId == _jwtTokenAccesser.RoleId);

            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == supplyManagementKITSeries.ProjectId).FirstOrDefault();

            var data = _context.SupplyManagementKITSeriesDetail.Where(x => x.SupplyManagementKITSeriesId == id).
                   ProjectTo<SupplyManagementKITSeriesDetailGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                if (x.ProductReceiptId > 0)
                {
                    var productreciept = _context.ProductVerification.Include(a => a.ProductReceipt).Where(a => a.ProductReceiptId == x.ProductReceiptId).FirstOrDefault();
                    if (productreciept != null)
                    {
                        x.ExpiryDate = productreciept.RetestExpiryDate;
                        x.LotBatchNo = productreciept.BatchLotNumber;
                    }
                }
                x.ProductType = setting.IsBlindedStudy == true && isShow ? "" : x.ProductType;
            });
            return data;
        }
        public List<SupplyManagementKITSeriesDetailHistoryGridDto> GetKITSeriesDetailHistoryList(int id)
        {
            var data = _context.SupplyManagementKITSeriesDetailHistory.Where(x => x.SupplyManagementKITSeriesId == id).
                   ProjectTo<SupplyManagementKITSeriesDetailHistoryGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                x.RoleName = _context.SecurityRole.Where(z => z.Id == x.RoleId).FirstOrDefault().RoleName;


                if (x.SupplyManagementShipmentId > 0)
                {
                    var shipment = _context.SupplyManagementShipment.Include(a => a.SupplyManagementRequest).ThenInclude(s => s.FromProject).Where(s => s.Id == x.SupplyManagementShipmentId).FirstOrDefault();
                    if (shipment != null)
                    {
                        if (shipment.SupplyManagementRequest.IsSiteRequest)
                        {
                            x.FromProjectCode = shipment.SupplyManagementRequest.FromProject.ProjectCode;

                            if (shipment.SupplyManagementRequest.ToProjectId > 0)
                            {
                                var project = _context.Project.Where(a => a.Id == shipment.SupplyManagementRequest.ToProjectId).FirstOrDefault();
                                if (project != null)
                                {
                                    x.ToProjectCode = project.ProjectCode;
                                }
                            }

                        }
                        else
                        {
                            x.FromProjectCode = shipment.SupplyManagementRequest.FromProject.ProjectCode;
                            var project = _context.Project.Where(a => a.Id == shipment.SupplyManagementRequest.FromProjectId).FirstOrDefault();
                            if (project != null)
                            {
                                var parentproject = _context.Project.Where(a => a.Id == project.ParentProjectId).FirstOrDefault();
                                if (parentproject != null)
                                {
                                    x.ToProjectCode = parentproject.ProjectCode;

                                }
                            }
                        }
                    }
                }


            });
            return data;
        }
        public string CheckAvailableQtySequenceKit(SupplyManagementKITSeriesDto supplyManagementKITSeriesDto)
        {
            if (supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Count > 0)
            {
                var data = supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.GroupBy(x => x.PharmacyStudyProductTypeId).Select(x => new
                {
                    Id = x.Key,
                    ProductReceiptId = x.FirstOrDefault().ProductReceiptId
                }).ToList();

                if (data.Count > 0)
                {
                    foreach (var item in data)
                    {
                        var Total = supplyManagementKITSeriesDto.SupplyManagementKITSeriesDetail.Where(x => x.PharmacyStudyProductTypeId == item.Id).Sum(z => z.NoOfImp * supplyManagementKITSeriesDto.NoofPatient);
                        var availableqty = GetAvailableRemainingkitSequenceCount(supplyManagementKITSeriesDto.ProjectId, item.Id, item.ProductReceiptId);
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
                        supplyManagementUnblindTreatment.IpAddress = _jwtTokenAccesser.IpAddress;
                        supplyManagementUnblindTreatment.TimeZone = _jwtTokenAccesser.GetHeader("clientTimeZone");
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
            var isShow = _context.SupplyManagementKitNumberSettingsRole.
                        Include(s => s.SupplyManagementKitNumberSettings).Any(s => s.DeletedDate == null && s.SupplyManagementKitNumberSettings.ProjectId == projectId
                        && s.RoleId == _jwtTokenAccesser.RoleId);


            var setting = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == projectId).FirstOrDefault();

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
                    x.IpAddress = unblind.IpAddress;
                    x.TimeZone = unblind.TimeZone;

                    if (setting != null && setting.KitCreationType == KitCreationType.KitWise)
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
            });

            return data;

        }
        public int GetAvailableRemainingkitSequenceCount(int ProjectId, int PharmacyStudyProductTypeId, int productReceiptId)
        {
            var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == productReceiptId).FirstOrDefault();
            if (productreciept == null)
                return 0;
            var verification = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceipt.ProjectId == ProjectId && x.BatchLotNumber == productreciept.BatchLotNumber && x.ProductReceipt.Status == ProductVerificationStatus.Approved).Select(x => x.ProductReceiptId).ToList();
            if (verification.Count == 0)
                return 0;

            var RemainingQuantity = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == ProjectId
                 && x.ProductReceipt.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                 && verification.Contains(x.ProductReceiptId)
                 && x.ProductReceipt.Status == ProductVerificationStatus.Approved)
                 .Sum(z => z.RemainingQuantity);
            if (RemainingQuantity > 0)
            {
                var approvedQty = _context.SupplyManagementKITSeriesDetail.Include(x => x.SupplyManagementKITSeries).Where(x => x.DeletedDate == null
                   && x.SupplyManagementKITSeries.ProjectId == ProjectId && x.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId
                   && verification.Contains(x.ProductReceiptId)
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

        public string CheckExpiryDate(SupplyManagementKITDto supplyManagementUploadFileDto)
        {
            var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == supplyManagementUploadFileDto.ProductReceiptId).FirstOrDefault();
            if (productreciept == null)
                return "Product receipt not found";

            var currentdate = DateTime.Now.Date;
            var date = currentdate.AddDays(supplyManagementUploadFileDto.Days);
            if (Convert.ToDateTime(productreciept.RetestExpiryDate).Date < date.Date)
            {
                return "Product is expired";
            }

            return "";
        }

        public List<DropDownDto> GetDoseListByProductRecieptId(int ProjectId, int PharmacyStudyProductTypeId, int productReceiptId)
        {
            var productreciept = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.ProductReceiptId == productReceiptId).FirstOrDefault();
            if (productreciept == null)
                return new List<DropDownDto>();
            var verification = _context.ProductVerification.Include(x => x.ProductReceipt).Where(x => x.BatchLotNumber == productreciept.BatchLotNumber
                                 && x.ProductReceipt.ProjectId == ProjectId && x.ProductReceipt.PharmacyStudyProductTypeId == PharmacyStudyProductTypeId && x.DeletedDate == null && x.ProductReceipt.Status == ProductVerificationStatus.Approved)
                                .Select(x => new DropDownDto
                                {
                                    Code = x.Dose.ToString(),
                                    Value = x.Dose.ToString()
                                }).Distinct().ToList();


            return verification;
        }

        public string GenerateKitBarcode(SupplyManagementKITDetail supplyManagementKITDetail)
        {
            string barcode = string.Empty;
            var detail = _context.SupplyManagementKIT.Include(s => s.Project).Include(s => s.ProjectDesignVisit)
                  .Include(s => s.PharmacyStudyProductType).ThenInclude(s => s.ProductType).Where(s => s.Id == supplyManagementKITDetail.SupplyManagementKITId).FirstOrDefault();
            if (detail != null)
            {
                barcode = detail.Project.ProjectCode + supplyManagementKITDetail.KitNo;
            }
            return barcode;
        }
        public List<ProductRecieptBarcodeGenerateGridDto> GetkitBarcodeDetail(int id, string type)
        {
            List<ProductRecieptBarcodeGenerateGridDto> lst = new List<ProductRecieptBarcodeGenerateGridDto>();
            if (type == "Kit")
            {
                var kit = _context.SupplyManagementKITDetail.Include(s => s.SupplyManagementKIT).Where(x => x.Id == id).FirstOrDefault();

                var pharmacyBarcodeConfig = _context.PharmacyBarcodeConfig.Include(s => s.BarcodeDisplayInfo).Where(x => x.ProjectId == kit.SupplyManagementKIT.ProjectId && x.BarcodeModuleType == BarcodeModuleType.kit && x.DeletedBy == null).FirstOrDefault();


                var sublst = new ProductRecieptBarcodeGenerateGridDto
                {
                    BarcodeString = kit.Barcode,
                    BarcodeType = pharmacyBarcodeConfig.BarcodeType.GetDescription(),
                    DisplayValue = pharmacyBarcodeConfig.DisplayValue,
                    DisplayInformationLength = pharmacyBarcodeConfig.DisplayInformationLength,
                    FontSize = pharmacyBarcodeConfig.FontSize,
                    FontSizeStr = pharmacyBarcodeConfig.FontSize + "px",
                    BarcodeDisplayInfo = new PharmacyBarcodeDisplayInfo[pharmacyBarcodeConfig.BarcodeDisplayInfo.Count()]
                };
                int index = 0;
                foreach (var subitem in pharmacyBarcodeConfig.BarcodeDisplayInfo)
                {
                    var tablefieldName = _context.TableFieldName.Where(s => s.Id == subitem.TableFieldNameId).FirstOrDefault();
                    if (tablefieldName != null)
                    {
                        sublst.BarcodeDisplayInfo[index] = new PharmacyBarcodeDisplayInfo();
                        sublst.BarcodeDisplayInfo[index].Alignment = subitem.Alignment;
                        sublst.BarcodeDisplayInfo[index].DisplayInformation = GetKitColumnValue(id, tablefieldName.FieldName, type);
                        sublst.BarcodeDisplayInfo[index].OrderNumber = subitem.OrderNumber;
                        sublst.BarcodeDisplayInfo[index].IsSameLine = subitem.IsSameLine;
                        index++;
                    }
                }
                lst.Add(sublst);
            }
            if (type == "KitPack")
            {
                var kit = _context.SupplyManagementKITSeries.Where(x => x.Id == id).FirstOrDefault();
                var pharmacyBarcodeConfig = _context.PharmacyBarcodeConfig.Include(s => s.BarcodeDisplayInfo).Where(x => x.ProjectId == kit.ProjectId && x.BarcodeModuleType == BarcodeModuleType.kit && x.DeletedBy == null).FirstOrDefault();
                var sublst = new ProductRecieptBarcodeGenerateGridDto
                {
                    BarcodeString = kit.Barcode,
                    BarcodeType = pharmacyBarcodeConfig.BarcodeType.GetDescription(),
                    DisplayValue = pharmacyBarcodeConfig.DisplayValue,
                    DisplayInformationLength = pharmacyBarcodeConfig.DisplayInformationLength,
                    FontSize = pharmacyBarcodeConfig.FontSize,
                    FontSizeStr = pharmacyBarcodeConfig.FontSize + "px",
                    BarcodeDisplayInfo = new PharmacyBarcodeDisplayInfo[pharmacyBarcodeConfig.BarcodeDisplayInfo.Count()]
                };
                int index = 0;
                foreach (var subitem in pharmacyBarcodeConfig.BarcodeDisplayInfo)
                {
                    var tablefieldName = _context.TableFieldName.Where(s => s.Id == subitem.TableFieldNameId).FirstOrDefault();
                    if (tablefieldName != null)
                    {
                        sublst.BarcodeDisplayInfo[index] = new PharmacyBarcodeDisplayInfo();
                        sublst.BarcodeDisplayInfo[index].Alignment = subitem.Alignment;
                        sublst.BarcodeDisplayInfo[index].DisplayInformation = GetKitColumnValue(id, tablefieldName.FieldName, type);
                        sublst.BarcodeDisplayInfo[index].OrderNumber = subitem.OrderNumber;
                        sublst.BarcodeDisplayInfo[index].IsSameLine = subitem.IsSameLine;
                        index++;
                    }
                }
                lst.Add(sublst);
            }
            return lst;
        }
        string GetKitColumnValue(int id, string ColumnName, string type)
        {
            if (type == "Kit")
            {
                var tableRepository = _context.SupplyManagementKITDetail.Include(s => s.SupplyManagementKIT).ThenInclude(s => s.Project)
                    .Include(s => s.SupplyManagementKIT).ThenInclude(s => s.ProjectDesignVisit)
                    .Include(s => s.SupplyManagementKIT).ThenInclude(s => s.PharmacyStudyProductType).ThenInclude(s => s.ProductType).Where(x => x.Id == id).FirstOrDefault();
                if (tableRepository == null) return "";

                if (ColumnName == "ProjectId")
                    return _context.Project.Find(tableRepository.SupplyManagementKIT.ProjectId).ProjectCode;

                if (ColumnName == "SiteId" || ColumnName == "RandomizationId" || ColumnName == "InvestigatorContactId")
                {
                    if (tableRepository.RandomizationId > 0)
                    {
                        if (ColumnName == "SiteId")
                        {
                            var randomization = _context.Randomization.Include(s => s.Project).Where(s => s.Id == tableRepository.RandomizationId).FirstOrDefault();
                            return randomization != null && randomization.Project != null ? randomization.Project.ProjectCode : "";
                        }
                        if (ColumnName == "RandomizationId")
                        {
                            var randomization = _context.Randomization.Include(s => s.Project).Where(s => s.Id == tableRepository.RandomizationId).FirstOrDefault();
                            return randomization != null ? randomization.ScreeningNumber : "";
                        }
                        if (ColumnName == "InvestigatorContactId")
                        {
                            var randomization = _context.Randomization.Include(s => s.Project).ThenInclude(s => s.InvestigatorContact).Where(s => s.Id == tableRepository.RandomizationId).FirstOrDefault();
                            return randomization != null && randomization.Project.InvestigatorContact != null ? randomization.Project.InvestigatorContact.NameOfInvestigator : "";
                        }
                    }
                }

                if (ColumnName == "PharmacyStudyProductTypeId")
                    return tableRepository.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode;

                if (ColumnName == "ProjectDesignVisitId")
                    return tableRepository.SupplyManagementKIT.ProjectDesignVisit.DisplayName;

                if (ColumnName == "KitNo")
                {
                    return tableRepository.KitNo;
                }
            }

            if (type == "KitPack")
            {
                var tableRepository = _context.SupplyManagementKITSeries.Include(s => s.Project).Where(x => x.Id == id).FirstOrDefault();
                if (tableRepository == null) return "";

                if (ColumnName == "ProjectId")
                    return _context.Project.Find(tableRepository.ProjectId).ProjectCode;

                if (ColumnName == "SiteId" || ColumnName == "RandomizationId" || ColumnName == "InvestigatorContactId")
                {
                    if (tableRepository.RandomizationId > 0)
                    {
                        if (ColumnName == "SiteId")
                        {
                            var randomization = _context.Randomization.Include(s => s.Project).Where(s => s.Id == tableRepository.RandomizationId).FirstOrDefault();
                            return randomization != null && randomization.Project != null ? randomization.Project.ProjectCode : "";
                        }
                        if (ColumnName == "RandomizationId")
                        {
                            var randomization = _context.Randomization.Include(s => s.Project).Where(s => s.Id == tableRepository.RandomizationId).FirstOrDefault();
                            return randomization != null ? randomization.ScreeningNumber : "";
                        }
                        if (ColumnName == "InvestigatorContactId")
                        {
                            var randomization = _context.Randomization.Include(s => s.Project).ThenInclude(s => s.InvestigatorContact).Where(s => s.Id == tableRepository.RandomizationId).FirstOrDefault();
                            return randomization != null ? randomization.Project.InvestigatorContact.NameOfInvestigator : "";
                        }
                    }
                }

                if (ColumnName == "PharmacyStudyProductTypeId")
                    return tableRepository.TreatmentType;

                if (ColumnName == "ProjectDesignVisitId")
                {
                    var kitdetail = _context.SupplyManagementKITSeriesDetail.Include(s => s.ProjectDesignVisit).Where(s => s.SupplyManagementKITSeriesId == tableRepository.Id).OrderBy(s => s.ProjectDesignVisit.Id).Select(s => s.ProjectDesignVisit.DisplayName).ToList();
                    return kitdetail.Count > 0 ? string.Join(",", kitdetail) : "";
                }

                if (ColumnName == "KitNo")
                {
                    return tableRepository.KitNo;
                }
            }

            return "";
        }

    }
}
