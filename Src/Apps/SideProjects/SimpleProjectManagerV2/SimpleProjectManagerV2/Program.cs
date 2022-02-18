
var builder = WebApplication.CreateBuilder(args);

// Additional configuration is required to successfully run gRPC on macOS.
// For instructions on how to configure Kestrel and gRPC clients on macOS, visit https://go.microsoft.com/fwlink/?linkid=2099682

// Add services to the container.
builder.Services.AddGrpc();
builder.Services.AddControllersWithViews();

#if DEBUG
builder.Services.AddSpaYarp();
#endif



var app = builder.Build();

Console.WriteLine($"Dev: {app.Environment.IsDevelopment()}");

Console.WriteLine(app.Configuration["ASPNETCORE_HOSTINGSTARTUPASSEMBLIES"]);

app.UseStaticFiles();
app.UseRouting();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller}/{action=Index}/{id?}");

app.MapFallbackToFile("index.html");

#if DEBUG
app.UseDeveloperExceptionPage();
app.UseSpaYarp();
#endif

app.Run();