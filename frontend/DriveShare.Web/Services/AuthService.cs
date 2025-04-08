using System.Net.Http.Json;
using DriveShare.Web.Models;
using System.Text.Json;

namespace DriveShare.Web.Services;

public class AuthService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public AuthService(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
    }

    public string GetBaseAddress() => _httpClient.BaseAddress?.ToString() ?? "Not set";

    public async Task<(bool success, string? error)> RegisterAsync(RegisterDto model)
    {
        try
        {
            var requestUrl = $"{_httpClient.BaseAddress}api/account/register";
            Console.WriteLine($"Making registration request to: {requestUrl}");
            Console.WriteLine($"Request payload: {JsonSerializer.Serialize(model, _jsonOptions)}");
            
            var response = await _httpClient.PostAsJsonAsync("api/account/register", model, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Response status: {response.StatusCode}");
            Console.WriteLine($"Response content: {content}");

            if (response.IsSuccessStatusCode)
            {
                return (true, null);
            }

            try
            {
                var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string[]>>(content, _jsonOptions);
                if (errorResponse != null && errorResponse.Any())
                {
                    return (false, string.Join(" ", errorResponse.SelectMany(x => x.Value)));
                }
            }
            catch (JsonException ex)
            {
                Console.WriteLine($"Failed to deserialize error response: {ex.Message}");
                Console.WriteLine($"Raw content: {content}");
            }

            return (false, content);
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"HTTP request failed: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return (false, $"Failed to connect to the server: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during registration: {ex.Message}");
            Console.WriteLine($"Stack trace: {ex.StackTrace}");
            return (false, "An unexpected error occurred during registration.");
        }
    }
} 