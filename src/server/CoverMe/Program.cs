using CoverMe.Core.Extensions;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();
builder.Services.AddAppServices(builder.Environment);

var app = builder.Build();
app.UseHttpsRedirection();
app.UseRouting();
app.MapControllers();

if (app.Environment.IsDevelopment())
{
    app.MapWhen(e =>
            !e.Request.Path.StartsWithSegments("/api"),
        config => { config.UseSpa(spa => { spa.UseProxyToSpaDevelopmentServer(app.Configuration["Client:Url"]!); }); });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.Run();