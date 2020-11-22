using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Dto.Barcode.Generate;
using GSC.Data.Entities.Barcode.Generate;
using GSC.Domain.Context;
using GSC.Shared;
using Microsoft.EntityFrameworkCore;

namespace GSC.Respository.Barcode.Generate
{
    public class BarcodeGenerateRepository : GenericRespository<BarcodeGenerate, GscContext>, IBarcodeGenerateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;

        public BarcodeGenerateRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
        }

        public List<BarcodeGenerateDto> GetBarcodeGenerate(bool isDeleted)
        {
            var barcodeGenerateDetails = All.Where(x =>
                    (x.CompanyId == null || x.CompanyId == _jwtTokenAccesser.CompanyId)
                    && isDeleted ? x.DeletedDate != null : x.DeletedDate == null).OrderByDescending(x => x.Id).Select(c => new BarcodeGenerateDto
                    {
                        Id = c.Id,
                        IsDeleted = c.DeletedDate != null
                    }
                )
                .ToList();
            return barcodeGenerateDetails;
        }

        public async Task<List<BarcodeGenerateDto>> GetGenerateBarcodeDetail(int[] templateId)
        {
            return await  (from barcodeGenerate in Context.BarcodeGenerate.Where(x =>
                    templateId.Contains(x.ProejctDesignTemplateId))
                join barcodeSubjectDetail in Context.BarcodeSubjectDetail.Where(t => t.DeletedDate == null) on
                    barcodeGenerate.Id equals barcodeSubjectDetail.BarcodeGenerateId
                join projectSubject in Context.ProjectSubject.Where(t => t.DeletedDate == null) on barcodeSubjectDetail
                    .ProjectSubjectId equals projectSubject.Id
                join projectDesignTemplate in Context.ProjectDesignTemplate.Where(t => t.DeletedDate == null) on
                    barcodeGenerate.ProejctDesignTemplateId equals projectDesignTemplate.Id
                join project in Context.Project.Where(t => t.DeletedDate == null) on projectSubject.ProjectId equals
                    project.Id
                join users in Context.Users.Where(t => t.DeletedDate == null) on barcodeGenerate.CreatedBy equals users
                    .Id
                from volunteer in Context.Volunteer
                    .Where(x => x.DeletedBy == null && x.Id == projectSubject.VolunteerId).DefaultIfEmpty()
                select new BarcodeGenerateDto
                {
                    TemplateCode = projectDesignTemplate.TemplateCode,
                    VolunteerName = volunteer.FirstName + ' ' + volunteer.MiddleName + ' ' + volunteer.LastName,
                    VolunteerNo = volunteer.VolunteerNo,
                    SubjectNumber = projectSubject.Number,
                    ProjectCode = project.ProjectCode,
                    GeneratedBy = users.UserName,
                    GeneratedOn = barcodeGenerate.CreatedDate,
                    BarcodeLabelString = barcodeSubjectDetail.BarcodeLabelString
                }).ToListAsync();
        }
    }
}