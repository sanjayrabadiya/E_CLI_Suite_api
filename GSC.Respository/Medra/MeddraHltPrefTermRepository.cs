using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using GSC.Shared.JWTAuth;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MeddraHltPrefTermRepository : GenericRespository<MeddraHltPrefTerm>, IMeddraHltPrefTermRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraHltPrefTermRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        public int AddHltFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\hlt.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraHltPrefTerm
                    {
                        MedraConfigId = obj.MedraId,
                        hlt_code = Convert.ToInt64(allField[0]),
                        hlt_name = allField[1],
                        hlt_whoart_code = allField[2],
                        hlt_harts_code = allField[3] == "" ? (long?)null : Convert.ToInt64(allField[3]),
                        hlt_costart_sym = allField[4],
                        hlt_icd9_code = allField[5],
                        hlt_icd9cm_code = allField[6],
                        hlt_icd10_code = allField[7],
                        hlt_jart_code = allField[8]
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
