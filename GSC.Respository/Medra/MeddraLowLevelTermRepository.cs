using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using System;
using System.Collections.Generic;
using System.Text;
using System.Linq;
using GSC.Data.Dto.Medra;
using System.IO;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Medra
{
    public class MeddraLowLevelTermRepository : GenericRespository<MeddraLowLevelTerm>, IMeddraLowLevelTermRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public MeddraLowLevelTermRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
            _context = context;
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

            return GetItems(query, search);
        }


        private IList<MeddraCodingSearchDetails> GetItems(IQueryable<Data.Entities.Medra.MeddraLowLevelTerm> query, MeddraCodingSearchDto search)
        {
            return (from q in query
                    join md in _context.MeddraMdHierarchy.Where(t => t.DeletedDate == null && t.MedraConfigId == search.MeddraConfigId) on q.pt_code equals md.pt_code
                    join soc in _context.MeddraSocTerm.Where(t => t.DeletedDate == null && t.MedraConfigId == search.MeddraConfigId) on md.soc_code equals soc.soc_code

                    select new MeddraCodingSearchDetails
                    {
                        LLTValue = q.llt_name,
                        SocCode = soc.soc_code.ToString(),
                        PT = md.pt_name,
                        HLT = md.hlt_name,
                        HLGT = md.hlgt_name,
                        SOCValue = soc.soc_name,
                        PrimarySoc = md.primary_soc_fg,
                        MeddraConfigId = search.MeddraConfigId,
                        MeddraLowLevelTermId = q.Id,
                        MeddraSocTermId = soc.Id,
                        LltCurrent = q.llt_currency
                    }).OrderBy(m => m.LLTValue.StartsWith(search.Value)
                                     ? (m.LLTValue == search.Value ? 0 : 1)
                                     : 2).ToList();
        }
    }
}
