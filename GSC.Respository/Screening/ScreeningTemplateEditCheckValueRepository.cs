using GSC.Common.GenericRespository;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using System.Linq;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateEditCheckValueRepository : GenericRespository<ScreeningTemplateEditCheckValue>, IScreeningTemplateEditCheckValueRepository
    {

        private readonly IGSCContext _context;
        public ScreeningTemplateEditCheckValueRepository(IGSCContext context) : base(context)
        {
            _context = context;
        }
        public bool CheckUpdateEditCheckRefValue(int screeningTemplateId, int projectDesignVariableId, int editCheckDetailId, EditCheckValidateType validateType, string sampleResult)
        {

            var screeningTemplateEditCheckValue = All.Where(t => t.ScreeningTemplateId == screeningTemplateId
                    && t.ProjectDesignVariableId == projectDesignVariableId &&
                    t.EditCheckDetailId == editCheckDetailId).FirstOrDefault();

            bool isFound = true;

            if (screeningTemplateEditCheckValue == null)
            {
                screeningTemplateEditCheckValue = new ScreeningTemplateEditCheckValue();
                screeningTemplateEditCheckValue.EditCheckRefValue = sampleResult;
                screeningTemplateEditCheckValue.ProjectDesignVariableId = projectDesignVariableId;
                screeningTemplateEditCheckValue.EditCheckDetailId = editCheckDetailId;
                screeningTemplateEditCheckValue.ValidateType = validateType;
                screeningTemplateEditCheckValue.ScreeningTemplateId = screeningTemplateId;
                Add(screeningTemplateEditCheckValue);
            }
            else
            {
                isFound = screeningTemplateEditCheckValue.EditCheckRefValue != sampleResult;
                screeningTemplateEditCheckValue.EditCheckRefValue = sampleResult;
                screeningTemplateEditCheckValue.ValidateType = validateType;
                Update(screeningTemplateEditCheckValue);

            }
            _context.Save();
            return isFound;
        }
    }
}
