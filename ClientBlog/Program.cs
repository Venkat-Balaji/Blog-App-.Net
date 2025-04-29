using ClientBlog;
using ClientBlog.Services;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System.Net.Http;
using Blazored.LocalStorage;
using MudBlazor.Services;


var builder = WebAssemblyHostBuilder.CreateDefault(args);
builder.RootComponents.Add<App>("#app");
builder.RootComponents.Add<HeadOutlet>("head::after");
builder.Services.AddScoped<SupabaseService>();
builder.Services.AddScoped<AuthService>();
builder.Services.AddMudServices();
builder.Services.AddBlazoredLocalStorage();

builder.Services.AddSingleton(_ => new Supabase.Client(
    "https://ufvgudyqlgblrvadvmse.supabase.co",
    "eyJhbGciOiJIUzI1NiIsInR5cCI6IkpXVCJ9.eyJpc3MiOiJzdXBhYmFzZSIsInJlZiI6InVmdmd1ZHlxbGdibHJ2YWR2bXNlIiwicm9sZSI6ImFub24iLCJpYXQiOjE3NDM2NjIxMzMsImV4cCI6MjA1OTIzODEzM30.GzeDgoP1TIDPJ9c1yr2of01T4CfoYqWJDTQEdbA6mvw"
));
builder.Services.AddScoped(sp =>
{
    var nav = sp.GetRequiredService<NavigationManager>();
    var client = new HttpClient
    {
        BaseAddress = new Uri("https://localhost:7195/")
    };

    return client;
});

await builder.Build().RunAsync();
