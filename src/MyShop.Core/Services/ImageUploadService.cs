using System;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Threading.Tasks;
using Windows.Storage;

namespace MyShop.Core.Services
{
    public interface IImageUploadService
    {
        Task<string> UploadProductImageAsync(StorageFile file);
        Task<bool> DeleteImageAsync(string imageUrl);
    }

    public class ImageUploadService : IImageUploadService
    {
        private readonly HttpClient _httpClient;
        private readonly string _baseUrl;

        public ImageUploadService(string baseUrl)
        {
            _httpClient = new HttpClient();
            _baseUrl = baseUrl;
        }

        public async Task<string> UploadProductImageAsync(StorageFile file)
        {
            try
            {
                // Read file as stream
                using var stream = await file.OpenStreamForReadAsync();
                using var content = new MultipartFormDataContent();
                using var fileContent = new StreamContent(stream);

                fileContent.Headers.ContentType = new MediaTypeHeaderValue(file.ContentType);
                content.Add(fileContent, "file", file.Name);

                // POST to backend
                var response = await _httpClient.PostAsync($"{_baseUrl}/api/upload/product-image", content);
                response.EnsureSuccessStatusCode();

                // Read response (expecting JSON with imageUrl field)
                var responseContent = await response.Content.ReadAsStringAsync();
                
                // Simple JSON parsing (assuming response: {"imageUrl": "..."})
                var urlStart = responseContent.IndexOf("\"imageUrl\":\"") + 12;
                var urlEnd = responseContent.IndexOf("\"", urlStart);
                var imageUrl = responseContent.Substring(urlStart, urlEnd - urlStart);

                return imageUrl;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image upload failed: {ex.Message}");
                throw new Exception($"Failed to upload image: {ex.Message}");
            }
        }

        public async Task<bool> DeleteImageAsync(string imageUrl)
        {
            try
            {
                var response = await _httpClient.DeleteAsync($"{_baseUrl}/api/upload/product-image?url={Uri.EscapeDataString(imageUrl)}");
                return response.IsSuccessStatusCode;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Image deletion failed: {ex.Message}");
                return false;
            }
        }
    }
}
