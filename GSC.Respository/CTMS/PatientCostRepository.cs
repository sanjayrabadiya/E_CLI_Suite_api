using GSC.Common.GenericRespository;
using GSC.Data.Dto.CTMS;
using GSC.Data.Entities.CTMS;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Respository.CTMS
{
    public class PatientCostRepository : GenericRespository<PatientCost>, IPatientCostRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;

        public PatientCostRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }

        public bool CheckVisitData(bool isDeleted, int studyId)
        {
            return _context.PatientCost.Any(x => x.ProjectId == studyId && x.DeletedBy == null);
        }

        public List<ProcedureVisitdadaDto> GetPullPatientCost(bool isDeleted, int studyId, int? procedureId, bool isPull)
        {
            if (isPull)
            {
                DeleteOldPatientCost(studyId);
                AddNewPatientCost(studyId);
                UpdatePatientCostFromProjectDesignVisit(studyId);
                RemoveDeletedVisitsFromPatientCost(studyId);
            }
            else
            {
                HandleNoPullCase(studyId);
            }

            return GetPatientCostProcedures(studyId, procedureId);
        }

        public List<PatientCostGridData> GetPatientCostGrid(bool isDeleted, int studyId)
        {
            var patientCosts = _context.PatientCost
                .Include(s => s.Procedure)
                .Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId != null)
                .Select(t => new PatientCostGridData
                {
                    ProjectId = t.ProjectId,
                    ProcedureId = t.ProcedureId,
                    ProcedureName = t.Procedure.Name,
                    CurrencyId = t.Procedure.CurrencyId,
                    CurrencyType = $"{t.Procedure.Currency.CurrencyName}-{t.Procedure.Currency.CurrencySymbol}",
                    Rate = t.Rate,
                    CurrencyRate = t.CurrencyRate.LocalCurrencyRate,
                    CurrencySymbol = t.Currency.CurrencySymbol,
                    LocalCurrencySymbol = t.Procedure.Currency.CurrencySymbol,
                    PatientCount = 1
                })
                .Distinct()
                .ToList();

            foreach (var item in patientCosts)
            {
                item.VisitGridDatas = GetPatientCostVisitData(studyId, item.ProcedureId);
            }

            return patientCosts;
        }

        public List<PatientCostGridData> GetPatientCostCurrencyGrid(bool isDeleted, int studyId)
        {
            var patientCostGridData = new List<PatientCostGridData>();
            var duplicates = _context.PatientCost
                .Include(s => s.Procedure)
                .Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId != null)
                .GroupBy(i => i.Procedure.CurrencyId)
                .Where(x => x.Count() > 0)
                .Select(val => val.Key)
                .ToList();

            foreach (var currencyId in duplicates)
            {
                var patientCostData = _context.PatientCost
                    .Include(s => s.Procedure)
                    .Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.Procedure.CurrencyId == currencyId)
                    .Select(t => new PatientCostGridData
                    {
                        ProcedureId = t.ProcedureId,
                        CurrencyId = t.Procedure.CurrencyId,
                        CurrencyType = $"{t.Procedure.Currency.CurrencyName}-{t.Procedure.Currency.CurrencySymbol}",
                        CurrencyRate = t.CurrencyRate.LocalCurrencyRate,
                        CurrencySymbol = t.Currency.CurrencySymbol,
                        LocalCurrencySymbol = t.Procedure.Currency.CurrencySymbol,
                        PatientCount = t.PatientCount
                    })
                    .Distinct()
                    .ToList();

                var visitData = GetVisitGridData(studyId, patientCostData.Select(r => r.ProcedureId).ToList());
                if (patientCostData.Any())
                {
                    patientCostData.First().VisitGridDatas = visitData;
                    patientCostGridData.Add(patientCostData.First());
                }
            }

            return patientCostGridData;
        }

        public bool AddPatientCount(int studyId, int currencyId, int patientCount)
        {
            var data = _context.PatientCost
                .Include(i => i.Procedure)
                .Where(w => w.DeletedBy == null && w.ProjectId == studyId && w.Procedure.CurrencyId == currencyId && w.ProcedureId != null)
                .ToList();

            data.ForEach(x =>
            {
                x.PatientCount = patientCount;
                _context.PatientCost.Update(x);
                _context.Save();
            });

            return true;
        }

        public string Duplicate(List<ProcedureVisitdadaDto> procedureVisitDataDto)
        {
            var locCurrency = _context.Procedure
                .Include(e => e.Currency)
                .FirstOrDefault(s => s.Id == procedureVisitDataDto[0].ProcedureId && s.DeletedDate == null);

            var studyPlan = _context.StudyPlan
                .FirstOrDefault(s => s.ProjectId == procedureVisitDataDto[0].ProjectId && s.DeletedDate == null);

            if (locCurrency == null || studyPlan == null)
            {
                return "Invalid Procedure or Study Plan.";
            }

            if (All.Any(x => x.Id != procedureVisitDataDto[0].Id && x.ProcedureId == procedureVisitDataDto[0].ProcedureId && x.DeletedDate == null && x.ProjectId == procedureVisitDataDto[0].ProjectId && !procedureVisitDataDto[0].IfEdit))
            {
                return "Duplicate Patient Cost";
            }

            var currencyRateExists = _context.CurrencyRate.Any(s => s.StudyPlanId == studyPlan.Id && s.CurrencyId == locCurrency.CurrencyId && s.DeletedBy == null);
            if (!currencyRateExists && locCurrency.CurrencyId != studyPlan.CurrencyId)
            {
                return $"{locCurrency.Currency.CurrencyName} - {locCurrency.Currency.CurrencySymbol} currency and rate must be added in the study plan.";
            }

            return string.Empty;
        }

        public void AddPatientCost(List<ProcedureVisitdadaDto> procedureVisitDataDto)
        {
            var locCurrency = _context.Procedure.FirstOrDefault(s => s.Id == procedureVisitDataDto[0].ProcedureId && s.DeletedDate == null);
            var studyPlan = _context.StudyPlan.FirstOrDefault(s => s.ProjectId == procedureVisitDataDto[0].ProjectId && s.DeletedDate == null);
            var currencyRate = _context.CurrencyRate.FirstOrDefault(s => s.StudyPlanId == studyPlan.Id && s.CurrencyId == locCurrency.CurrencyId && s.DeletedDate == null);

            if (locCurrency == null || studyPlan == null)
            {
                throw new ArgumentException("Invalid Procedure or Study Plan.");
            }

            procedureVisitDataDto.ForEach(d =>
            {
                var patientCost = _context.PatientCost.Where(s => s.Id == d.Id && s.DeletedBy == null).ToList();
                patientCost.ForEach(t =>
                {
                    t.ProcedureId = d.ProcedureId;
                    t.Cost = d.Cost;
                    t.FinalCost = currencyRate != null ? d.FinalCost * currencyRate.LocalCurrencyRate : d.FinalCost;
                    t.Rate = d.Rate;
                    t.CurrencyRateId = currencyRate?.Id;
                    t.CurrencyId = studyPlan.CurrencyId;
                    _context.PatientCost.Update(t);
                    _context.Save();
                });
            });
        }

        public void DeletePatientCost(int projectId, int procedureId)
        {
            var patientCosts = _context.PatientCost.Where(s => s.ProjectId == projectId && s.ProcedureId == procedureId && s.DeletedBy == null).ToList();

            patientCosts.ForEach(t =>
            {
                t.DeletedBy = _jwtTokenAccesser.UserId;
                t.DeletedDate = DateTime.UtcNow;
                _context.PatientCost.Update(t);
                _context.Save();
            });
        }

        // Helper Methods

        private void DeleteOldPatientCost(int studyId)
        {
            var patientCosts = _context.PatientCost
                .Where(s => s.ProjectId == studyId && s.ProcedureId == null && s.DeletedBy == null)
                .ToList();

            patientCosts.ForEach(t =>
            {
                t.DeletedBy = _jwtTokenAccesser.UserId;
                t.DeletedDate = DateTime.UtcNow;
                _context.PatientCost.Update(t);
                _context.Save();
            });
        }

        private void AddNewPatientCost(int studyId)
        {
            var newPatientCosts = _context.ProjectDesignVisit
                .Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == studyId && x.DeletedBy == null)
                .Select(t => new PatientCost
                {
                    ProjectId = studyId,
                    ProcedureId = null,
                    ProjectDesignVisitId = t.Id,
                    Rate = null,
                    Cost = null,
                    FinalCost = null,
                    IfPull = true
                })
                .ToList();

            newPatientCosts.ForEach(x =>
            {
                _context.PatientCost.Add(x);
                _context.Save();
            });
        }

        private void UpdatePatientCostFromProjectDesignVisit(int studyId)
        {
            var existingPatientCosts = _context.PatientCost
                .Where(s => s.ProjectId == studyId && s.ProcedureId != null && s.DeletedBy == null)
                .ToList();

            if (!existingPatientCosts.Any()) return;

            var newVisits = _context.ProjectDesignVisit
                .Where(x => !existingPatientCosts.Select(v => v.ProjectDesignVisitId).Contains(x.Id) && x.DeletedBy == null && x.ProjectDesignPeriod.ProjectDesign.ProjectId == studyId)
                .ToList();

            newVisits.ForEach(x =>
            {
                var newPatientCosts = existingPatientCosts
                    .Select(t => new PatientCost
                    {
                        ProjectId = studyId,
                        ProcedureId = t.ProcedureId,
                        ProjectDesignVisitId = x.Id,
                        Rate = t.Rate,
                        CurrencyRateId = t.CurrencyRateId,
                        CurrencyId = t.CurrencyId,
                        IfPull = t.IfPull
                    })
                    .Distinct()
                    .ToList();

                newPatientCosts.ForEach(t =>
                {
                    _context.PatientCost.Add(t);
                    _context.Save();
                });
            });
        }

        private void RemoveDeletedVisitsFromPatientCost(int studyId)
        {
            var deletedVisits = _context.ProjectDesignVisit
                .Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == studyId && x.DeletedBy != null)
                .Select(x => x.Id)
                .ToList();

            var patientCostsToDelete = _context.PatientCost
                .Where(s => deletedVisits.Contains(s.ProjectDesignVisitId.Value) && s.DeletedBy == null)
                .ToList();

            patientCostsToDelete.ForEach(t =>
            {
                t.DeletedBy = _jwtTokenAccesser.UserId;
                t.DeletedDate = DateTime.UtcNow;
                _context.PatientCost.Update(t);
                _context.Save();
            });
        }

        private void HandleNoPullCase(int studyId)
        {
            if (!_context.PatientCost.Any(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId == null) &&
                _context.PatientCost.Where(x => x.DeletedBy == null && x.ProjectId == studyId).Select(s => s.IfPull).FirstOrDefault())
            {
                AddNewPatientCost(studyId);
            }
            else if (!_context.PatientCost.Any(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId == null) &&
                     !_context.PatientCost.Where(x => x.DeletedBy == null && x.ProjectId == studyId).Select(s => s.IfPull).FirstOrDefault())
            {
                var visitData = _context.PatientCost
                    .Where(x => x.ProjectId == studyId && x.DeletedBy == null && x.VisitName != null)
                    .Select(t => new PatientCost
                    {
                        ProjectId = studyId,
                        ProcedureId = null,
                        ProjectDesignVisitId = null,
                        VisitName = t.VisitName,
                        VisitDescription = t.VisitDescription,
                        Rate = null,
                        Cost = null,
                        FinalCost = null,
                        IfPull = false
                    })
                    .Distinct()
                    .ToList();

                visitData.ForEach(x =>
                {
                    _context.PatientCost.Add(x);
                    _context.Save();
                });
            }
        }

        private List<ProcedureVisitdadaDto> GetPatientCostProcedures(int studyId, int? procedureId)
        {
            return _context.PatientCost
                .Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId == (procedureId == 0 ? null : procedureId))
                .Select(t => new ProcedureVisitdadaDto
                {
                    Id = t.Id,
                    ProjectId = t.ProjectId,
                    ProcedureId = t.ProcedureId,
                    ProcedureName = t.Procedure.Name,
                    ProjectDesignVisitId = t.ProjectDesignVisitId,
                    VisitName = t.ProjectDesignVisitId != null ? t.ProjectDesignVisit.DisplayName : t.VisitName,
                    Rate = t.Rate,
                    Cost = t.Cost,
                    FinalCost = t.FinalCost,
                    CurrencyRate = t.CurrencyRate.LocalCurrencyRate,
                    GlobleCurrencySymbol = t.Currency.CurrencySymbol,
                    CurrencySymbol = t.Procedure.Currency.CurrencySymbol,
                    IfPull = t.IfPull
                })
                .Distinct()
                .ToList();
        }

        private List<VisitGridData> GetPatientCostVisitData(int studyId, int? procedureId)
        {
            return _context.PatientCost
                .Include(s => s.ProjectDesignVisit)
                .Where(x => x.ProcedureId == procedureId && x.ProjectId == studyId && x.DeletedBy == null)
                .Select(t => new VisitGridData
                {
                    VisitId = t.ProjectDesignVisitId,
                    VisitName = t.ProjectDesignVisitId != null ? t.ProjectDesignVisit.DisplayName : t.VisitName,
                    FinalCost = t.FinalCost,
                    LocalFinalCost = t.Cost * t.Rate
                })
                .ToList();
        }

        private List<VisitGridData> GetVisitGridData(int studyId, List<int?> procedureIds)
        {
            return _context.PatientCost
                .Include(s => s.ProjectDesignVisit)
                .Where(x => procedureIds.Contains(x.ProcedureId) && x.ProjectId == studyId && x.DeletedBy == null)
                .GroupBy(g => g.ProjectDesignVisitId)
                .Select(t => new VisitGridData
                {
                    VisitId = t.Key,
                    VisitName = t.Select(r => r.ProjectDesignVisit.DisplayName).FirstOrDefault(),
                    FinalCost = t.Sum(r => r.FinalCost),
                    LocalFinalCost = t.Sum(r => r.Cost * r.Rate)
                })
                .ToList();
        }
    }
}
