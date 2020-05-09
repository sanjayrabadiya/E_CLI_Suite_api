using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Helper;
using GSC.Respository.PropertyMapping;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MeddraMdHierarchyRepository : GenericRespository<MeddraMdHierarchy, GscContext>, IMeddraMdHierarchyRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraMdHierarchyRepository(IUnitOfWork<GscContext> uow, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(uow, jwtTokenAccesser)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public int AddMdhierFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\mdhier.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraMdHierarchy
                    {
                        MedraConfigId = obj.MedraId,
                        pt_code = Convert.ToInt64(allField[0]),
                        hlt_code = Convert.ToInt64(allField[1]),
                        hlgt_code = Convert.ToInt64(allField[2]),
                        soc_code = Convert.ToInt64(allField[3]),
                        pt_name = allField[4],
                        hlt_name = allField[5],
                        hlgt_name = allField[6],
                        soc_name = allField[7],
                        soc_abbrev = allField[8],
                        null_field = allField[9],
                        pt_soc_code = allField[10] == "" ? (long?)null : Convert.ToInt64(allField[10]),
                        primary_soc_fg = allField[11]
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
