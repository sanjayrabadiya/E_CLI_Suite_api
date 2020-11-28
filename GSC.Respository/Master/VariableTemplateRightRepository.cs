using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Master;
using GSC.Data.Entities.Master;
using GSC.Domain.Context;
using GSC.Shared.JWTAuth;

namespace GSC.Respository.Master
{
    public class VariableTemplateRightRepository : GenericRespository<VariableTemplateRight>,
        IVariableTemplateRightRepository
    {
        public VariableTemplateRightRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
        }

        public void SaveTemplateRights(VariableTemplateRightDto templateRightDto)
        {
            var savedTemplateRights = FindBy(t => t.SecurityRoleId == templateRightDto.SecurityRoleId).ToList();
            var deleteTemplateRights =
                savedTemplateRights.Where(t => !templateRightDto.VariableTemplateIds.Contains(t.VariableTemplateId));
            deleteTemplateRights.ToList().ForEach(Delete);

            templateRightDto.VariableTemplateIds.ForEach(variableTemplateId =>
            {
                var savedTemplateRight =
                    savedTemplateRights.FirstOrDefault(t => t.VariableTemplateId == variableTemplateId);
                if (savedTemplateRight != null)
                {
                    savedTemplateRight.DeletedBy = null;
                    savedTemplateRight.DeletedDate = null;

                    Update(savedTemplateRight);
                }
                else
                {
                    savedTemplateRight = new VariableTemplateRight
                    {
                        VariableTemplateId = variableTemplateId,
                        SecurityRoleId = templateRightDto.SecurityRoleId
                    };

                    Add(savedTemplateRight);
                }
            });
        }
    }
}