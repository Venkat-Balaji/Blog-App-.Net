    using ClientBlog.Models;
    using System.Net.Http.Json;

    namespace ClientBlog.Services
    {
        public class AuthService
        {
            private readonly HttpClient _httpClient;

            public AuthService(HttpClient httpClient)
            {
                _httpClient = httpClient;
            }

            public async Task<string> Register(User user)
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/register", user);
                return response.IsSuccessStatusCode ? "Registration successful." : await response.Content.ReadAsStringAsync();
            }

            public async Task<string> Login(User user)
            {
                var response = await _httpClient.PostAsJsonAsync("api/Auth/login", user);
                if (!response.IsSuccessStatusCode)
                    return await response.Content.ReadAsStringAsync();

                var result = await response.Content.ReadFromJsonAsync<LoginResponse>();
                return result?.Token ?? "";
            }
        }

        public class LoginResponse
        {
            public string Token { get; set; } = string.Empty;
        }
    }
