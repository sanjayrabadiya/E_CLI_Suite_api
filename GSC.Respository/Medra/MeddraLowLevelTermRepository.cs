using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GSC.Data.Dto.Medra;
using System.IO;

namespace GSC.Respository.Medra
{
    public class MeddraLowLevelTermRepository : GenericRespository<MeddraLowLevelTerm, GscContext>, IMeddraLowLevelTermRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraLowLevelTermRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public int AddLltFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\llt.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraLowLevelTerm
                    {
                        MedraConfigId = obj.MedraId,
                        llt_code = Convert.ToInt64(allField[0]),
                        llt_name = allField[1],
                        pt_code = Convert.ToInt64(allField[2]),
                        llt_whoart_code = allField[3],
                        llt_harts_code = allField[4] == "" ? (long?)null : Convert.ToInt64(allField[4]),
                        llt_costart_sym = allField[5],
                        llt_icd9_code = allField[6],
                        llt_icd9cm_code = allField[7],
                        llt_icd10_code = allField[8],
                        llt_currency = allField[9],
                        llt_jart_code = allField[10]
                    });

                }
                count = Lines.Length;
            }
            return count;
        }

        public IList<MeddraCodingSearchDetails> GetManualCodes(MeddraCodingSearchDto search)
        {
            var query = All.AsQueryable();

            if (search.SearchBy == 0)
            {
                query = query.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && x.llt_name.Contains(search.Value));
            }
            else if (search.SearchBy == 1)
            {
                query = query.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && x.llt_name.StartsWith(search.Value));
            }
            else
            {
                query = query.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId) && x.DeletedDate == null && search.Value == x.llt_name);
            }

            return GetItems(query, search.MeddraConfigId);
        }


        private IList<MeddraCodingSearchDetails> GetItems(IQueryable<Data.Entities.Medra.MeddraLowLevelTerm> query, int MeddraConfigId)
        {
            return (from q in query
                    join mpt in Context.MeddraPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == MeddraConfigId) on q.pt_code equals mpt.pt_code
                    join soc in Context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == MeddraConfigId) on mpt.pt_soc_code equals soc.soc_code
                    join hpt in Context.MeddraHltPrefComp.Where(t => t.DeletedDate == null && t.MedraConfigId == MeddraConfigId) on mpt.pt_code equals hpt.pt_code
                    join hlt in Context.MeddraHltPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == MeddraConfigId) on hpt.hlt_code equals hlt.hlt_code
                    join hlgtHLT in Context.MeddraHlgtHltComp.Where(t => t.DeletedDate == null && t.MedraConfigId == MeddraConfigId) on hlt.hlt_code equals hlgtHLT.hlt_code
                    join hlgt in Context.MeddraHlgtPrefTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == MeddraConfigId) on hlgtHLT.hlgt_code equals hlgt.hlgt_code
                    join md in Context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == MeddraConfigId) on q.pt_code equals md.pt_code
                    select new MeddraCodingSearchDetails
                    {
                        LLTValue = q.llt_name,
                        SocCode = soc.soc_code,
                        PT = mpt.pt_name,
                        HLT = hlt.hlt_name,
                        HLGT = hlgt.hlgt_name,
                        SOCValue = soc.soc_name,
                        PrimarySoc = md.primary_soc_fg,
                        MeddraConfigId = MeddraConfigId,
                        MeddraLowLevelTermId = q.Id
                    }).ToList();
        }
    }
}
