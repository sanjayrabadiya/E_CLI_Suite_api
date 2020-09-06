using System;
using System.Collections.Generic;
using System.Text;

namespace GSC.Audit
{
    public class DictionaryCollection : IDictionaryCollection
    {
        readonly List<Dictionary> _dictionaries = new List<Dictionary>();
        public IList<Dictionary> Dictionaries { get; }
        public DictionaryCollection()
        {
            Dictionaries = LoadDictionaries();
        }

        List<Dictionary> LoadDictionaries()
        {
            SetDictionariesForMaster();
           
            return _dictionaries;
        }
        void SetDictionariesForMaster()
        {

            _dictionaries.Add(new Dictionary { FieldName = "TitleId", DisplayName = "Title", SourceColumn = "Title", TableName = "LstTitle" });
            _dictionaries.Add(new Dictionary { FieldName = "OtherTitle", DisplayName = "Other Title" });
            _dictionaries.Add(new Dictionary { FieldName = "Forename", DisplayName = "First Name" });
            _dictionaries.Add(new Dictionary { FieldName = "MiddleName", DisplayName = "Middle Name" });
            _dictionaries.Add(new Dictionary { FieldName = "Surname", DisplayName = "Last Name" });
            _dictionaries.Add(new Dictionary { FieldName = "DateOfBirth", DisplayName = "Date Of Birth" });
            _dictionaries.Add(new Dictionary { FieldName = "CorrespondenceAddressId", DisplayName = "Address", SourceColumn = "DisplayAddress", TableName = "Address", PkName = "AddressId" });
            _dictionaries.Add(new Dictionary { FieldName = "AddressId", DisplayName = "Address", SourceColumn = "DisplayAddress", TableName = "Address", PkName = "AddressId" });
        }

      
    }
}
