using AutoMapper.QueryableExtensions;
using DocumentFormat.OpenXml.Office2010.Excel;
using DocumentFormat.OpenXml.Office2010.ExcelAc;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.Attendance;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Attendance;
using GSC.Data.Entities.Barcode;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.LanguageSetup;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core;

namespace GSC.Respository.Barcode
{
    public class CentrifugationDetailsRepository : GenericRespository<CentrifugationDetails>, ICentrifugationDetailsRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IProjectRepository _projectRepository;
        private readonly IPKBarcodeRepository _pKBarcodeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditReasonRepository _auditReasonRepository;

        public CentrifugationDetailsRepository(IGSCContext context,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
         IProjectRepository projectRepository,

        IAuditReasonRepository auditReasonRepository,
             IUserRepository userRepository,
        IPKBarcodeRepository pKBarcodeRepository,
        IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _pKBarcodeRepository = pKBarcodeRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _userRepository = userRepository;
            _auditReasonRepository = auditReasonRepository;
            _projectRepository = projectRepository;
        }

        public List<CentrifugationDetailsGridDto> GetCentrifugationDetails(int siteId)
        {
            List<CentrifugationDetailsGridDto> listci = new List<CentrifugationDetailsGridDto>();

            _pKBarcodeRepository.All.Where(p => p.DeletedDate == null && p.SiteId == siteId && p.BarcodeString != null).ToList().ForEach(r =>
        {
            var projectDesignVariable = _projectDesignVariableRepository.All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == r.TemplateId).OrderBy(x => x.DesignOrder).FirstOrDefault();

            var exists = All.Where(x => x.PKBarcodeId == r.Id).FirstOrDefault();

            if (exists == null)
            {
                var data = _screeningTemplateValueRepository.All
                                                     .Where(x => x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.SiteId == siteId
                                                     && x.ScreeningTemplate.Status >= Helper.ScreeningTemplateStatus.Submitted
                                                     && x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == r.VolunteerId
                                                     && x.ProjectDesignVariableId == projectDesignVariable.Id)
                                                     .Select(x => new CentrifugationDetailsGridDto
                                                     {
                                                         PKBarcodeId = r.Id,
                                                         StudyCode = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                                                         SiteCode = _projectRepository.Find(siteId).ProjectCode,
                                                         RandomizationNumber = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.RandomizationNumber,
                                                         PKBarcode = r.BarcodeString,
                                                         PKActualTime = x.Value,
                                                         CentrifugationStartTime = null,
                                                         CentrifugationByUser = null,
                                                         CentrifugationOn = null,
                                                         Status = null,
                                                         ReCentrifugationByUser = null,
                                                         ReCentrifugationOn = null,
                                                         AuditReason = null,
                                                         ReasonOth = null,
                                                         MissedBy = null,
                                                         MissedOn = null
                                                     }).FirstOrDefault();
                listci.Add(data);

            }
            else
            {
                var centrifugationData = All.Where(x => x.PKBarcodeId == r.Id).FirstOrDefault();
                var data = _screeningTemplateValueRepository.All
                                                     .Where(x => x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.SiteId == siteId
                                                     && x.ScreeningTemplate.Status >= Helper.ScreeningTemplateStatus.Submitted
                                                     && x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == r.VolunteerId
                                                     && x.ProjectDesignVariableId == projectDesignVariable.Id)
                                                     .Select(x => new CentrifugationDetailsGridDto
                                                     {
                                                         PKBarcodeId = r.Id,
                                                         StudyCode = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Project.ProjectCode,
                                                         SiteCode = _projectRepository.Find(siteId).ProjectCode,
                                                         RandomizationNumber = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.RandomizationNumber,
                                                         PKBarcode = r.BarcodeString,
                                                         PKActualTime = x.Value,
                                                         CentrifugationStartTime = centrifugationData.CentrifugationStartTime,
                                                         CentrifugationByUser = _userRepository.All.Where(x => x.Id == centrifugationData.CentrifugationBy).FirstOrDefault().UserName,
                                                         CentrifugationOn = centrifugationData.CentrifugationOn,
                                                         Status = centrifugationData.Status.GetDescription(),
                                                         ReCentrifugationByUser = centrifugationData.ReCentrifugationBy != null ? _userRepository.All.Where(x => x.Id == centrifugationData.ReCentrifugationBy).FirstOrDefault().UserName : null,
                                                         ReCentrifugationOn = centrifugationData.ReCentrifugationOn,
                                                         AuditReason = centrifugationData.AuditReasonId != null ? _auditReasonRepository.All.Where(x => x.Id == centrifugationData.AuditReasonId).FirstOrDefault().ReasonName : null,
                                                         ReasonOth = centrifugationData.ReasonOth,
                                                         MissedBy = centrifugationData.MissedBy != null ? _userRepository.All.Where(x => x.Id == centrifugationData.MissedBy).FirstOrDefault().UserName : null,
                                                         MissedOn = centrifugationData.MissedOn
                                                     }).FirstOrDefault();
                listci.Add(data);
            }
        });
            return listci;
        }

        public List<CentrifugationDetailsGridDto> GetCentrifugationDetailsByPKBarcode(string PkBarcodeString)
        {
            List<CentrifugationDetailsGridDto> listci = new List<CentrifugationDetailsGridDto>();
             _pKBarcodeRepository.All.Where(p => p.DeletedDate == null && p.BarcodeString == PkBarcodeString).ToList().ForEach(r =>
            {
                var projectDesignVariable = _projectDesignVariableRepository.All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == r.TemplateId).OrderBy(x => x.DesignOrder).FirstOrDefault();

                var exists = All.Where(x => x.PKBarcodeId == r.Id).FirstOrDefault();
                if (exists == null)
                {
                    var data = _screeningTemplateValueRepository.All
                                                         .Where(x => x.ScreeningTemplate.Status >= Helper.ScreeningTemplateStatus.Submitted
                                                         && x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == r.VolunteerId
                                                         && x.ProjectDesignVariableId == projectDesignVariable.Id)
                                                         .Select(x => new CentrifugationDetailsGridDto
                                                         {
                                                             PKBarcodeId = r.Id,
                                                             StudyCode = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                                                             SiteCode = _projectRepository.All.Where(p => p.Id == (int)x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.SiteId).FirstOrDefault().ProjectCode,
                                                             RandomizationNumber = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.RandomizationNumber,
                                                             PKBarcode = r.BarcodeString,
                                                             PKActualTime = x.Value,
                                                             CentrifugationStartTime = null,
                                                             CentrifugationByUser = null,
                                                             CentrifugationOn = null,
                                                             Status = null,
                                                             ReCentrifugationByUser = null,
                                                             ReCentrifugationOn = null,
                                                             AuditReason = null,
                                                             ReasonOth = null,
                                                             MissedBy = null,
                                                             MissedOn = null
                                                         }).FirstOrDefault();
                    listci.Add(data);
                }
            });

            return listci;
        }

        public void StartCentrifugation(List<int> ids)
        {
            foreach (var id in ids)
            {
                CentrifugationDetails r = new CentrifugationDetails();

                r.PKBarcodeId = id;
                r.CentrifugationStartTime = _jwtTokenAccesser.GetClientDate();
                r.CentrifugationBy = _jwtTokenAccesser.UserId;
                r.CentrifugationOn = _jwtTokenAccesser.GetClientDate();
                r.Status = CentrifugationFilter.Centrifugation;

                Add(r);

            }
        }

        public void StartReCentrifugation(List<int> ids)
        {
            var details = All.Where(x => ids.Contains(x.PKBarcodeId)).ToList();
            foreach (var detail in details)
            {
                detail.CentrifugationStartTime = _jwtTokenAccesser.GetClientDate();
                detail.ReCentrifugationBy = _jwtTokenAccesser.UserId;
                detail.ReCentrifugationOn = _jwtTokenAccesser.GetClientDate();
                detail.Status = CentrifugationFilter.ReCentrifugation;
                Update(detail);
            }
        }
    }

}
