using System.Net;

var builder = WebApplication.CreateBuilder(args);
builder.Services.AddControllers();

//builder.WebHost.ConfigureKestrel(options =>
//{
//    //options.Limits.RequestHeadersTimeout = TimeSpan.FromSeconds(5);
//});
builder.Services.AddControllers();
var app = builder.Build();
app.UseStaticFiles();

app.UseRouting();

app.MapGet("/", () => "Hello World!");
app.MapGet("/delay", () =>
{
    System.Threading.Thread.Sleep(TimeSpan.FromMinutes(1));
    return HttpStatusCode.OK;
});

app.UseEndpoints(endpoints =>
{
    endpoints.MapControllerRoute(
        name: "default",
        pattern: "{controller=Home}/{action=Index}/{id?}");
});
app.UseStaticFiles();
app.UseRouting();
app.MapControllers();

app.Run();
