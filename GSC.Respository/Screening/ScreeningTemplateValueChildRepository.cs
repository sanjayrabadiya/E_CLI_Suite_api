using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using System.Collections.Generic;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueChildRepository : GenericRespository<ScreeningTemplateValueChild>, IScreeningTemplateValueChildRepository
    {
        public ScreeningTemplateValueChildRepository(IGSCContext context) : base(context)
        {
        }


        public void Save(ScreeningTemplateValue screeningTemplateValue)
        {
            if (screeningTemplateValue.Children != null)
            {
                screeningTemplateValue.Children.ForEach(x =>
                {
                    if (x.Id == 0)
                        Add(x);
                    else
                        Update(x);
                });
            }
        }

        public void DeleteChild(List<int> Ids)
        {
            if (Ids != null)
            {
                Ids.ForEach(x =>
                {
                    var record = Find(x);
                    Delete(record);
                });
            }
        }

    }
}