using Azure.Core;
using Microsoft.Extensions.Options;
using System.IO;
using System.Threading.Tasks;



namespace ERP.Services
{

    public class YandexDiskSettings
    {
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string RedirectUri { get; set; }
    }

    //public interface IYandexDiskService
    //{
    //    Task UploadFileAsync(Stream fileStream, string fileName);
    //   Task CreateFolderAsync(string fileName);
    //}

    //public class YandexDiskService : IYandexDiskService
    //{
    //    private readonly HttpClient _httpClient;
    //    private readonly string _accessToken;

    //    public YandexDiskService(HttpClient httpClient, IOptions<YandexDiskSettings> options)
    //    {
    //        _httpClient = httpClient;
    //        _accessToken = options.Value.ClientSecret;
    //    }


    //    public async Task CreateFolderAsync(string path)
    //    {
    //        var requestUri = $"https://cloud-api.yandex.net/v1/disk/resources?path={Uri.EscapeDataString(path)}";
    //        var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
    //        request.Headers.Add("Authorization", $"OAuth {_accessToken}");

    //        var response = await _httpClient.SendAsync(request);
    //        response.EnsureSuccessStatusCode();
    //    }

    //    //public async Task UploadFileAsync(string path)
    //    //{
    //    //    var requestUri = $"https://cloud-api.yandex.net/v1/disk/upload?path={Uri.EscapeDataString(path)}";
    //    //    var request = new HttpRequestMessage(HttpMethod.Post, requestUri);
    //    //    request.Headers.Add("Authorization", $"OAuth {_accessToken}");

    //    //    var response = await _httpClient.SendAsync(request);
    //    //    response.EnsureSuccessStatusCode();
    //    //}


    //    public async Task UploadFileAsync(Stream fileStream, string fileName)
    //    {
    //        var requestUri = $"https://yandex.ru/v1/disk/resources";
    //        var request = new HttpRequestMessage(HttpMethod.Put, requestUri);
    //        request.Headers.Add("Authorization", $"OAuth {_accessToken}");
    //        request.Headers.Add("Accept", "application/json");
           



    //        var response = await _httpClient.SendAsync(request);
    //        var responseContent = await response.Content.ReadAsStringAsync();

    //        if (!response.IsSuccessStatusCode)
    //        {
    //            throw new Exception($"Ошибка {response.StatusCode}: {responseContent}");
    //        }
    //        response.EnsureSuccessStatusCode();

    //        var uploadUrl = await response.Content.ReadAsStringAsync();

    //        var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
    //        {
    //            Content = new StreamContent(fileStream)
    //        };

    //        var uploadResponse = await _httpClient.SendAsync(uploadRequest);
    //        uploadResponse.EnsureSuccessStatusCode();
    //    }

    //}

    ////public class YandexDiskService : IYandexDiskService
    ////{
    ////    private readonly HttpClient _httpClient;

    ////    public YandexDiskService(HttpClient httpClient)
    ////    {
    ////        _httpClient = httpClient;
    ////    }

    ////    public async Task UploadFileAsync(Stream fileStream, string fileName)
    ////    {
    ////        var encodedFileName = Uri.EscapeDataString(fileName);
    ////        var requestUri = $"resources/upload?path={encodedFileName}&overwrite=true";

    ////        var response = await _httpClient.GetAsync(requestUri);
    ////        var responseContent = await response.Content.ReadAsStringAsync();

    ////        if (!response.IsSuccessStatusCode)
    ////        {
    ////            throw new Exception($"Ошибка {response.StatusCode}: {responseContent}");
    ////        }
    ////        response.EnsureSuccessStatusCode();

    ////        var uploadUrl = await response.Content.ReadAsStringAsync();

    ////        var uploadRequest = new HttpRequestMessage(HttpMethod.Put, uploadUrl)
    ////        {
    ////            Content = new StreamContent(fileStream)
    ////        };

    ////        var uploadResponse = await _httpClient.SendAsync(uploadRequest);
    ////        uploadResponse.EnsureSuccessStatusCode();
    ////    }
    ////}
}
