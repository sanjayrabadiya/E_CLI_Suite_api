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

        public SupplyManagementKITRepository(IGSCContext context,
        IMapper mapper)
            : base(context)
        {


            _mapper = mapper;
            _context = context;
        }

        public List<SupplyManagementKITGridDto> GetKITList(bool isDeleted, int ProjectId)
        {
            var data = _context.SupplyManagementKITDetail.Where(x => (isDeleted ? x.DeletedDate != null : x.DeletedDate == null) && x.SupplyManagementKIT.ProjectId == ProjectId).
                   ProjectTo<SupplyManagementKITGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(x =>
            {
                if (x.RandomizationId > 0)
                    x.RandomizationNo = _context.Randomization.Where(z => z.Id == x.RandomizationId).FirstOrDefault().RandomizationNumber;
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
            var obj = _context.SupplyManagementShipment.Where(x => x.Id == id).FirstOrDefault();
            if (obj == null)
                return new List<KitListApproved>();
            var data = new List<KitListApproved>();

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
                                    .Cast<KitStatus>().Select(e => new DropDownEnum
                                    {
                                        Id = Convert.ToInt16(e),
                                        Value = e.GetDescription()
                                    }).Where(x => x.Id == 4 || x.Id == 5 || x.Id == 6 || x.Id == 7).ToList();
                item.StatusList = refrencetype;
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
                 && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)).Sum(x => x.SupplyManagementKIT.TotalUnits);

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
        public List<SupplyManagementVisitKITDetailGridDto> GetRandomizationKitNumberAssignList(int projectId, int siteId, int id)
        {
            List<SupplyManagementVisitKITDetailGridDto> data = new List<SupplyManagementVisitKITDetailGridDto>();
            SupplyManagementUploadFileDetail supplyManagementUploadFileDetail = new SupplyManagementUploadFileDetail();

            data = _context.SupplyManagementVisitKITDetail.Where(x =>
                     x.DeletedDate == null
                     && x.ProjectDesignVisit.ProjectDesignPeriod.ProjectDesign.ProjectId == projectId
                     && x.Randomization.ProjectId == siteId
                     && x.RandomizationId == id).
                     ProjectTo<SupplyManagementVisitKITDetailGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var randomizationdata = _context.Randomization.Include(x => x.Project).Where(x => x.Id == id).FirstOrDefault();

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

            return data;
        }
        public List<DropDownDto> GetRandomizationDropdownKit(int projectid)
        {
            return _context.Randomization.Where(a => a.DeletedDate == null && a.ProjectId == projectid)
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
            SupplyManagementUploadFileDetail data = new SupplyManagementUploadFileDetail();
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

            var kitdata = _context.SupplyManagementKITDetail.Include(x => x.SupplyManagementShipment).ThenInclude(x => x.SupplyManagementRequest).Where(x =>
                              x.DeletedDate == null
                              && x.SupplyManagementKIT.ProjectDesignVisitId == visit.ProjectDesignVisitId
                              && x.SupplyManagementKIT.PharmacyStudyProductType.ProjectId == obj.ParentProjectId
                              && x.SupplyManagementKIT.PharmacyStudyProductType.ProductType.ProductTypeCode == visit.Value
                              && x.SupplyManagementShipmentId != null
                              && x.SupplyManagementKIT.DeletedDate == null
                              && x.SupplyManagementShipment.SupplyManagementRequest.FromProjectId == obj.ProjectId
                              && (x.Status == KitStatus.WithIssue || x.Status == KitStatus.WithoutIssue)
                              && x.RandomizationId == null).OrderBy(x => x.Id).FirstOrDefault();
            if (kitdata == null)
                return obj;

            kitdata.RandomizationId = obj.RandomizationId;
            kitdata.Status = KitStatus.Allocated;
            _context.SupplyManagementKITDetail.Update(kitdata);
            var supplyManagementVisitKITDetailDto = new SupplyManagementVisitKITDetailDto
            {
                RandomizationId = obj.RandomizationId,
                ProjectDesignVisitId = visit.ProjectDesignVisitId,
                KitNo = kitdata.KitNo,
                ProductCode = visit.Value,
                ReasonOth = obj.ReasonOth,
                AuditReasonId = obj.AuditReasonId,
                SupplyManagementKITDetailId = kitdata.Id
            };
            InsertKitRandomizationDetail(supplyManagementVisitKITDetailDto);
            _context.Save();
            obj.KitNo = kitdata.KitNo;

            return obj;
        }

        public void InsertKitHistory(SupplyManagementKITDetailHistory supplyManagementVisitKITDetailHistory)
        {
            _context.SupplyManagementKITDetailHistory.Add(supplyManagementVisitKITDetailHistory);
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

    }
}
