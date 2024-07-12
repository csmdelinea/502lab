using System.Net;

var builder = WebApplication.CreateBuilder(args);

//builder.WebHost.UseKestrel(options =>
//{
//    options.
//});//builder.WebHost.UseKestrel(options =>
//{
//    options.
//});

builder.Services.AddReverseProxy()
       .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

builder.Logging.AddLog4Net();

// This is the HTTP/2 endpoint to register this app as part of the cluster endpoint
var url = builder.Configuration["Tunnel:Url"]!;

//overload command line arg for url
if (args.Length > 0 && Uri.TryCreate(args[0].ToString(), UriKind.Absolute, out var test))
    url = test.ToString();

builder.WebHost.UseTunnelTransport(url, options =>
{
    options.Transport = url.Contains("connect-h2") ? TransportType.HTTP2 : TransportType.WebSockets;
});

var app = builder.Build();


app.MapReverseProxy();

app.UseWhen(context => context.Request.Path.Value?.Contains("/api/health") == true, applicationBuilder =>
{
    applicationBuilder.UseWebSockets();

});
app.MapGet("/api/health", (HttpContext context) =>
{
    return HttpStatusCode.OK;
});

   


app.Run();
