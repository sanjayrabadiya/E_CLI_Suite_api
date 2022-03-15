using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using Microsoft.EntityFrameworkCore;
using System.Linq;

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


        public bool IsSameValue(ScreeningTemplateValue screeningTemplateValue)
        {
            var result = All.AsNoTracking().Where(x => x.ProjectDesignVariableValueId == screeningTemplateValue.Id).Select(t => new
            {
                t.Value,
                t.ProjectDesignVariableValueId
            }).ToList();

            if (screeningTemplateValue.Children != null)
            {
                foreach (var x in screeningTemplateValue.Children.ToList())
                {
                    if (!result.Any(r => r.Value == x.Value && r.ProjectDesignVariableValueId == x.ProjectDesignVariableValueId))
                        return false;
                }
            }

            return true;
        }
    }
}