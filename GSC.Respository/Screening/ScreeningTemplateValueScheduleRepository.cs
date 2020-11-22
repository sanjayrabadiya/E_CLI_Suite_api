using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Shared;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueScheduleRepository :
        GenericRespository<ScreeningTemplateValueSchedule>, IScreeningTemplateValueScheduleRepository
    {
        private readonly List<int> _projectDesignVariableId = new List<int>();

        public ScreeningTemplateValueScheduleRepository(IGSCContext context, IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }

        public void CloseSystemQuery(int screeningTemplateId, int projectDesignVariableId)
        {
            var queries = FindBy(x =>
                    x.ScreeningTemplateId == screeningTemplateId &&
                    x.ProjectDesignVariableId == projectDesignVariableId)
                .ToList();
            queries.ForEach(x =>
            {
                x.IsClosed = true;
                Update(x);
            });
        }

        public void InsertUpdate(ScreeningTemplateValueScheduleDto objSave)
        {
            if (_projectDesignVariableId != null &&
                _projectDesignVariableId.Any(c => c == objSave.ProjectDesignVariableId))
                return;

            var screeningTemplateValueScedule = All.Where(c =>
                                                        c.ProjectDesignVariableId == objSave.ProjectDesignVariableId
                                                        && c.ScreeningTemplateId == objSave.ScreeningTemplateId)
                                                    .AsNoTracking().FirstOrDefault() ??
                                                new ScreeningTemplateValueSchedule();
            screeningTemplateValueScedule.IsVerify = objSave.IsVerify;
            screeningTemplateValueScedule.IsStarted = objSave.IsStarted;
            if (screeningTemplateValueScedule.Id == 0)
            {
                screeningTemplateValueScedule.ProjectDesignVariableId = objSave.ProjectDesignVariableId;
                screeningTemplateValueScedule.ScreeningTemplateId = objSave.ScreeningTemplateId;
                screeningTemplateValueScedule.ScreeningEntryId = objSave.ScreeningEntryId;
                screeningTemplateValueScedule.Message = objSave.Message;
                Add(screeningTemplateValueScedule);
            }
            else
            {
                Update(screeningTemplateValueScedule);
            }

            if (_projectDesignVariableId != null) _projectDesignVariableId.Add(objSave.ProjectDesignVariableId);
        }
    }
}