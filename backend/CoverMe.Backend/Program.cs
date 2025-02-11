using CoverMe.Backend;
using CoverMe.Backend.Extensions;
using CoverMe.Backend.Helpers;

LoggingHelper.ConfigureLogging();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseContentRoot(AppContext.BaseDirectory);
builder.Services.AddRazorComponents()
    .AddInteractiveServerComponents();
builder.Services.AddControllers();
builder.WebHost.UseStaticWebAssets();
builder.Services
    .AddHostedServices()
    .AddAppServices()
    .AddManagers()
    .AddRepositories()
    .AddIpc();

var app = builder.Build();
if (!app.Environment.IsEnvironment("Headless") && !app.Environment.IsEnvironment("Intellij"))
{
    app.UseExceptionHandler("/Error", createScopeForErrors: true);
    app.UseHsts();
}

app.UseHttpsRedirection();

app.MapControllerRoute("default", "{controller=Channel}/{action=Post}");
app.UseStaticFiles();

app.UseAntiforgery();

app.MapRazorComponents<App>()
    .AddInteractiveServerRenderMode();

app.Run();
