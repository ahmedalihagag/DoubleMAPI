using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http.Headers;
using System.Text;
using System.Threading.Tasks;

namespace BLL.Services
{
    public class BunnyCDNStorage
    {
        private readonly string _storageZone;
        private readonly string _apiKey;
        private readonly string _pullZoneUrl; // Public URL
        private readonly HttpClient _httpClient;

        public BunnyCDNStorage(string storageZone, string apiKey, string pullZoneUrl)
        {
            _storageZone = storageZone;
            _apiKey = apiKey;
            _pullZoneUrl = pullZoneUrl.TrimEnd('/');
            _httpClient = new HttpClient();
            _httpClient.DefaultRequestHeaders.Add("AccessKey", _apiKey);
        }

        // Upload a file stream
        public async Task UploadAsync(string path, Stream stream)
        {
            var url = $"https://storage.bunnycdn.com/{_storageZone}/{path.Replace("\\", "/")}";
            using var content = new StreamContent(stream);
            content.Headers.ContentType = new MediaTypeHeaderValue("application/octet-stream");

            var response = await _httpClient.PutAsync(url, content);
            response.EnsureSuccessStatusCode();
        }

        // Delete a file by path
        public async Task DeleteAsync(string path)
        {
            var url = $"https://storage.bunnycdn.com/{_storageZone}/{path.Replace("\\", "/")}";
            var request = new HttpRequestMessage(HttpMethod.Delete, url);
            var response = await _httpClient.SendAsync(request);
            response.EnsureSuccessStatusCode();
        }

        // Get public URL for a file
        public string GetUrl(string path)
        {
            return $"{_pullZoneUrl}/{path.Replace("\\", "/")}";
        }

        // Convert public URL back to path
        public string GetPathFromUrl(string url)
        {
            if (!url.StartsWith(_pullZoneUrl))
                throw new ArgumentException("URL does not belong to this storage");

            return url.Substring(_pullZoneUrl.Length + 1); // remove domain + slash
        }
    }
}
