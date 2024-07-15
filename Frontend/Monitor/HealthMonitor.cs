using Yarp.ReverseProxy.Configuration;
using Yarp.ReverseProxy.Health;

namespace Frontend.Monitor
{
    public class HealthMonitor
    {
        public List<HealthMonitorStats> MonitorStats { get; set; } = new List<HealthMonitorStats>();

        public void UpdateHealth(string id)
        {
           var target =  MonitorStats.SingleOrDefault(n => n.Id == id);
           if (target == null)
           {
               target = new HealthMonitorStats { Id = id };
               MonitorStats.Add(target);
           }

           target.LastHealthy=DateTime.UtcNow;
           

        }
    }

    public class HealthMonitorStats
    {
        public string Id { get; set; }
        public DateTime? LastHealthy { get; set; }
    }

}
