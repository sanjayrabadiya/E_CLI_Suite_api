using GSC.Common.GenericRespository;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Etmf;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Respository.Etmf
{
    public interface IPdfViewerRepository : IGenericRepository<ProjectWorkplaceArtificatedocument>
    {

        void SaveDocument(Dictionary<string, string> jsonObject);
        object Load(Dictionary<string, string> jsonData);
    }
}
