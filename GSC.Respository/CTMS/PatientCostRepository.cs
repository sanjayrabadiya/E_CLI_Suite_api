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

        public PatientCostRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
        }
        public bool CheckVisitData(bool isDeleted, int studyId)
        {
            if (_context.PatientCost.Where(x => x.ProjectId == studyId && x.DeletedBy == null).ToList().Count == 0)
                return false;

            return true;
        }
        public List<ProcedureVisitdadaDto> GetPullPatientCost(bool isDeleted, int studyId, int? procedureId, bool ispull)
        {
            if (ispull)
            {
                var patientCost = _context.PatientCost.Where(s => s.ProjectId == studyId && s.ProcedureId == null && s.DeletedBy == null).ToList();
                patientCost.ForEach(t =>
                {

                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.PatientCost.Update(t);
                    _context.Save();
                });


                var VisitData = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == studyId && x.DeletedBy == null)
                    .Select(t => new PatientCost
                    {
                        ProjectId = studyId,
                        ProcedureId = null,
                        ProjectDesignVisitId = t.Id,
                        Rate = null,
                        Cost = null,
                        FinalCost = null,
                        IfPull = true,
                    }).ToList();
                VisitData.ForEach(x =>
                {
                    _context.PatientCost.Add(x);
                    _context.Save();
                });

                // Click pull get new visit from ProjectDesignVisit
                var patientCostdDta = _context.PatientCost.Where(s => s.ProjectId == studyId && s.ProcedureId != null && s.DeletedBy == null).ToList();
                if (patientCostdDta.Count > 0)
                {
                    var VisitData1 = _context.ProjectDesignVisit.Where(x => !patientCostdDta.Select(v => v.ProjectDesignVisitId).Contains(x.Id) && x.DeletedBy == null && x.ProjectDesignPeriod.ProjectDesign.ProjectId == studyId).ToList();
                    VisitData1.ForEach(x =>
                    {
                        var patientCost1 = _context.PatientCost.Where(s => s.ProjectId == studyId && s.ProcedureId != null && s.DeletedBy == null).
                        Select(t => new PatientCost
                        {
                            ProjectId = studyId,
                            ProcedureId = t.ProcedureId,
                            ProjectDesignVisitId = x.Id,
                            Rate = t.Rate,
                            CurrencyRateId = t.CurrencyRateId,
                            CurrencyId = t.CurrencyId,
                            IfPull = t.IfPull,
                        }).Distinct().ToList();

                        patientCost1.ForEach(t =>
                        {
                            _context.PatientCost.Add(t);
                            _context.Save();
                        });
                    });
                }

                //Click pull delete visit from ProjectDesignVisit
                var data = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == studyId && x.DeletedBy != null).ToList();
                var patientCostDel = _context.PatientCost.Where(s => data.Select(v => (int?)v.Id).Contains(s.ProjectDesignVisitId) && s.DeletedBy == null).ToList();
                patientCostDel.ForEach(t =>
                {
                    t.DeletedBy = _jwtTokenAccesser.UserId;
                    t.DeletedDate = DateTime.UtcNow;
                    _context.PatientCost.Update(t);
                    _context.Save();
                });
            }
            else if (_context.PatientCost.Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId == null).ToList().Count == 0 &&
                    _context.PatientCost.Where(x => x.DeletedBy == null && x.ProjectId == studyId).Select(s => s.IfPull).FirstOrDefault())
            {
                var VisitData = _context.ProjectDesignVisit.Where(x => x.ProjectDesignPeriod.ProjectDesign.ProjectId == studyId && x.DeletedBy == null)
                    .Select(t => new PatientCost
                    {
                        ProjectId = studyId,
                        ProcedureId = null,
                        ProjectDesignVisitId = t.Id,
                        Rate = null,
                        Cost = null,
                        FinalCost = null,
                        IfPull = true,
                    }).ToList();
                VisitData.ForEach(x =>
                {
                    _context.PatientCost.Add(x);
                    _context.Save();
                });
            }
            else if (_context.PatientCost.Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId == null).ToList().Count == 0 &&
                    !_context.PatientCost.Where(x => x.DeletedBy == null && x.ProjectId == studyId).Select(s => s.IfPull).FirstOrDefault())
            {
                var VisitData = _context.PatientCost.Where(x => x.ProjectId == studyId && x.DeletedBy == null && x.VisitName != null)
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
                        IfPull = false,
                    }).Distinct().ToList();
                VisitData.ForEach(x =>
                {
                    _context.PatientCost.Add(x);
                    _context.Save();
                });
            }

            var PatientCostProced = _context.PatientCost.Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId == (procedureId == 0 ? null : procedureId)).
            Select(t => new ProcedureVisitdadaDto
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
            }).Distinct().ToList();

            return PatientCostProced;
        }
        public List<PatientCostGridData> GetPatientCostGrid(bool isDeleted, int studyId)
        {
            var PatientCostProced = _context.PatientCost.Include(s => s.Procedure).Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId != null).
            Select(t => new PatientCostGridData
            {
                ProjectId = t.ProjectId,
                ProcedureId = t.ProcedureId,
                ProcedureName = t.Procedure.Name,
                CurrencyId = t.Procedure.CurrencyId,
                CurrencyType = t.Procedure.Currency.CurrencyName + "-" + t.Procedure.Currency.CurrencySymbol,
                Rate = t.Rate,
                CurrencyRate = t.CurrencyRate.LocalCurrencyRate,
                CurrencySymbol = t.Currency.CurrencySymbol,
                LocalCurrencySymbol = t.Procedure.Currency.CurrencySymbol,
                PatientCount = 1
            }).Distinct().ToList();

            foreach (var item in PatientCostProced)
            {
                var PatientCostVisit = _context.PatientCost.Include(s => s.ProjectDesignVisit).Where(x => x.ProcedureId == item.ProcedureId && x.ProjectId == studyId && x.DeletedBy == null)
                .Select(t => new VisitGridData
                {
                    VisitId = t.ProjectDesignVisitId,
                    VisitName = t.ProjectDesignVisitId != null ? t.ProjectDesignVisit.DisplayName : t.VisitName,
                    FinalCost = t.FinalCost,
                    LocalFinalCost = t.Cost * t.Rate,
                }).ToList();
                item.VisitGridDatas = PatientCostVisit;
            }
            return PatientCostProced;
        }

        public List<PatientCostGridData> GetPatientCostCurrencyGrid(bool isDeleted, int studyId)
        {
            var patientcostproced = new List<PatientCostGridData>();
            var patientcostprocedTemp = new List<PatientCostGridData>();
            var duplicates = _context.PatientCost.Include(s => s.Procedure).Where(x=> x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId != null).GroupBy(i => i.Procedure.CurrencyId).Where(x => x.Count() > 0  ).Select(val => val.Key).ToList();
           for (var i = 0; i < duplicates.Count; i++)
            {
                patientcostprocedTemp = _context.PatientCost.Include(s => s.Procedure).Where(x => x.DeletedBy == null && x.ProjectId == studyId && x.ProcedureId != null && x.Procedure.CurrencyId == duplicates[i]).
                Select(t => new PatientCostGridData
                {
                    ProcedureId = t.ProcedureId,
                    CurrencyId = t.Procedure.CurrencyId,
                    CurrencyType = t.Procedure.Currency.CurrencyName + "-" + t.Procedure.Currency.CurrencySymbol,
                    CurrencyRate = t.CurrencyRate.LocalCurrencyRate,
                    CurrencySymbol = t.Currency.CurrencySymbol,
                    LocalCurrencySymbol = t.Procedure.Currency.CurrencySymbol,
                    PatientCount = t.PatientCount
                }).Distinct().ToList();

                var PatientCostVisit = _context.PatientCost.Include(s => s.ProjectDesignVisit).
                Where(x => patientcostprocedTemp.Select(r => r.ProcedureId).Contains(x.ProcedureId) && x.ProjectId == studyId && x.DeletedBy == null).
                GroupBy(g => g.ProjectDesignVisitId)
                .Select(t => new VisitGridData
                {
                    VisitId = t.Key,
                    VisitName = t.Select(r => r.ProjectDesignVisit.DisplayName).FirstOrDefault(),
                    FinalCost = t.Sum(r => r.FinalCost),
                    LocalFinalCost = t.Sum(r => r.Cost * r.Rate)
                }).ToList();
                patientcostproced.Add(patientcostprocedTemp.FirstOrDefault());
                patientcostproced[i].VisitGridDatas = PatientCostVisit;
            }
            return patientcostproced;
        }

        public bool AddPatientCount(int studyId, int currencyId, int patientCount)
        {
            var data = _context.PatientCost.Include(i => i.Procedure).Where(w => w.DeletedBy == null && w.ProjectId == studyId && w.Procedure.CurrencyId == currencyId && w.ProcedureId != null).ToList();
            data.ForEach(x => {
                x.PatientCount = patientCount;
                _context.PatientCost.Update(x);
                _context.Save();
            });
            return true;
        }
        public string Duplicate(List<ProcedureVisitdadaDto> ProcedureVisitdadaDto)
        {
            var locCurrency = _context.Procedure.Include(e => e.Currency).Where(s => s.Id == ProcedureVisitdadaDto[0].ProcedureId && s.DeletedDate == null).FirstOrDefault();
            var studyPlan = _context.StudyPlan.Where(s => s.ProjectId == ProcedureVisitdadaDto[0].ProjectId && s.DeletedDate == null).FirstOrDefault();

            //new Cost add time duplication check
            if (All.Any(x => x.Id != ProcedureVisitdadaDto[0].Id && x.ProcedureId == ProcedureVisitdadaDto[0].ProcedureId && x.DeletedDate == null && x.ProjectId == ProcedureVisitdadaDto[0].ProjectId && !ProcedureVisitdadaDto[0].IfEdit))
            {
                return "Duplicate Patient Cost";
            }
            //check currency rate added or not, currency rate is requerd
            else if (!_context.CurrencyRate.Where(s => s.StudyPlanId == studyPlan.Id && s.CurrencyId == locCurrency.CurrencyId && s.DeletedBy == null).Any() && locCurrency.CurrencyId != studyPlan.CurrencyId)
            {
                return locCurrency.Currency.CurrencyName + " - " + locCurrency.Currency.CurrencySymbol + " Is Currency And Rate Added in Study plan. ";
            }
            else
            {
                return "";
            }
        }
        public void AddPatientCost(List<ProcedureVisitdadaDto> procedureVisitdadaDto)
        {
            //get CurrencyRate And Globel Currency form studyPlan
            var locCurrency = _context.Procedure.Where(s => s.Id == procedureVisitdadaDto[0].ProcedureId && s.DeletedDate == null).FirstOrDefault();
            var studyPlan = _context.StudyPlan.Where(s => s.ProjectId == procedureVisitdadaDto[0].ProjectId && s.DeletedDate == null).FirstOrDefault();
            var CurrencyRate = _context.CurrencyRate.Where(s => s.StudyPlanId == studyPlan.Id && s.CurrencyId == locCurrency.CurrencyId && s.DeletedDate == null).FirstOrDefault();
            if (CurrencyRate != null)
            {
                procedureVisitdadaDto.ForEach(d =>
                {
                    var patientCost = _context.PatientCost.Where(s => s.Id == d.Id && s.DeletedBy == null).ToList();
                    patientCost.ForEach(t =>
                    {
                        t.ProcedureId = d.ProcedureId;
                        t.Cost = d.Cost;
                        t.FinalCost = d.FinalCost * CurrencyRate.LocalCurrencyRate; //Cost Conveart into globale currency
                        t.Rate = d.Rate;
                        t.CurrencyRateId = CurrencyRate.Id;
                        t.CurrencyId = studyPlan.CurrencyId;
                        _context.PatientCost.Update(t);
                        _context.Save();
                    });
                });
            }
            else
            {
                procedureVisitdadaDto.ForEach(d =>
                {
                    var patientCost = _context.PatientCost.Where(s => s.Id == d.Id && s.DeletedBy == null).ToList();
                    patientCost.ForEach(t =>
                    {
                        t.ProcedureId = d.ProcedureId;
                        t.Cost = d.Cost;
                        t.FinalCost = d.FinalCost;  //Cost Conveart into same Currency
                        t.Rate = d.Rate;
                        t.CurrencyRateId = null;
                        t.CurrencyId = studyPlan.CurrencyId;
                        _context.PatientCost.Update(t);
                        _context.Save();
                    });
                });
            }
        }
        public void DeletePatientCost(int projectId, int procedureId)
        {
            var patientCost = _context.PatientCost.Where(s => s.ProjectId == projectId && s.ProcedureId == procedureId && s.DeletedBy == null).ToList();
            patientCost.ForEach(t =>
            {
                t.DeletedBy = _jwtTokenAccesser.UserId;
                t.DeletedDate = DateTime.UtcNow;
                _context.PatientCost.Update(t);
                _context.Save();
            });

        }

    }
}
