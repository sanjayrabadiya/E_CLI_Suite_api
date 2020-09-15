using System.Collections.Generic;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Project.Design;
using GSC.Data.Dto.Screening;
using GSC.Data.Entities.Common;
using GSC.Data.Entities.Screening;
using GSC.Domain.Context;
using GSC.Helper;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Screening
{
    public class ScreeningTemplateValueEditCheckRepository :
        GenericRespository<ScreeningTemplateValueEditCheck, GscContext>, IScreeningTemplateValueEditCheckRepository
    {
        public ScreeningTemplateValueEditCheckRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }

        public void CloseSystemQuery(int screeningTemplateId, int projectDesignVariableId)
        {
            var queries = FindBy(x =>
                    x.ScreeningTemplateId == screeningTemplateId &&
                    x.ProjectDesignVariableId == projectDesignVariableId)
                .ToList();
            //queries.ForEach(x =>
            //{
            //    x.IsClosed = true;
            //    Update(x);
            //});
        }


        public void UpdateById(ScreeningTemplateValueEditCheck objSave)
        {
            objSave.ObjectState = ObjectState.Modified;
            Update(objSave);
        }

        public void Insert(ScreeningTemplateValueEditCheck objSave)
        {
            Add(objSave);
        }

        public void InsertUpdate(ScreeningTemplateValueEditCheckDto objSave, bool isUpdate)
        {
            if (objSave.ScreeningTemplateId == 0)
            {
                var screeningTemplate = Context.ScreeningTemplate.FirstOrDefault(c =>
                    c.ScreeningVisit.ScreeningEntryId == objSave.ScreeningEntryId
                    && c.ProjectDesignTemplateId == objSave.ProjectDesignTemplateId);
                if (screeningTemplate == null) return;
                objSave.ScreeningTemplateId = screeningTemplate.Id;
            }


            var screeningTemplateValueEditCheck = All.Where(c => c.EditCheckDetailId == objSave.EditCheckDetailId
                                                                 && c.ScreeningTemplateId ==
                                                                 objSave.ScreeningTemplateId).AsNoTracking()
                                                      .FirstOrDefault() ?? new ScreeningTemplateValueEditCheck();
            if (screeningTemplateValueEditCheck.Id == 0)
            {
                screeningTemplateValueEditCheck.ProjectDesignVariableId = objSave.ProjectDesignVariableId;
                screeningTemplateValueEditCheck.ScreeningTemplateId = objSave.ScreeningTemplateId;
                screeningTemplateValueEditCheck.ScreeningEntryId = objSave.ScreeningEntryId;
                screeningTemplateValueEditCheck.EditCheckDetailId = objSave.EditCheckDetailId;
                //screeningTemplateValueEditCheck.IsVerify = objSave.IsVerify;
                //screeningTemplateValueEditCheck.IsClosed = objSave.IsClosed;
                Add(screeningTemplateValueEditCheck);
            }
            else if (isUpdate)
            {
                //screeningTemplateValueEditCheck.IsClosed = objSave.IsClosed;
                //screeningTemplateValueEditCheck.IsVerify = objSave.IsVerify;
                Update(screeningTemplateValueEditCheck);
            }
        }

        public List<EditCheckTargetValidation> EditCheckSet(int screeningTemplateId, bool isFromQuery)
        {
            //var editCheckValidations = new List<EditCheckTargetValidation>();
            //return editCheckValidations;
            //var screeningTemplateValueEditCheck = Context.ScreeningTemplateValueEditCheck.Where(x =>
            //    x.ScreeningTemplateId == screeningTemplateId
            //    && x.EditCheckDetail.IsTarget
            //    && x.EditCheckDetail.DeletedDate == null
            //    && x.EditCheckDetail.EditCheck.DeletedDate == null
            //).AsNoTracking().Select(v => new
            //{
            //    v.ProjectDesignVariableId,
            //    v.EditCheckDetail.Message,
            //    v.ScreeningTemplateId,
            //    v.EditCheckDetail.EditCheck.AutoNumber,
            //    v.EditCheckDetail.Operator,
            //    v.ValidateType
            //}).ToList();

            //screeningTemplateValueEditCheck.ForEach(x =>
            //{
            //    var checkValidation = new EditCheckTargetValidation();
            //    checkValidation.ProjectDesignVariableId = x.ProjectDesignVariableId;
            //    checkValidation.IsEditCheck = true;
            //    if (x.Operator == Operator.Enable)
            //        checkValidation.EditCheckDisable = x.ValidateType == EditCheckValidateType.NotProcessed;

            //    if (x.Operator == Operator.HardFetch)
            //        checkValidation.EditCheckDisable = x.ValidateType != EditCheckValidateType.NotProcessed;

            //    if ((x.Operator == Operator.Required || x.Operator == Operator.Enable ||
            //         x.Operator == Operator.HardFetch) && x.ValidateType== EditCheckValidateType.ReferenceVerifed)
            //        checkValidation.OriginalValidationType = ValidationType.Hard;
            //    else if ((x.Operator == Operator.Optional || x.Operator == Operator.SoftFetch) && x.ValidateType == EditCheckValidateType.ReferenceVerifed)
            //        checkValidation.OriginalValidationType = ValidationType.Soft;

            //    if ((x.Operator == Operator.HardFetch || x.Operator == Operator.SoftFetch) && isFromQuery)
            //        checkValidation.Value = Context.ScreeningTemplateValue.FirstOrDefault(c =>
            //            c.ProjectDesignVariableId == x.ProjectDesignVariableId &&
            //            c.ScreeningTemplateId == x.ScreeningTemplateId)?.Value;

            //    checkValidation.IsInfo = x.ValidateType == EditCheckValidateType.ReferenceVerifed;

            //    //if (x.IsVerify) checkValidation.IsInfo = true;
            //    //if (x.IsClosed)
            //    //{
            //    //    checkValidation.IsInfo = true;
            //    //}
            //    //else
            //    //{
            //    //    checkValidation.HasQueries = Context.ScreeningTemplateValue.Any(c =>
            //    //        c.ProjectDesignVariableId == x.ProjectDesignVariableId &&
            //    //        c.ScreeningTemplateId == x.ScreeningTemplateId
            //    //        && c.IsSystem && c.QueryStatus == QueryStatus.Open);
            //    //    checkValidation.IsInfo = !checkValidation.HasQueries;
            //    //}
            //    checkValidation.EditCheckMsg = string.Join("\n ",
            //        screeningTemplateValueEditCheck.Where(b => b.ProjectDesignVariableId == x.ProjectDesignVariableId)
            //            .Select(s => "Edit Check : " + s.AutoNumber + " " + s.Message).Distinct().ToList());

            //    editCheckValidations.Add(checkValidation);
            //});

            //SceduleList(screeningTemplateId, editCheckValidations);

            //return editCheckValidations;

            return new List<EditCheckTargetValidation>();
        }

        private void SceduleList(int screeningTemplateId, List<EditCheckTargetValidation> editCheckValidations)
        {
            //var scheduleList = Context.ScreeningTemplateValueSchedule
            //    .Where(x => x.ScreeningTemplateId == screeningTemplateId).ToList();

            //if (scheduleList.Count() > 0)
            //    scheduleList.ForEach(x =>
            //    {
            //        var checkValidation = new EditCheckTargetValidation();
            //        var findEditCheck =
            //            editCheckValidations.FirstOrDefault(c =>
            //                c.ProjectDesignVariableId == x.ProjectDesignVariableId);

            //        if (findEditCheck != null && !string.IsNullOrEmpty(findEditCheck.EditCheckMsg))
            //            checkValidation.EditCheckMsg =
            //                findEditCheck.EditCheckMsg + "\n Date Time Validation " + x.Message;
            //        else
            //            checkValidation.EditCheckMsg = x.Message;
            //        checkValidation.IsInfo = x.IsVerify;
            //        if (x.IsClosed)
            //            checkValidation.IsInfo = true;

            //        checkValidation.ProjectDesignVariableId = x.ProjectDesignVariableId;
            //        editCheckValidations.Add(checkValidation);
            //    });
        }
    }
}