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
    public class MeddraHlgtPrefTermRepository : GenericRespository<MeddraHlgtPrefTerm>, IMeddraHlgtPrefTermRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraHlgtPrefTermRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }
        public int AddHlgtFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\hlgt.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraHlgtPrefTerm
                    {
                        MedraConfigId = obj.MedraId,
                        hlgt_code = Convert.ToInt64(allField[0]),
                        hlgt_name = allField[1],
                        hlgt_whoart_code = allField[2],
                        hlgt_harts_code = allField[3] == "" ? (long?)null : Convert.ToInt64(allField[3]),
                        hlgt_costart_sym = allField[4],
                        hlgt_icd9_code = allField[5],
                        hlgt_icd9cm_code = allField[6],
                        hlgt_icd10_code = allField[7],
                        hlgt_jart_code = allField[8]
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
