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
    public class BarcodeGenerateRepository : GenericRespository<BarcodeGenerate>, IBarcodeGenerateRepository
    {
        private readonly IJwtTokenAccesser _jwtTokenAccesser;
        private readonly IGSCContext _context;
        public BarcodeGenerateRepository(IGSCContext context,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(context)
        {
            _jwtTokenAccesser = jwtTokenAccesser;
           _context = context;
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
            return await  (from barcodeGenerate in _context.BarcodeGenerate.Where(x =>
                    templateId.Contains(x.ProejctDesignTemplateId))
                join barcodeSubjectDetail in _context.BarcodeSubjectDetail.Where(t => t.DeletedDate == null) on
                    barcodeGenerate.Id equals barcodeSubjectDetail.BarcodeGenerateId
                join projectSubject in _context.ProjectSubject.Where(t => t.DeletedDate == null) on barcodeSubjectDetail
                    .ProjectSubjectId equals projectSubject.Id
                join projectDesignTemplate in _context.ProjectDesignTemplate.Where(t => t.DeletedDate == null) on
                    barcodeGenerate.ProejctDesignTemplateId equals projectDesignTemplate.Id
                join project in _context.Project.Where(t => t.DeletedDate == null) on projectSubject.ProjectId equals
                    project.Id
                join users in _context.Users.Where(t => t.DeletedDate == null) on barcodeGenerate.CreatedBy equals users
                    .Id
                from volunteer in _context.Volunteer
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