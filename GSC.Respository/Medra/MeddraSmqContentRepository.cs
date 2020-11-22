using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Dto.Medra;
using GSC.Data.Entities.Medra;
using GSC.Domain.Context;
using GSC.Respository.PropertyMapping;
using GSC.Shared;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace GSC.Respository.Medra
{
    public class MeddraSmqContentRepository : GenericRespository<MeddraSmqContent>, IMeddraSmqContentRepository
    {
        private IPropertyMappingService _propertyMappingService;
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        public MeddraSmqContentRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser, IPropertyMappingService propertyMappingService) : base(context)
        {
            _propertyMappingService = propertyMappingService;
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public int AddSmqContentFileData(SaveFileDto obj)
        {
            int count = 0;
            string[] paths = { obj.Path, obj.FolderType.ToString(), obj.Language, obj.Version, obj.RootName };
            var fullPath = Path.Combine(paths) + "\\Unzip\\MedAscii\\smq_content.asc";
            if (File.Exists(fullPath))
            {
                var Lines = File.ReadAllLines(fullPath);
                foreach (var line in Lines)
                {
                    var allField = line.Split('$');
                    Add(new Data.Entities.Medra.MeddraSmqContent
                    {
                        MedraConfigId = obj.MedraId,
                        smq_code = Convert.ToInt64(allField[0]),
                        term_code = Convert.ToInt64(allField[1]),
                        term_level = Convert.ToInt32(allField[2]),
                        term_scope = Convert.ToInt32(allField[3]),
                        term_category = allField[4],
                        term_weight = Convert.ToInt32(allField[5]),
                        term_status = allField[6],
                        term_addition_version = allField[7],
                        term_last_modified_version = allField[8]
                    });
                }
                count = Lines.Length;
            }
            return count;
        }
    }
}
