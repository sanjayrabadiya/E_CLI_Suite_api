using Microsoft.Extensions.Configuration;
using System;
using System.Collections.Generic;
using System.Text;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Formatting;
using Newtonsoft.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Builder;
using System.Web.Http;
using System.Net;

namespace GSC.Helper
{
    public class APICall : IAPICall
    {
        private readonly IConfiguration _configuration;
        private readonly HttpClient _httpClient;
        public APICall(IConfiguration configuration, HttpClient httpClient)
        {
            _configuration = configuration;
            _httpClient = httpClient;
        }

        public string Get(string URL)
        {
            //string URL = String.Format("{0}{1}/{2}", _configuration["EndPointURL"], controllername, Id);
            // HttpClient client = new HttpClient();
           // _httpClient.BaseAddress = new Uri(URL);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            HttpResponseMessage response = _httpClient.GetAsync(URL).Result;
            //  client.Dispose();
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new HttpRequestException(response.Content.ReadAsStringAsync().Result);
        }

        public string Post<T>(T data, string URL)
        {  
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = _httpClient.PostAsJsonAsync(URL, data).Result;
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new HttpResponseException(response);
        }

        public string Put<T>(T data, string URL)
        {
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = _httpClient.PutAsJsonAsync(URL, data).Result;
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new HttpResponseException(response);
            //return response;
        }

        public string Delete(string URL)
        {
            //string URL = String.Format("{0}{1}/{2}",
            //              _configuration["EndPointURL"], controllername, Id);
            //HttpClient client = new HttpClient();
           // _httpClient.BaseAddress = new Uri(URL);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var response = _httpClient.DeleteAsync(URL).Result;
            //client.Dispose();
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new HttpResponseException(response);

        }

        public string Patch(string URL, object data)
        {
            //string URL = String.Format("{0}{1}/{2}",
            //              _configuration["EndPointURL"], controllername, Id);
            // HttpClient client = new HttpClient();
           // _httpClient.BaseAddress = new Uri(URL);
            _httpClient.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));
            var content = new ObjectContent<object>(data, new JsonMediaTypeFormatter());
            HttpResponseMessage response = _httpClient.PatchAsync(URL, content).Result;
            if (response.IsSuccessStatusCode)
                return response.Content.ReadAsStringAsync().Result;
            else
                throw new HttpResponseException(response);
        }

    }
}
