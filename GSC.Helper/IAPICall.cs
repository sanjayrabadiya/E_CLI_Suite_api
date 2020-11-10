using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;

namespace GSC.Helper
{
    public interface IAPICall
    {
      //  string Get(int Id, string controllername);
        string Post<T>(T data, string controllername);
        string Put<T>(T data, string controllername);
        string Delete(int Id, string controllername);
        string Patch(int Id, string controllername, object data);

    }
}
