using AutoMapper;
using AutoMapper.QueryableExtensions;
using ClosedXML.Excel;
using ExcelDataReader;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.ProjectRight;
using GSC.Data.Dto.SupplyManagement;
using GSC.Data.Entities.Project.Design;
using GSC.Data.Entities.SupplyManagement;
using GSC.Domain.Context;
using GSC.Respository.Configuration;
using GSC.Respository.EmailSender;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
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
    public class SupplyManagementApprovalRepository : GenericRespository<SupplyManagementApproval>, ISupplyManagementApprovalRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IMapper _mapper;

        private readonly IGSCContext _context;

        private readonly IEmailSenderRespository _emailSenderRespository;

        public SupplyManagementApprovalRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser,
            IEmailSenderRespository emailSenderRespository,
        IMapper mapper)
            : base(context)
        {


            _jwtTokenAccesser = jwtTokenAccesser;
            _mapper = mapper;
            _context = context;
            _emailSenderRespository = emailSenderRespository;
        }
        public List<SupplyManagementApprovalGridDto> GetSupplyManagementApprovalList(int projectId, bool isDeleted)
        {
            var data = All.Where(x => isDeleted ? x.DeletedDate != null : x.DeletedDate == null && x.ProjectId == projectId).
                    ProjectTo<SupplyManagementApprovalGridDto>(_mapper.ConfigurationProvider).OrderByDescending(x => x.Id).ToList();
            data.ForEach(t =>
            {
                if (t.RoleId > 0)
                {
                    t.RoleName = _context.SecurityRole.Where(x => x.Id == t.RoleId).FirstOrDefault().RoleName;
                }
                var Users = _context.SupplyManagementApprovalDetails.Where(x => x.DeletedDate == null && x.SupplyManagementApprovalId == t.Id).Select(a => a.Users.UserName).ToList();

                if (Users.Count > 0)
                {
                    t.Users = string.Join(",", Users.Distinct());
                }
                if (t.AuditReasonId > 0)
                {
                    t.AuditReasonName = _context.AuditReason.Where(s => s.Id == t.AuditReasonId).FirstOrDefault().ReasonName;
                }
            });

            return data;
        }
        public void ChildUserApprovalAdd(SupplyManagementApprovalDto obj, int id)
        {
            if (obj.SupplyManagementApprovalDetails.Count > 0)
            {
                foreach (var item in obj.SupplyManagementApprovalDetails)
                {
                    var data = _context.SupplyManagementApprovalDetails.Where(x => x.SupplyManagementApprovalId == id && x.DeletedDate == null && x.UserId == item.UserId).FirstOrDefault();
                    if (data == null)
                    {
                        SupplyManagementApprovalDetails detail = new SupplyManagementApprovalDetails();
                        detail.UserId = item.UserId;
                        detail.SupplyManagementApprovalId = id;
                        _context.SupplyManagementApprovalDetails.Add(detail);
                        _context.Save();
                    }
                    else
                    {
                        var data1 = _context.SupplyManagementApprovalDetails.Where(x => x.SupplyManagementApprovalId == id && x.DeletedDate != null && x.UserId == item.UserId).FirstOrDefault();
                        if (data1 != null)
                        {
                            data1.DeletedBy = null;
                            data1.DeletedDate = null;
                            _context.SupplyManagementApprovalDetails.Add(data1);
                            _context.Save();
                        }
                    }

                }
            }
        }

        public void DelectChildWorkflowEmailUser(SupplyManagementApprovalDto obj, int id)
        {
            var list = _context.SupplyManagementApprovalDetails.Where(s => s.DeletedDate == null && s.SupplyManagementApprovalId == id).ToList();
            if (list.Count > 0)
            {
                _context.SupplyManagementApprovalDetails.RemoveRange(list);
                _context.Save();
            }
        }

        public string Duplicate(SupplyManagementApprovalDto obj)
        {
            if (obj.Id > 0)
            {
                var data = _context.SupplyManagementApproval.Where(x => x.Id != obj.Id && x.ProjectId == obj.ProjectId && x.DeletedDate == null && x.RoleId == obj.RoleId).FirstOrDefault();
                if (data != null)
                {
                    return "already assigned for this role!";
                }
            }
            else
            {

                var data = _context.SupplyManagementApproval.Where(x => x.Id != obj.Id && x.ProjectId == obj.ProjectId && x.DeletedDate == null && x.RoleId == obj.RoleId).FirstOrDefault();
                if (data != null)
                {
                    return "already assigned for this role!";
                }


            }

            return "";
        }

        public List<DropDownDto> GetProjectRightsRoleShipmentApproval(int projectId)
        {

            var projectrights = _context.ProjectRight.Include(x => x.User).Where(x => x.ProjectId == projectId && x.DeletedDate == null)
                              .Select(x => new DropDownDto { Id = x.RoleId, Value = x.role.RoleName }).Distinct().ToList();

            return projectrights; ;

        }
        public List<DropDownDto> GetRoleUserShipmentApproval(int roleId, int projectId)
        {

            var projectrights = _context.ProjectRight.Include(x => x.User).Where(x => x.ProjectId == projectId && x.RoleId == roleId && x.DeletedDate == null)
                              .Select(x => new DropDownDto { Id = x.UserId, Value = x.User.UserName }).Distinct().ToList();

            return projectrights;

        }

        public void SendShipmentWorkflowApprovalEmail(SupplyManagementShipmentApproval supplyManagementShipmentApproval)
        {

            IWRSEmailModel iWRSEmailModel = new IWRSEmailModel();
            var request = _context.SupplyManagementRequest.Include(x => x.ProjectDesignVisit).Include(x => x.FromProject).Include(x => x.PharmacyStudyProductType).ThenInclude(x => x.ProductType).Where(x => x.Id == supplyManagementShipmentApproval.SupplyManagementRequestId).FirstOrDefault();
            if (request != null)
            {
                var emailconfiglist = _context.SupplyManagementApprovalDetails.Include(s => s.Users).Include(s => s.SupplyManagementApproval).ThenInclude(s => s.Project).Where(x => x.DeletedDate == null && x.SupplyManagementApproval.ProjectId == request.FromProject.ParentProjectId &&
                x.SupplyManagementApproval.ApprovalType == Helper.SupplyManagementApprovalType.WorkflowApproval).ToList();
                if (emailconfiglist != null && emailconfiglist.Count > 0)
                {
                    var allocation = _context.SupplyManagementKitNumberSettings.Where(x => x.DeletedDate == null && x.ProjectId == request.FromProject.ParentProjectId).FirstOrDefault();


                    if (request.PharmacyStudyProductType != null && request.PharmacyStudyProductType.ProductType != null)
                        iWRSEmailModel.ProductType = request.PharmacyStudyProductType.ProductType.ProductTypeCode;
                    if (allocation != null && allocation.IsBlindedStudy == true)
                    {
                        iWRSEmailModel.ProductType = "Blinded study";
                    }
                    iWRSEmailModel.StudyCode = _context.Project.Where(x => x.Id == request.FromProject.ParentProjectId).FirstOrDefault().ProjectCode;
                    iWRSEmailModel.RequestFromSiteCode = request.FromProject.ProjectCode;
                    var managesite = _context.ManageSite.Where(x => x.Id == request.FromProject.ManageSiteId).FirstOrDefault();
                    if (managesite != null)
                    {
                        iWRSEmailModel.RequestFromSiteName = managesite.SiteName;
                    }
                    iWRSEmailModel.RequestedBy = _jwtTokenAccesser.UserName;
                    iWRSEmailModel.RequestedQty = request.RequestQty;
                    if (request.IsSiteRequest)
                    {
                        iWRSEmailModel.RequestType = "Site to Site Request";
                        if (request.ToProjectId > 0)
                        {
                            var toproject = _context.Project.Where(x => x.Id == request.ToProjectId).FirstOrDefault();
                            if (toproject != null)
                            {
                                iWRSEmailModel.RequestToSiteCode = toproject.ProjectCode;
                                var tomanagesite = _context.ManageSite.Where(x => x.Id == toproject.ManageSiteId).FirstOrDefault();
                                if (tomanagesite != null)
                                {
                                    iWRSEmailModel.RequestToSiteName = tomanagesite.SiteName;
                                }

                            }
                            var Projectrights = _context.ProjectRight.Where(x => x.DeletedDate == null && x.ProjectId == request.ToProjectId).ToList();
                            if (Projectrights.Count > 0)
                                emailconfiglist = emailconfiglist.Where(x => Projectrights.Select(z => z.UserId).Contains(x.UserId)).ToList();

                        }
                    }
                    else
                    {
                        iWRSEmailModel.RequestType = "Site to Study Request";
                    }
                    if (request.ProjectDesignVisit != null)
                        iWRSEmailModel.Visit = request.ProjectDesignVisit.DisplayName;

                    iWRSEmailModel.ApprovedOn = Convert.ToDateTime(supplyManagementShipmentApproval.CreatedDate).ToString("dddd, dd MMMM yyyy");
                    iWRSEmailModel.ApprovedBy = supplyManagementShipmentApproval.CreatedBy > 0 ? _context.Users.Where(s => s.Id == supplyManagementShipmentApproval.CreatedBy).FirstOrDefault().UserName : "";

                    _emailSenderRespository.SendforShipmentApprovalEmailIWRS(iWRSEmailModel, emailconfiglist.Select(x => x.Users.Email).Distinct().ToList(), emailconfiglist.FirstOrDefault().SupplyManagementApproval);


                }
            }


        }
    }
}
