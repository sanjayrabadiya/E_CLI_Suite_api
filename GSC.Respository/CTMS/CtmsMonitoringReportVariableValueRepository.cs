using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.QueryableExtensions;
using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.CTMS
{
    public class CtmsMonitoringReportVariableValueRepository : GenericRespository<CtmsMonitoringReportVariableValue>, ICtmsMonitoringReportVariableValueRepository
    {
        private readonly IMapper _mapper;
        private readonly IGSCContext _context;
        private readonly ICtmsMonitoringReportVariableValueAuditRepository _ctmsMonitoringReportVariableValueAuditRepository;
        private readonly ICtmsMonitoringReportVariableValueChildRepository _ctmsMonitoringReportVariableValueChildRepository;
        //private readonly ICtmsMonitoringReportRepository _ctmsMonitoringReportRepository;
        public CtmsMonitoringReportVariableValueRepository(IGSCContext context, IMapper mapper,
            ICtmsMonitoringReportVariableValueAuditRepository ctmsMonitoringReportVariableValueAuditRepository,
            ICtmsMonitoringReportVariableValueChildRepository ctmsMonitoringReportVariableValueChildRepository)
            : base(context)
        {
            _mapper = mapper;
            _context = context;
            _ctmsMonitoringReportVariableValueAuditRepository = ctmsMonitoringReportVariableValueAuditRepository;
            _ctmsMonitoringReportVariableValueChildRepository = ctmsMonitoringReportVariableValueChildRepository;
        }

        public void UpdateChild(List<CtmsMonitoringReportVariableValueChild> children)
        {
            _context.CtmsMonitoringReportVariableValueChild.UpdateRange(children);
        }

        public List<CtmsMonitoringReportVariableValueBasic> GetVariableValues(int CtmsMonitoringReportId)
        {
            return All.Include(x => x.CtmsMonitoringReport).AsNoTracking().Where(t => t.CtmsMonitoringReportId == CtmsMonitoringReportId)
                    .ProjectTo<CtmsMonitoringReportVariableValueBasic>(_mapper.ConfigurationProvider).ToList();
        }

        public string GetValueForAudit(CtmsMonitoringReportVariableValueDto cstmsMonitoringReportVariableValueDto, CtmsMonitoringReportVariableValueChildDto ctmsMonitoringReportVariableValueChildDto)
        {
            if (cstmsMonitoringReportVariableValueDto.IsDeleted) return null;

            if (cstmsMonitoringReportVariableValueDto.Children?.Count > 0)
            {
                var child = ctmsMonitoringReportVariableValueChildDto;

                var variableValue = _context.StudyLevelFormVariableValue.Find(child.StudyLevelFormVariableValueId);
                if (variableValue != null)
                {
                    var valueChild = _context.CtmsMonitoringReportVariableValueChild.AsNoTracking()
                        .FirstOrDefault(t => t.Id == child.Id);
                    if (valueChild != null && child.Value == "false")
                    {
                        cstmsMonitoringReportVariableValueDto.OldValue = variableValue.ValueName;
                        return "";
                    }

                    cstmsMonitoringReportVariableValueDto.OldValue = "";
                    return variableValue.ValueName;
                }

                return child.Value;
            }

            if (cstmsMonitoringReportVariableValueDto.IsNa)
                return "N/A";

            return string.IsNullOrWhiteSpace(cstmsMonitoringReportVariableValueDto.ValueName)
                ? cstmsMonitoringReportVariableValueDto.Value
                : cstmsMonitoringReportVariableValueDto.ValueName;
        }

        public void DeleteChild(int ctmsMonitoringReportVariableValueId)
        {
            var childs = _context.CtmsMonitoringReportVariableValueChild
                .Where(t => t.CtmsMonitoringReportVariableValueId == ctmsMonitoringReportVariableValueId).ToList();
            _context.CtmsMonitoringReportVariableValueChild.RemoveRange(childs);
        }

        public bool GetQueryStatusByReportId(int ctmsMonitoringReportId)
        {
            var result = All.Where(x => x.DeletedDate == null
                    && x.CtmsMonitoringReportId == ctmsMonitoringReportId && x.QueryStatus != null
                    && x.QueryStatus != CtmsCommentStatus.Closed).Count();

            return result != 0;
        }

        public void SaveVariableValue(CtmsMonitoringReportVariableValueSaveDto ctmsMonitoringReportVariableValueSaveDto)
        {
            if (ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList != null)
            {
                foreach (var item in ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList)
                {
                    var ctmsMonitoringReportVariableValue = _mapper.Map<CtmsMonitoringReportVariableValue>(item);

                    var Exists = All.Where(x => x.DeletedDate == null && x.CtmsMonitoringReportId == ctmsMonitoringReportVariableValue.CtmsMonitoringReportId && x.StudyLevelFormVariableId == item.StudyLevelFormVariableId).FirstOrDefault();

                    if (Exists == null)
                    {
                        ctmsMonitoringReportVariableValue.Id = 0;
                        Add(ctmsMonitoringReportVariableValue);

                        if (item.Children?.Count > 0)
                        {
                            foreach (var child in item.Children)
                            {
                                var childvalue = GetValueForAudit(item, child);
                                var aduit = new CtmsMonitoringReportVariableValueAudit
                                {
                                    CtmsMonitoringReportVariableValue = ctmsMonitoringReportVariableValue,
                                    Value = item.IsNa ? "N/A" : childvalue,
                                    OldValue = item.OldValue,
                                };
                                _ctmsMonitoringReportVariableValueAuditRepository.Save(aduit);
                            }
                        }
                        else
                        {
                            var value = GetValueForAudit(item, null);

                            var aduit = new CtmsMonitoringReportVariableValueAudit
                            {
                                CtmsMonitoringReportVariableValue = ctmsMonitoringReportVariableValue,
                                Value = item.IsNa ? "N/A" : value,
                                OldValue = item.OldValue,
                            };
                            _ctmsMonitoringReportVariableValueAuditRepository.Save(aduit);
                        }
                        _ctmsMonitoringReportVariableValueChildRepository.Save(ctmsMonitoringReportVariableValue);
                    }
                    else
                    {
                        if (item.Children?.Count > 0)
                        {
                            foreach (var child in item.Children)
                            {
                                var childvalue = GetValueForAudit(item, child);
                                var aduit = new CtmsMonitoringReportVariableValueAudit
                                {
                                    CtmsMonitoringReportVariableValueId = Exists.Id,
                                    Value = item.IsNa ? "N/A" : childvalue,
                                    OldValue = item.OldValue,
                                };
                                _ctmsMonitoringReportVariableValueAuditRepository.Save(aduit);
                            }
                        }
                        else
                        {
                            var value = GetValueForAudit(item, null);

                            var aduit = new CtmsMonitoringReportVariableValueAudit
                            {
                                CtmsMonitoringReportVariableValueId = Exists.Id,
                                Value = item.IsNa ? "N/A" : value,
                                OldValue = item.OldValue,
                            };
                            _ctmsMonitoringReportVariableValueAuditRepository.Save(aduit);
                        }

                        if (item.IsDeleted)
                            DeleteChild(Exists.Id);

                        _ctmsMonitoringReportVariableValueChildRepository.Save(ctmsMonitoringReportVariableValue);

                        ctmsMonitoringReportVariableValue.Id = Exists.Id;
                        Update(ctmsMonitoringReportVariableValue);
                    }
                }

                //Changes made by Sachin
                var ctmsMonitoringReport = _context.CtmsMonitoringReport.Where(x => x.Id == ctmsMonitoringReportVariableValueSaveDto.CtmsMonitoringReportVariableValueList[0].CtmsMonitoringReportId).FirstOrDefault();
                ctmsMonitoringReport.ReportStatus = MonitoringReportStatus.OnGoing;
                _context.CtmsMonitoringReport.Update(ctmsMonitoringReport);
            }

        }
    }
}