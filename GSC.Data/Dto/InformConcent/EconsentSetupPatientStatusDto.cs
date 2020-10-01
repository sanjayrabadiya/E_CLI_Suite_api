using GSC.Data.Entities.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace GSC.Data.Dto.InformConcent
{
    public class EconsentSetupPatientStatusDto : BaseDto
    {
        [Required(ErrorMessage = "Econsent Document is required.")]
        public int EconsentDocumentId { get; set; }

        [Required(ErrorMessage = "Patient Status is required.")]
        public int PatientStatusId { get; set; }
    }
}
