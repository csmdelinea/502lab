using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;

namespace Frontend.Monitor
{
    public class HealthMonitor
    {

        private readonly IProxyConfigProvider _proxyConfigProvider;
        private readonly ILogger<HealthMonitor> _logger;
        private readonly IAvailableDestinationsPolicy _availableDestinationsPolicy;
        private readonly IActiveHealthCheckMonitor _activeHealthCheckMonitor;

        public HealthMonitor(IProxyConfigProvider proxyConfigProvider, ILogger<HealthMonitor> logger, IActiveHealthCheckMonitor activeHealthCheckMonitor, IAvailableDestinationsPolicy availableDestinationsPolicy)
        {
            _proxyConfigProvider = proxyConfigProvider;
            _logger = logger;
            _activeHealthCheckMonitor = activeHealthCheckMonitor;
            _availableDestinationsPolicy = availableDestinationsPolicy;
        }


        public void StartMonitor()
        {
            Task.Run(() => RunPeriodicTaskAsync(TimeSpan.FromSeconds(10), CheckHealth));
        }

        async Task RunPeriodicTaskAsync(TimeSpan interval, Func<Task> action)
        {
            while (true)
            {
                var delayTask = Task.Delay(interval);
                await delayTask;
                await action();

            }
        }

        async Task CheckHealth()
        {
            if (_proxyConfigProvider.GetConfig().ChangeToken.HasChanged)
            {
                var y = "x";
            }
            //var cb = _proxyConfigProvider.GetConfig().ChangeToken.RegisterChangeCallback((s) =>
            //{
            //    var x = "Y";
            //},null);
            foreach (var routeConfig in _proxyConfigProvider.GetConfig().Routes)
            {
                var x = routeConfig;
            }
            foreach (var clusterConfig in _proxyConfigProvider.GetConfig().Clusters)
            {
               
                var x = clusterConfig;
                //clusterConfig.HealthCheck
            }


        }
    }
}
