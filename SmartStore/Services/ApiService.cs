using SmartStorePOS.Helpers;
using SmartStorePOS.Models;
using System.Configuration;
using System.IO;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace SmartStorePOS.Services
{
    public interface IApiService
    {
        Task<LoginResponse> LoginAsync(string email, string password);
        Task<UploadResponse> UploadImageAsync(string filePath);
        Task<Order> CreateOrderAsync(CreateOrderRequest request);
        Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request);
        string GenerateQrCodeUrl(string orderId);
        void SetToken(string token);
    }

    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private string _accessToken;
        private string BaseUrl = ConfigurationManager.AppSettings["BaseUrl"];

        public ApiService()
        {
            _httpClient = new HttpClient();
            _httpClient.BaseAddress = new Uri(BaseUrl);
        }

        public async Task<LoginResponse> LoginAsync(string email, string password)
        {
            var loginRequest = new LoginRequest
            {
                UserEmail = email,
                Password = password
            };

            var content = new StringContent(
                JsonSerializer.Serialize(loginRequest),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/auth/admin/login", content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var loginResponse = JsonSerializer.Deserialize<LoginResponse>(
                responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (loginResponse.IsSuccess)
            {
                _accessToken = loginResponse.AccessToken;
                _httpClient.DefaultRequestHeaders.Authorization =
                    new AuthenticationHeaderValue("Bearer", _accessToken);
            }

            return loginResponse;
        }

        public async Task<UploadResponse> UploadImageAsync(string filePath)
        {
            try
            {
                using var fileContent = new MultipartFormDataContent();
                var fileBytes = await File.ReadAllBytesAsync(filePath);
                var fileName = Path.GetFileName(filePath);
                var mimeType = MimeMapping.GetMimeType(filePath); // Lấy MIME type chính xác

                var byteArrayContent = new ByteArrayContent(fileBytes);
                byteArrayContent.Headers.ContentType = new MediaTypeHeaderValue(mimeType); // Gán Content-Type

                // Manually add Content-Disposition to mimic curl exactly
                byteArrayContent.Headers.Add("Content-Disposition", $"form-data; name=\"file\"; filename=\"{fileName}\"");

                fileContent.Add(byteArrayContent); // no need to pass "file" here again

                var response = await _httpClient.PostAsync("/upload", fileContent);

                if (response.IsSuccessStatusCode)
                {
                    var jsonResponse = await response.Content.ReadAsStringAsync();
                    var uploadResponse = JsonSerializer.Deserialize<UploadResponse>(jsonResponse, new JsonSerializerOptions
                    {
                        PropertyNameCaseInsensitive = true
                    });

                    return uploadResponse;
                }
                else
                {
                    throw new Exception($"Error: {response.StatusCode} - {await response.Content.ReadAsStringAsync()}");
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Upload failed: {ex.Message}");
            }
        }

        public async Task<Order> CreateOrderAsync(CreateOrderRequest request)
        {
            if (string.IsNullOrEmpty(_accessToken))
                throw new InvalidOperationException("User not authenticated");

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var apiUrl = "/api/order";
            if (ConfigurationManager.AppSettings["IsMockOrder"] == "true")
            {
                apiUrl = "/api/order/mock";
            }
            var response = await _httpClient.PostAsync(apiUrl, content);
            response.EnsureSuccessStatusCode();

            var responseString = await response.Content.ReadAsStringAsync();
            var orderResponse = JsonSerializer.Deserialize<Order>(
                responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return orderResponse;
        }

        public string GenerateQrCodeUrl(string orderId)
        {
            return $"aistore://payment/{orderId}";
        }

        public void SetToken(string token)
        {
            _accessToken = token;
            _httpClient.DefaultRequestHeaders.Authorization =
                new AuthenticationHeaderValue("Bearer", _accessToken);
        }

        /// <summary>
        /// Create a payment using membership card
        /// </summary>
        /// <param name="request"></param>
        /// <returns></returns>
        public async Task<PaymentResponse> CreatePaymentAsync(CreatePaymentRequest request)
        {
            if (string.IsNullOrEmpty(_accessToken))
                throw new InvalidOperationException("User not authenticated");

            var content = new StringContent(
                JsonSerializer.Serialize(request),
                Encoding.UTF8,
                "application/json");

            var response = await _httpClient.PostAsync("/api/payment", content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                return new PaymentResponse
                {
                    Status = "FAILED",
                    Msg = responseString
                };
            }

            var paymentResponse = JsonSerializer.Deserialize<PaymentResponse>(
                responseString,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            return paymentResponse;
        }
    }
}
