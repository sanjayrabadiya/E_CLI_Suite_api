using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.SupplyManagement
{
    public class SupplyManagementShipmentRepository : GenericRespository<SupplyManagementShipment>, ISupplyManagementShipmentRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private static Random random = new Random();
        private readonly ISupplyManagementRequestRepository _supplyManagementRequestRepository;
        private readonly IUnitOfWork _uow;
        public SupplyManagementShipmentRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IMapper mapper, ISupplyManagementRequestRepository supplyManagementRequestRepository,
            IUnitOfWork uow)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _supplyManagementRequestRepository = supplyManagementRequestRepository;
            _uow = uow;
        }
        public List<SupplyManagementShipmentGridDto> GetSupplyShipmentList(bool isDeleted)
        {
            List<SupplyManagementShipmentGridDto> FinalData = new List<SupplyManagementShipmentGridDto>();
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null).
                    ProjectTo<SupplyManagementShipmentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();

            var requestdata = _context.SupplyManagementRequest.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null
                        && !data.Select(x => x.SupplyManagementRequestId).Contains(x.Id)).
                     ProjectTo<SupplyManagementShipmentGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            requestdata.ForEach(t =>
            {
                SupplyManagementShipmentGridDto obj = new SupplyManagementShipmentGridDto();
                obj = t;
                obj.Id = 0;
                obj.RequestDate = t.CreatedDate;
                obj.CreatedByUser = null;
                obj.CreatedDate = null;
                obj.ProductUnitType = t.ProductUnitType;
                var fromproject = _context.Project.Where(x => x.Id == t.FromProjectId).FirstOrDefault();
                if (fromproject != null)
                {
                    var study = _context.Project.Where(x => x.Id == fromproject.ParentProjectId).FirstOrDefault();
                    obj.StudyProjectCode = study != null ? study.ProjectCode : "";
                }
                t.SiteRequest = t.IsSiteRequest ? "Site to Site" : "Site to Study";
                FinalData.Add(obj);

            });
            return FinalData.OrderByDescending(x => x.RequestDate).ToList();
        }

        public string GenerateShipmentNo()
        {
            const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
            return new string(Enumerable.Repeat(chars, 10)
                .Select(s => s[random.Next(s.Length)]).ToArray());
        }
        public string ApprovalValidation(SupplyManagementShipmentDto supplyManagementshipmentDto)
        {
            var shipmentData = _context.SupplyManagementRequest.Include(x => x.PharmacyStudyProductType).Where(x => x.Id == supplyManagementshipmentDto.SupplyManagementRequestId).FirstOrDefault();
            if (shipmentData == null)
            {
                return "Request data not found!";
            }
            var project = _context.Project.Where(x => x.Id == shipmentData.FromProjectId).FirstOrDefault();
            if (project == null)
            {
                return "From project code not found!";
            }
            var productreciept = _context.ProductReceipt.Where(x => x.ProjectId == project.ParentProjectId
                                && x.Status == Helper.ProductVerificationStatus.Approved).FirstOrDefault();
            if (productreciept == null)
            {
                return "Product receipt is pending!";
            }
            var productVerificationDetail = _context.ProductVerificationDetail.Where(x => x.ProductReceipt.ProjectId == project.ParentProjectId
                                && x.ProductReceipt.Status == Helper.ProductVerificationStatus.Approved).FirstOrDefault();
            if (productVerificationDetail == null)
            {
                return "Product verification is pending!";
            }
            if (supplyManagementshipmentDto.Status == Helper.SupplyMangementShipmentStatus.Approved)
            {
                if (shipmentData.PharmacyStudyProductType.ProductUnitType == Helper.ProductUnitType.Kit)
                {
                    if (supplyManagementshipmentDto.ApprovedQty > _supplyManagementRequestRepository.GetAvailableRemainingKit(supplyManagementshipmentDto.SupplyManagementRequestId))
                    {

                        return "Approve qty should not greater than remaining qty!";
                    }
                    if (supplyManagementshipmentDto.Kits.Count == 0)
                    {

                        return "Please select kits!";
                    }
                }
                else
                {
                    if (!_supplyManagementRequestRepository.CheckAvailableRemainingQty(supplyManagementshipmentDto.ApprovedQty, (int)project.ParentProjectId, shipmentData.StudyProductTypeId))
                    {
                        return "Approve qty should not greater than remaining qty!";
                    }
                }
            }
            return "";
        }
        public string GetShipmentNo()
        {
            string no = string.Empty;
            bool isnotexist = false;
            while (!isnotexist)
            {
                var str = GenerateShipmentNo();
                if (!string.IsNullOrEmpty(str))
                {
                    var data = All.Where(x => x.ShipmentNo == str).FirstOrDefault();
                    if (data == null)
                    {
                        isnotexist = true;
                        no = str;
                        break;

                    }
                }
            }
            return no;
        }
        public void Assignkits(SupplyManagementRequest shipmentdata, SupplyManagementShipmentDto supplyManagementshipmentDto)
        {
            if (supplyManagementshipmentDto.Status == Helper.SupplyMangementShipmentStatus.Approved)
            {
                if (shipmentdata.PharmacyStudyProductType.ProductUnitType == Helper.ProductUnitType.Kit)
                {
                    var shipmentkitid = 0;
                    foreach (var item in supplyManagementshipmentDto.Kits)
                    {
                        var kit = _context.SupplyManagementKITDetail.Where(x => x.Id == item.Id).FirstOrDefault();
                        if (kit != null && supplyManagementshipmentDto.Id > 0)
                        {
                            shipmentkitid = kit.SupplyManagementKITId;
                            kit.Status = Helper.KitStatus.Allocated;
                            kit.SupplyManagementShipmentId = supplyManagementshipmentDto.Id;
                            _context.SupplyManagementKITDetail.Update(kit);
                            _uow.Save();
                        }
                    }

                    var kitmaster = _context.SupplyManagementKIT.Where(x => x.Id == shipmentkitid).FirstOrDefault();
                    if (kitmaster != null)
                    {
                        kitmaster.SiteId = shipmentdata.FromProjectId;
                    }
                    _context.SupplyManagementKIT.Update(kitmaster);
                    _uow.Save();
                }
            }
        }
    }
}
