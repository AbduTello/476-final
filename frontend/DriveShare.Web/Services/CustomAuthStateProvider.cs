using Microsoft.AspNetCore.Components.Authorization;
using System.Security.Claims;
using System.Threading.Tasks;
using System; // For IDisposable
using System.IdentityModel.Tokens.Jwt;
using System.Collections.Generic;
using System.Linq;

namespace DriveShare.Web.Services
{
    public class CustomAuthStateProvider : AuthenticationStateProvider, IDisposable
    {
        private readonly AuthService _authService;
        private Task<AuthenticationState>? _authenticationStateTask;
        private bool _disposed;
        private bool _isInitialized;
        private bool _hasRendered = false;

        // Constructor to inject the AuthService
        public CustomAuthStateProvider(AuthService authService)
        {
            _authService = authService;
            // Subscribe to the event from AuthService
            _authService.AuthenticationStateChanged += HandleAuthenticationStateChanged;
            Console.WriteLine("CustomAuthStateProvider: Subscribed to AuthService.AuthenticationStateChanged");
        }

        // This is the core method Blazor calls to get the current user's auth state
        public override async Task<AuthenticationState> GetAuthenticationStateAsync()
        {
            Console.WriteLine("CustomAuthStateProvider: GetAuthenticationStateAsync called");

            // During static rendering, return anonymous state
            if (!_hasRendered)
            {
                Console.WriteLine("CustomAuthStateProvider: Not rendered yet, returning anonymous state");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }

            try
            {
                // Get token from localStorage
                var token = _authService.GetToken();
                Console.WriteLine($"CustomAuthStateProvider: Token found: {!string.IsNullOrEmpty(token)}");

                if (string.IsNullOrEmpty(token))
                {
                    Console.WriteLine("CustomAuthStateProvider: No token found, returning anonymous state");
                    return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
                }

                // Create authenticated state
                var identity = await CreateIdentityFromToken(token);
                var user = new ClaimsPrincipal(identity);
                var state = new AuthenticationState(user);
                
                Console.WriteLine($"CustomAuthStateProvider: Returning authenticated state. IsAuthenticated: {state.User.Identity?.IsAuthenticated ?? false}");
                return state;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomAuthStateProvider: Error getting authentication state: {ex.Message}");
                return new AuthenticationState(new ClaimsPrincipal(new ClaimsIdentity()));
            }
        }

        private async Task<ClaimsIdentity> CreateIdentityFromToken(string token)
        {
            Console.WriteLine("CustomAuthStateProvider: Creating identity from token");
            var identity = new ClaimsIdentity();

            try
            {
                var handler = new JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);

                if (jwtToken.ValidTo > DateTime.UtcNow)
                {
                    var userEmail = jwtToken.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email)?.Value;
                    Console.WriteLine($"CustomAuthStateProvider: Creating authenticated identity for user: {userEmail}");
                    var claims = new List<Claim>();

                    if (!string.IsNullOrEmpty(userEmail))
                    {
                        claims.Add(new Claim(JwtRegisteredClaimNames.Email, userEmail));
                        claims.Add(new Claim(ClaimTypes.Name, userEmail));
                    }

                    var subject = jwtToken.Subject;
                    if (!string.IsNullOrEmpty(subject))
                    {
                        claims.Add(new Claim(ClaimTypes.NameIdentifier, subject));
                    }

                    var roles = jwtToken.Claims.Where(c => c.Type == ClaimTypes.Role).Select(c => c.Value);
                    foreach (var role in roles)
                    {
                        claims.Add(new Claim(ClaimTypes.Role, role));
                    }

                    identity = new ClaimsIdentity(claims, "apiauth", ClaimTypes.Name, ClaimTypes.Role);
                    Console.WriteLine($"CustomAuthStateProvider: Created identity with {claims.Count} claims");
                }
                else
                {
                    Console.WriteLine("CustomAuthStateProvider: Token is expired");
                    await _authService.ClearAuthData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CustomAuthStateProvider: Error validating token: {ex.Message}");
                await _authService.ClearAuthData();
            }

            return identity;
        }

        private async Task<AuthenticationState> CreateAuthenticationStateAsync()
        {
            Console.WriteLine("CustomAuthStateProvider: Creating authentication state");
            var identity = new ClaimsIdentity();

            var userEmail = await _authService.GetUserEmailAsync();
            var token = _authService.GetToken();

            Console.WriteLine($"CustomAuthStateProvider: Token found: {!string.IsNullOrEmpty(token)}");
            Console.WriteLine($"CustomAuthStateProvider: User email: {userEmail}");

            if (!string.IsNullOrEmpty(token))
            {
                identity = await CreateIdentityFromToken(token);
            }
            else
            {
                Console.WriteLine("CustomAuthStateProvider: No token found, creating unauthenticated identity");
                identity = new ClaimsIdentity(); // Create empty identity for unauthenticated state
            }

            var user = new ClaimsPrincipal(identity);
            var state = new AuthenticationState(user);

            Console.WriteLine($"CustomAuthStateProvider: Returning state. IsAuthenticated: {state.User.Identity?.IsAuthenticated ?? false}");
            return state;
        }

        // Method called by the event from AuthService
        private async void HandleAuthenticationStateChanged()
        {
            Console.WriteLine("CustomAuthStateProvider: Authentication state changed");
            
            // Reset all state
            _isInitialized = false;
            _authenticationStateTask = null;
            
            // Create new authentication state
            var authState = await CreateAuthenticationStateAsync();
            Console.WriteLine($"CustomAuthStateProvider: Created new auth state. IsAuthenticated: {authState.User.Identity?.IsAuthenticated ?? false}");
            
            // Notify about the state change
            NotifyAuthenticationStateChanged(Task.FromResult(authState));
            Console.WriteLine("CustomAuthStateProvider: Notified Blazor of state change");
        }

        // Implement IDisposable to unsubscribe from the event
        public void Dispose()
        {
            if (!_disposed)
            {
                Console.WriteLine("CustomAuthStateProvider: Disposing and unsubscribing from event.");
                _authService.AuthenticationStateChanged -= HandleAuthenticationStateChanged;
                _disposed = true;
                GC.SuppressFinalize(this);
            }
        }

        public void MarkAsRendered()
        {
            Console.WriteLine("CustomAuthStateProvider: Marking as rendered");
            _hasRendered = true;
            _isInitialized = false; // Reset initialization to force a new state check
            _authenticationStateTask = null; // Clear the cached state
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }
}