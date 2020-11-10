using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GSC.Helper
{
    public interface IAPICall
    {
        string Get(string URL);
        string Post<T>(T data, string URL);
        string Put<T>(T data, string URL);
        string Delete(string URL);
        string Patch(string URL, object data);

    }
}
