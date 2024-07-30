using ToRefactor;

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
builder.Services.AddControllers();
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

app.UseStaticFiles();
app.UseRouting();
app.MapControllers();


//app.MapReverseProxy(proxyPipeline =>
//{
//    // Use a custom proxy middleware, defined below
//    proxyPipeline.UseExceptionHandlingMiddleware();
//    proxyPipeline.Use(DiagnosticPipeline.DiagnosticPipelineStep);
//    // Don't forget to include these two middleware when you make a custom proxy pipeline (if you need them).
//    proxyPipeline.UseSessionAffinity();
//    proxyPipeline.UseLoadBalancing();
//});
//app.Use(DiagnosticPipeline.DiagnosticPipelineStep);
//app.UseExceptionHandlingMiddleware();
app.MapReverseProxy();

app.Run();
