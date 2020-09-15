using GSC.Data.Entities.Audit;
using GSC.Data.Entities.LogReport;
using GSC.Data.Entities.Screening;
using GSC.Data.Entities.Volunteer;
using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Audit
{
    public class DictionaryCollection : IDictionaryCollection
    {
        readonly List<Dictionary> _dictionaries = new List<Dictionary>();
        public IList<Dictionary> Dictionaries { get; }
        public IList<string> SkipEntityForAudit { get; }

        public DictionaryCollection()
        {
            Dictionaries = LoadDictionaries();
            SkipEntityForAudit = GetSkipEntityForAudit();
        }


        List<Dictionary> LoadDictionaries()
        {
            SetDictionariesForMaster();

            return _dictionaries;
        }
        void SetDictionariesForMaster()
        {

            _dictionaries.Add(new Dictionary { FieldName = "TrialTypeId", DisplayName = "Trial Type", SourceColumn = "TrialTypeName", TableName = "TrialType", PkName = "Id" });
            _dictionaries.Add(new Dictionary { FieldName = "OtherTitle", DisplayName = "Other Title" });
            _dictionaries.Add(new Dictionary { FieldName = "Forename", DisplayName = "First Name" });
            _dictionaries.Add(new Dictionary { FieldName = "MiddleName", DisplayName = "Middle Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Surname", DisplayName = "Last Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DateOfBirth", DisplayName = "Date Of Birth" });
            _dictionaries.Add(new Dictionary { FieldName = "CorrespondenceAddressId", DisplayName = "Address", SourceColumn = "DisplayAddress", TableName = "Address", PkName = "AddressId" });
            _dictionaries.Add(new Dictionary { FieldName = "AddressId", DisplayName = "Address", SourceColumn = "DisplayAddress", TableName = "Address", PkName = "AddressId" });
        }



        private List<string> GetSkipEntityForAudit()
        {
            return new List<string> {
            nameof(AuditTrailCommon),
            nameof(UserLoginReport),
            nameof(AuditTrail),
            nameof(Volunteer),
            nameof(VolunteerAddress),
            nameof(VolunteerBiometric),
            nameof(VolunteerContact),
            nameof(VolunteerDocument),
            nameof(VolunteerFood),
            nameof(VolunteerHistory),
            nameof(VolunteerImage),
            nameof(VolunteerLanguage),
            nameof(ScreeningTemplateValue),
            nameof(ScreeningTemplateValueAudit)
            };

        }
    }
}
