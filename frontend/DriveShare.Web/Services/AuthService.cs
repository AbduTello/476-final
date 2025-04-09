using System.Net.Http.Json;
using DriveShare.Web.Models;
using System.Text.Json;
using System.Net.Http.Headers; // For AuthenticationHeaderValue
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;
using Microsoft.Extensions.Http;

namespace DriveShare.Web.Services;

public class AuthService
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly JsonSerializerOptions _jsonOptions;
    // Simple in-memory storage for the token. In a real app, use ProtectedBrowserStorage.
    private string? _authToken;
    private string? _userEmail;
    private readonly IJSRuntime _jsRuntime;
    private readonly NavigationManager _navigationManager;

    public event Action? AuthenticationStateChanged;

    public AuthService(IHttpClientFactory httpClientFactory, IJSRuntime jsRuntime, NavigationManager navigationManager)
    {
        _httpClientFactory = httpClientFactory;
        _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true
        };
        _jsRuntime = jsRuntime;
        _navigationManager = navigationManager;
    }

    public string GetBaseAddress() => _httpClientFactory.CreateClient("AuthService").BaseAddress?.ToString() ?? "Not set";

    // Method to check if user is currently considered authenticated by this service
    public bool IsAuthenticated()
    {
        if (string.IsNullOrEmpty(_authToken))
        {
            return false;
        }

        try
        {
            var handler = new JwtSecurityTokenHandler();
            var token = handler.ReadJwtToken(_authToken);
            return token.ValidTo > DateTime.UtcNow;
        }
        catch
        {
            return false;
        }
    }

    // Method to get the stored token
    public string? GetToken()
    {
        if (_authToken != null)
        {
            return _authToken;
        }

        try
        {
            // Try to get token from localStorage
            var tokenTask = _jsRuntime.InvokeAsync<string>("localStorage.getItem", "authToken").AsTask();
            tokenTask.Wait(); // Blocking call, but necessary for sync method
            _authToken = tokenTask.Result;
            return _authToken;
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error getting token from localStorage: {ex.Message}");
            return null;
        }
    }

    public async Task<string?> GetUserEmailAsync()
    {
        if (!string.IsNullOrEmpty(_userEmail))
        {
            return _userEmail;
        }

        var token = GetToken();
        if (!string.IsNullOrEmpty(token))
        {
            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                _userEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                Console.WriteLine($"GetUserEmailAsync: Found email in token: {_userEmail}");
                return _userEmail;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error parsing JWT token: {ex.Message}");
                return null;
            }
        }

        return null;
    }

    public async Task<(bool success, string? error)> RegisterAsync(RegisterDto model)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AuthService");
            var requestUrl = $"{client.BaseAddress}api/account/register";
            Console.WriteLine($"Making registration request to: {requestUrl}");
            Console.WriteLine($"Request payload: {JsonSerializer.Serialize(model, _jsonOptions)}");
            
            var response = await client.PostAsJsonAsync("api/account/register", model, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Register Response status: {response.StatusCode}");
            //Console.WriteLine($"Register Response content: {content}");

            if (response.IsSuccessStatusCode)
            {
                 // If registration directly logs in (returns token), handle it
                 var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
                 if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                 {
                     await SetAuthData(loginResponse.Token);
                 }
                 return (true, null);
            }

             // Handle error response as before
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

            return (false, content); // Return raw content on failure
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

    public async Task<(bool success, string? error)> LoginAsync(LoginDto model)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AuthService");
            Console.WriteLine($"Attempting login for: {model.Email}");
            var response = await client.PostAsJsonAsync("api/account/login", model);
            var content = await response.Content.ReadAsStringAsync();

            Console.WriteLine($"Login Response status: {response.StatusCode}");
            Console.WriteLine($"Login Response content: {content}");

            if (response.IsSuccessStatusCode)
            {
                var loginResponse = JsonSerializer.Deserialize<LoginResponseDto>(content, _jsonOptions);
                if (loginResponse != null && !string.IsNullOrEmpty(loginResponse.Token))
                {
                    Console.WriteLine("Login successful, setting auth data");
                    await SetAuthData(loginResponse.Token);
                    return (true, null);
                }
                else
                {
                    Console.WriteLine("Login succeeded but response format was invalid.");
                    return (false, "Invalid response from server.");
                }
            }
            else
            {
                // Try to parse standard error format first
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string[]>>(content, _jsonOptions);
                    if (errorResponse != null && errorResponse.Any())
                    {
                        return (false, string.Join(" ", errorResponse.SelectMany(x => x.Value)));
                    }
                }
                catch (JsonException) { /* Ignore if not standard error */ }
                // Return the raw content or a generic message if parsing fails or content is empty
                return (false, string.IsNullOrWhiteSpace(content) ? "Invalid credentials." : content);
            }
        }
        catch (HttpRequestException ex)
        {
            Console.WriteLine($"Login HTTP request failed: {ex.Message}");
            return (false, $"Failed to connect to the server: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during login: {ex.Message}");
            return (false, "An unexpected error occurred during login.");
        }
    }

    public async Task LogoutAsync()
    {
        // Clear auth data
        _authToken = null;
        _userEmail = null;
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");

        // Notify state change
        AuthenticationStateChanged?.Invoke();
    }

    public async Task<(bool success, string? error)> RecoverPasswordAsync(PasswordRecoveryDto model)
    {
        try
        {
            var client = _httpClientFactory.CreateClient("AuthService");
            Console.WriteLine($"Attempting password recovery for: {model.Email}");
            var response = await client.PostAsJsonAsync("api/account/recover", model, _jsonOptions);
            var content = await response.Content.ReadAsStringAsync();
            
            Console.WriteLine($"Recovery Response status: {response.StatusCode}");
            //Console.WriteLine($"Recovery Response content: {content}");

            if (response.IsSuccessStatusCode)
            {
                // Password recovery on backend doesn't return a token
                return (true, null);
            }
            else
            {
                 // Try to parse standard error format first
                try
                {
                    var errorResponse = JsonSerializer.Deserialize<Dictionary<string, string[]>>(content, _jsonOptions);
                    if (errorResponse != null && errorResponse.Any())
                    {
                        return (false, string.Join(" ", errorResponse.SelectMany(x => x.Value)));
                    }
                }
                catch (JsonException) { /* Ignore if not standard error */ }
                // Return the raw content or a generic message
                 return (false, string.IsNullOrWhiteSpace(content) ? "Password recovery failed." : content);
            }
        }
        catch (HttpRequestException ex)
        {
             Console.WriteLine($"Recovery HTTP request failed: {ex.Message}");
            return (false, $"Failed to connect to the server: {ex.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception during password recovery: {ex.Message}");
            return (false, "An unexpected error occurred during password recovery.");
        }
    }

    // --- Helper methods for simple token management --- 

    public async Task SetAuthData(string token)
    {
        Console.WriteLine("Setting auth data with token");
        _authToken = token;
        var client = _httpClientFactory.CreateClient("AuthService");
        client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
        
        try
        {
            var handler = new JwtSecurityTokenHandler();
            var tokenObj = handler.ReadJwtToken(token);
            _userEmail = tokenObj.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
            Console.WriteLine($"SetAuthData: Parsed email from token: {_userEmail}");
            
            // Store in localStorage for persistence
            await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "authToken", token);
            if (_userEmail != null)
            {
                await _jsRuntime.InvokeVoidAsync("localStorage.setItem", "userEmail", _userEmail);
            }

            // Notify about the state change
            Console.WriteLine("Auth data set, notifying state change");
            AuthenticationStateChanged?.Invoke();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error parsing JWT token: {ex.Message}");
            await ClearAuthData();
        }
    }

    public async Task ClearAuthData()
    {
        Console.WriteLine("Clearing auth data");
        _authToken = null;
        _userEmail = null;
        var client = _httpClientFactory.CreateClient("AuthService");
        client.DefaultRequestHeaders.Authorization = null;
        
        // Clear localStorage
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "authToken");
        await _jsRuntime.InvokeVoidAsync("localStorage.removeItem", "userEmail");
        
        // Notify about the state change
        Console.WriteLine("Auth data cleared, notifying state change");
        AuthenticationStateChanged?.Invoke();
    }

    private void NotifyAuthenticationStateChanged()
    {
        AuthenticationStateChanged?.Invoke();
    }
}

// Add LoginResponseDto class if not already present
public class LoginResponseDto
{
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? UserId { get; set; }
    public DateTime Expiration { get; set; }
} 