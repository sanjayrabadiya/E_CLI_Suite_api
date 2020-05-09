using System;
using System.Collections.Generic;
using System.Linq;

namespace GSC.Helper
{
    public static class Utility
    {
        public static bool IsAny<T>(this IEnumerable<T> data)
        {
            return data != null && data.Any();
        }

        public static string GetFullMessage(this Exception ex)
        {
            return ex.InnerException == null
                ? ex.Message
                : ex.InnerException.GetFullMessage();
        }
    }
}