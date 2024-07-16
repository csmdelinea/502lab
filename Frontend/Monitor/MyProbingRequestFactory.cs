using System.Net;
using Microsoft.AspNetCore.Http.Extensions;
using Microsoft.Net.Http.Headers;
using System.Reflection;
using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;
using Yarp.ReverseProxy.Model;

namespace Frontend.Monitor
{



    public class MyProbingRequestFactory : IProbingRequestFactory
    {
        private static readonly string? _version = Assembly.GetExecutingAssembly().GetCustomAttribute<AssemblyInformationalVersionAttribute>()?.InformationalVersion;
        private static readonly string _defaultUserAgent = $"YARP{(string.IsNullOrEmpty(_version) ? "" : $"/{_version.Split('+')[0]}")} (Active Health Check Monitor)";
        private DiagnosticsService _diagnosticsService;
        


        public MyProbingRequestFactory(DiagnosticsService diagnosticsService)
        {
            _diagnosticsService = diagnosticsService;
        }





        public HttpRequestMessage CreateRequest(ClusterModel cluster, DestinationModel destination)
        {
            var probeAddress = !string.IsNullOrEmpty(destination.Config.Health) ? destination.Config.Health : destination.Config.Address;
            var probePath = cluster.Config.HealthCheck?.Active?.Path;
            UriHelper.FromAbsolute(probeAddress, out var destinationScheme, out var destinationHost, out var destinationPathBase, out _, out _);
            var query = QueryString.FromUriComponent(cluster.Config.HealthCheck?.Active?.Query ?? "");
            var probeUri = UriHelper.BuildAbsolute(destinationScheme, destinationHost, destinationPathBase, probePath, query);

            var request = new HttpRequestMessage(HttpMethod.Get, probeUri)
            {
                Version = cluster.Config.HttpRequest?.Version ?? HttpVersion.Version20,
                VersionPolicy = cluster.Config.HttpRequest?.VersionPolicy ?? HttpVersionPolicy.RequestVersionOrLower,
            };

            if (!string.IsNullOrEmpty(destination.Config.Host))
            {
                request.Headers.Add(HeaderNames.Host, destination.Config.Host);
            }

            request.Headers.Add(HeaderNames.UserAgent, _defaultUserAgent);

            if(!_diagnosticsService.ClusterProbingDictionary.ContainsKey(cluster.Config.ClusterId))
                _diagnosticsService.ClusterProbingDictionary.Add(cluster.Config.ClusterId,new ProbingModel(cluster,destination));
            //if(!_probeDictionary.Any(n=>n.Key.Config.ClusterId.Equals(cluster.Config.ClusterId)))
            //    _probeDictionary.Add(cluster,destination);
            return request;
        }

    }
}
