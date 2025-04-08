using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using DriveShare.Web.Components;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();

// Configure HTTP client for the API
builder.Services.AddHttpClient("DriveShareApi", client =>
{
    // Use the value from configuration if available, 
    // otherwise default to backend running on localhost:5295.
    client.BaseAddress = new Uri(builder.Configuration["ApiSettings:BaseUrl"] ?? "http://localhost:5295");
    client.Timeout = TimeSpan.FromSeconds(
        int.Parse(builder.Configuration["ApiSettings:TimeoutSeconds"] ?? "30")
    );
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseAntiforgery();

app.MapStaticAssets();
app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
