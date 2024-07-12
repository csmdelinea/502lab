using System.Net;

var builder = WebApplication.CreateBuilder(args);
var app = builder.Build();

app.MapGet("/", () => "Hello World!");

app.MapGet("/api/health", () =>
{
    return HttpStatusCode.OK;
});

app.Run();
