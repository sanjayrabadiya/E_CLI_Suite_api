using GSC.Shared;
using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;


namespace GSC.Shared    
{
    public class HttpService
    {
        public static async Task<T> Post<T>(HttpClient httpClient, string url, object parameter) where T : new()
        {
            string responseData;
            StringContent content;
            if (CheckObjectExtensions.Isnt(parameter, typeof(string)))
            {
                content = new StringContent(parameter.ToString());
            }
            else
            {
                content = new StringContent(parameter.ToJsonString());

            }
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
            return responseData.ToObject<T>();
        }

        public static async Task<string> Post(HttpClient httpClient, string url, object parameter)
        {
            string responseData;
            StringContent content;
            if (CheckObjectExtensions.Isnt(parameter, typeof(string)))
            {
                content = new StringContent(parameter.ToString());
            }
            else
            {
                content = new StringContent(parameter.ToJsonString());

            }
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PostAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
            return responseData;
        }

        public static async Task<T> Get<T>(HttpClient httpClient, string url, object parameter = null) where T : new()
        {
            string responseData;
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
            return responseData.ToObject<T>();
        }
        public static async Task<string> Get(HttpClient httpClient, string url, object parameter = null)
        {
            string responseData;
            var response = await httpClient.GetAsync(url);
            if (response.IsSuccessStatusCode)
            {
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
            return responseData;
        }
        public static async Task<T> Put<T>(HttpClient httpClient, string url, object parameter) where T : new()
        {
            string responseData;
            StringContent content;
            if (CheckObjectExtensions.Isnt(parameter, typeof(string)))
            {
                content = new StringContent(parameter.ToString());
            }
            else
            {
                content = new StringContent(parameter.ToJsonString());

            }
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
            return responseData.ToObject<T>();
        }

        public static async Task<string> Put(HttpClient httpClient, string url, object parameter)
        {
            string responseData;
            StringContent content;
            if (CheckObjectExtensions.Isnt(parameter, typeof(string)))
            {
                content = new StringContent(parameter.ToString());
            }
            else
            {
                content = new StringContent(parameter.ToJsonString());

            }
            content.Headers.ContentType = new MediaTypeHeaderValue("application/json");
            var response = await httpClient.PutAsync(url, content);
            if (response.IsSuccessStatusCode)
            {
                responseData = await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
            return responseData;
        }
        public static async Task Delete(HttpClient httpClient, string url)
        {
            var response = await httpClient.DeleteAsync(url);
            if (response.IsSuccessStatusCode)
            {
                await response.Content.ReadAsStringAsync();
            }
            else
            {
                throw new Exception(response.ReasonPhrase);
            }
        }


    }
    public static class CheckObjectExtensions
    {
        public static bool Isnt(this object source, Type targetType)
        {
            return source.GetType() == targetType;
        }
    }
}
