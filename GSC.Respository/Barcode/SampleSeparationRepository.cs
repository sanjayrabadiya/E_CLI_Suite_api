using GSC.Common.GenericRespository;
using GSC.Data.Dto.Barcode;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Barcode;
using GSC.Data.Entities.Project.Design;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.Attendance;
using GSC.Respository.Master;
using GSC.Respository.Project.Design;
using GSC.Respository.Screening;
using GSC.Respository.UserMgt;
using GSC.Shared.Extension;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace GSC.Respository.Barcode
{
    public class SampleSeparationRepository : GenericRespository<SampleSeparation>, ISampleSeparationRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IScreeningTemplateValueRepository _screeningTemplateValueRepository;
        private readonly IProjectDesignVariableRepository _projectDesignVariableRepository;
        private readonly IProjectDesignTemplateRepository _projectDesignTemplateRepository;

        private readonly IProjectRepository _projectRepository;
        private readonly IPKBarcodeRepository _pKBarcodeRepository;
        private readonly ISampleBarcodeRepository _sampleBarcodeRepository;
        private readonly IUserRepository _userRepository;
        private readonly IAuditReasonRepository _auditReasonRepository;
        private readonly ICentrifugationDetailsRepository _centrifugationDetailsRepository;

        public SampleSeparationRepository(IGSCContext context,
            IScreeningTemplateValueRepository screeningTemplateValueRepository,
            IProjectDesignVariableRepository projectDesignVariableRepository,
            IProjectDesignTemplateRepository projectDesignTemplateRepository,
        IProjectRepository projectRepository,
         ICentrifugationDetailsRepository centrifugationDetailsRepository,
        IAuditReasonRepository auditReasonRepository,
             IUserRepository userRepository,
        IPKBarcodeRepository pKBarcodeRepository,
        ISampleBarcodeRepository sampleBarcodeRepository,
        IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _screeningTemplateValueRepository = screeningTemplateValueRepository;
            _pKBarcodeRepository = pKBarcodeRepository;
            _projectDesignTemplateRepository = projectDesignTemplateRepository;
            _projectDesignVariableRepository = projectDesignVariableRepository;
            _userRepository = userRepository;
            _auditReasonRepository = auditReasonRepository;
            _projectRepository = projectRepository;
            _centrifugationDetailsRepository = centrifugationDetailsRepository;
            _sampleBarcodeRepository = sampleBarcodeRepository;
        }

        public List<SampleSeparationGridDto> GetSampleDetails(int siteId, int templateId)
        {
            List<SampleSeparationGridDto> listci = new List<SampleSeparationGridDto>();

            _sampleBarcodeRepository.All.Where(p => p.DeletedDate == null && p.SiteId == siteId && p.BarcodeString != null
        && p.TemplateId == templateId).ToList().ForEach(s =>
            {
                var pkData = _pKBarcodeRepository.All.Where(pk => pk.DeletedDate == null
                    && pk.VolunteerId == s.VolunteerId
                    && pk.SiteId == siteId && pk.BarcodeString != null && pk.TemplateId == templateId
                    && _centrifugationDetailsRepository.All.Any(c => c.DeletedDate == null && c.PKBarcodeId == pk.Id
                    && (c.Status == CentrifugationFilter.Centrifugation || c.Status == CentrifugationFilter.ReCentrifugation))
                    ).FirstOrDefault();
                if (pkData != null)
                {
                    s.BarcodeString.Split(',').ToList().ForEach(sb =>
                {

                    var projectDesignVariable = _projectDesignVariableRepository.All.Where(x => x.DeletedDate == null && x.ProjectDesignTemplateId == templateId).OrderBy(x => x.DesignOrder).FirstOrDefault();
                    var exists = All.Where(x => x.PKBarcodeId == pkData.Id && x.SampleBarcodeString == sb).FirstOrDefault();
                    if (exists == null)
                    {
                        var data = _screeningTemplateValueRepository.All
                                                             .Where(x => x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.SiteId == siteId
                                                             && x.ScreeningTemplate.Status >= Helper.ScreeningTemplateStatus.Submitted
                                                             && x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == s.VolunteerId
                                                             && x.ProjectDesignVariableId == projectDesignVariable.Id)
                                                             .Select(x => new SampleSeparationGridDto
                                                             {
                                                                 PKBarcodeId = pkData.Id,
                                                                 SampleBarcodeId=s.Id,
                                                                 StudyCode = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Project.ProjectCode,
                                                                 SiteCode = _projectRepository.Find(siteId).ProjectCode,
                                                                 RandomizationNumber = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.RandomizationNumber,
                                                                 PKBarcode = pkData.BarcodeString,
                                                                 Template=_projectDesignTemplateRepository.Find(templateId).TemplateName,
                                                                 SampleBarcode = sb,
                                                                 PKActualTime = x.Value,
                                                                 SampleStartTime = null,
                                                                 SampleUserBy = null,
                                                                 SampleOn = null,
                                                                 Status = "Remaining",
                                                                 AuditReason = null,
                                                                 ReasonOth = null,
                                                                 ActionBy=null,
                                                                 ActionOn=null
                                                             }).FirstOrDefault();
                        if (data != null)
                            listci.Add(data);
                    }
                    else
                    {
                        var sampleData = All.Where(x => x.PKBarcodeId == pkData.Id && x.SampleBarcodeString == sb).FirstOrDefault();
                        var data = _screeningTemplateValueRepository.All
                                                             .Where(x => x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.SiteId == siteId
                                                             && x.ScreeningTemplate.Status >= Helper.ScreeningTemplateStatus.Submitted
                                                             && x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.VolunteerId == s.VolunteerId
                                                             && x.ProjectDesignVariableId == projectDesignVariable.Id)
                                                             .Select(x => new SampleSeparationGridDto
                                                             {
                                                                 PKBarcodeId = pkData.Id,
                                                                 SampleBarcodeId = s.Id,
                                                                 StudyCode = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Project.ProjectCode,
                                                                 SiteCode = _projectRepository.Find(siteId).ProjectCode,
                                                                 RandomizationNumber = x.ScreeningTemplate.ScreeningVisit.ScreeningEntry.Attendance.Volunteer.RandomizationNumber,
                                                                 PKBarcode = pkData.BarcodeString,
                                                                 Template = _projectDesignTemplateRepository.Find(templateId).TemplateName,
                                                                 PKActualTime = x.Value,
                                                                 SampleBarcode = sb,
                                                                 SampleStartTime = sampleData.SampleStartTime,
                                                                 SampleUserBy = _userRepository.All.Where(x => x.Id == sampleData.CreatedBy).FirstOrDefault().UserName,
                                                                 SampleOn = sampleData.CreatedDate,
                                                                 Status = sampleData.Status.GetDescription(),
                                                                 AuditReason = sampleData.AuditReasonId != null ? _auditReasonRepository.All.Where(x => x.Id == sampleData.AuditReasonId).FirstOrDefault().ReasonName : null,
                                                                 ReasonOth = sampleData.ReasonOth,
                                                                 ActionBy = sampleData.ModifiedBy != null ? _userRepository.All.Where(x => x.Id == sampleData.ModifiedBy).FirstOrDefault().UserName : null,
                                                                 ActionOn= sampleData.ModifiedDate,

                                                             }).FirstOrDefault();
                        if (data != null)
                            listci.Add(data);
                    }
                });
                }
            });

            return listci;
        }
        public void StartSampleSaparation(SampleSaveSeparationDto dto)
        {
                SampleSeparation r = new SampleSeparation();
                r.PKBarcodeId = dto.PKBarcodeId;
                r.SampleBarcodeId = dto.SampleBarcodeId;
            r.SampleStartTime = _jwtTokenAccesser.GetClientDate();
                r.Status = SampleSeparationFilter.Separated;
            r.SampleBarcodeString = dto.SampleBarcodeString;
                Add(r);
         }


        public IList<DropDownDto> GetTemplateForSaparation(int siteId)
        {
            var pkData = _pKBarcodeRepository.All.Where(pk => pk.DeletedDate == null                   
                   && pk.SiteId == siteId && pk.BarcodeString != null 
                   && _centrifugationDetailsRepository.All.Any(c => c.DeletedDate == null && c.PKBarcodeId==pk.Id
                   && (c.Status == CentrifugationFilter.Centrifugation || c.Status == CentrifugationFilter.ReCentrifugation))
            ).Select(t=>t.TemplateId).ToList();


            return _projectDesignTemplateRepository.All.Where(x => x.DeletedDate == null && pkData.Contains(x.Id))
              .Select(t => new DropDownDto { Id = t.Id, Value = t.TemplateName }).ToList();
        }

    }
}
