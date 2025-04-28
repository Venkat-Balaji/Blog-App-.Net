using Supabase;
using Supabase.Gotrue;

namespace ClientBlog.Services
{
    public class SupabaseService
    {
        private readonly Supabase.Client _client;

        public SupabaseService()
        {
            var options = new SupabaseOptions
            {
                AutoRefreshToken = true,         // Automatically refresh tokens when needed
                AutoConnectRealtime = true       // Connect to Supabase realtime database
            };

            // Initialize the Supabase client with your project URL and anon key
            _client = new Supabase.Client("https://pxbzrfiurzazjlqitmbi.supabase.co", "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InB4YnpyZml1cnphempscWl0bWJpIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDU0ODUyNDUsImV4cCI6MjA2MTA2MTI0NX0.7GzwFM99Q-Om8Z5CzOzjxmzaJHIvK8fayp2k37EwoNY", options);
        }

        // Handles login and returns true if successful
        public async Task<bool> Login(string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignIn(email, password);
                return session != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Handles registration and returns true if successful
        public async Task<bool> Register(string email, string password)
        {
            try
            {
                var session = await _client.Auth.SignUp(email, password);
                return session != null;
            }
            catch (Exception)
            {
                return false;
            }
        }

        // Get the current access token
        public string? GetToken() => _client.Auth.CurrentSession?.AccessToken;

        // Get the current logged-in user's ID
        public string? GetUserId() => _client.Auth.CurrentUser?.Id;

        // Logout user
        public async Task LogoutAsync()
        {
            try
            {
                await _client.Auth.SignOut();
            }
            catch (Exception ex)
            {
                Console.WriteLine("Error logging out: " + ex.Message);
            }
        }
    }
}
