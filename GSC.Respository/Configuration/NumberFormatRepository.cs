using System;
using System.Linq;
using GSC.Common.GenericRespository;
using GSC.Common.UnitOfWork;
using GSC.Data.Entities.Configuration;
using GSC.Domain.Context;
using GSC.Helper;

namespace GSC.Respository.Configuration
{
    public class NumberFormatRepository : GenericRespository<NumberFormat, GscContext>, INumberFormatRepository
    {
        public NumberFormatRepository(IUnitOfWork<GscContext> uow,
            IJwtTokenAccesser jwtTokenAccesser)
            : base(uow, jwtTokenAccesser)
        {
        }


        public string GenerateNumber(string keyName)
        {
            var result = FindBy(x => x.KeyName == keyName && x.DeletedDate == null).FirstOrDefault();
            if (result == null)
                return null;
            var separate = result.SeparateSign ?? "";
            var number = GetPrefix(result.PrefixFormat, separate);
            number += GetYear(result.YearFormat) + separate;
            number += GetMonth(result.MonthFormat) + separate;
            number += (result.StartNumber + 1).ToString().PadLeft(result.NumberLength, '0') + separate;
            number = number.Replace(" ", "").Replace("//", "/").Replace("--", "-");

            result.StartNumber += 1;
            Update(result);
            return number.ToUpper();
        }

        public string GetNumberFormat(string keyName, int number)
        {
            var result = FindBy(x => x.KeyName == keyName && x.DeletedDate == null).FirstOrDefault();
            if (result == null)
                return null;
            var separate = result.SeparateSign ?? "";
            var numberPrefix = GetPrefix(result.PrefixFormat, separate);
            numberPrefix += GetYear(result.YearFormat) + separate;
            numberPrefix += GetMonth(result.MonthFormat) + separate;
            numberPrefix += (number + 1).ToString().PadLeft(result.NumberLength, '0') + separate;
            numberPrefix = numberPrefix.Replace(" ", "").Replace("//", "/").Replace("--", "-");

            return numberPrefix.ToUpper();
        }

        private string GetYear(string year)
        {
            if (string.IsNullOrEmpty(year)) return "";

            if (year.ToUpper() == "YY")
                return DateTime.Now.ToString("yy");
            if (year.ToUpper() == "YYYY")
                return DateTime.Now.ToString("yyyy");

            return "";
        }

        private string GetMonth(string month)
        {
            if (string.IsNullOrEmpty(month)) return "";

            return DateTime.Now.ToString("MM");
        }

        private string GetPrefix(string prefix, string separate)
        {
            if (string.IsNullOrEmpty(prefix)) return "";

            prefix = prefix.ToUpper();
            prefix = prefix.Replace("##", separate.ToLower());
            prefix = prefix.Replace("##", "");

            return prefix;
        }
    }
}