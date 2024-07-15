﻿using Frontend.Monitor;
using Microsoft.AspNetCore.Mvc;
using Yarp.ReverseProxy.Configuration;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Frontend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DiagnosticsController : ControllerBase
    {

        private readonly IProxyConfigProvider _proxyConfigProvider;
        private readonly HealthMonitor _healthMonitor;

        public DiagnosticsController(IProxyConfigProvider proxyConfigProvider,
            HealthMonitor healthMonitor)
        {
            _proxyConfigProvider = proxyConfigProvider;
            _healthMonitor = healthMonitor;
        }


        // GET: api/<DiagnosticsController>
        [HttpGet]
        public DiagnosticsViewModel Get()
        {
            var config = _proxyConfigProvider.GetConfig();
            var result = new DiagnosticsViewModel();
            result.Clusters = config.Clusters.Select(n => new ClusterConfigModel
            {
                Model = n,
                LastHealthyProbeUtc = _healthMonitor.MonitorStats.SingleOrDefault(o => o.Id == n.ClusterId)?.LastHealthy
            }).ToList();
            result.Routes = config.Routes.Select(n => new RouteConfigModel
            {
                Model = n,
                LastHealthyProbeUtc = _healthMonitor.MonitorStats.SingleOrDefault(o => o.Id == n.ClusterId)?.LastHealthy
            }).ToList();
            result.Tunnels = new List<string> { "Tunnel 1", "Tunnel 2" };
            result.HybridVaultLocations = new List<HybridVaultLocationModel>
            {
                new HybridVaultLocationModel
                {
                    TenantId = Guid.NewGuid(),
                    Id = Guid.NewGuid(),
                    ServerNames = "server1, server2",
                    VaultId = Guid.NewGuid(),
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                },

                new HybridVaultLocationModel
                {
                    TenantId = Guid.NewGuid(),
                    Id = Guid.NewGuid(),
                    ServerNames = "server3, server4",
                    VaultId = Guid.NewGuid(),
                    CreatedOn = DateTime.UtcNow,
                    ModifiedOn = DateTime.UtcNow
                },
            };

            return result;

        }

        // GET api/<DiagnosticsController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<DiagnosticsController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
        }

        // PUT api/<DiagnosticsController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<DiagnosticsController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }


        public class DiagnosticsViewModel
        {
            public List<string> Tunnels { get; set; } = new List<string>();
            public List<RouteConfigModel> Routes { get; set; } = new List<RouteConfigModel>();
            public List<ClusterConfigModel> Clusters { get; set; } = new List<ClusterConfigModel>();
            public List<HybridVaultLocationModel> HybridVaultLocations { get; set; } = new List<HybridVaultLocationModel>();
        }

        // had serialization issue with two id properties
        public class HybridVaultLocationModel
        {
            public Guid TenantId { get; set; }

            /// <summary>
            /// Gets or sets the identifier.
            /// </summary>
            public Guid Id { get; set; }

            /// <summary>
            /// Gets or sets the server hosting the proxy connection to the on-premise hybrid vault
            /// </summary>
            public string ServerNames { get; set; }
            /// <summary>
            /// UUID of the vault
            /// </summary>
            public Guid VaultId { get; set; }
            /// <summary>
            ///     Datetime the record was Created
            /// </summary>
            public DateTime CreatedOn { get; set; }
            /// <summary>
            ///     Who Modified this last
            /// </summary>
            public DateTime ModifiedOn { get; set; }



        }

        public class ClusterConfigModel
        {
            public ClusterConfig Model { get; set; }
            public DateTime? LastHealthyProbeUtc { get; set; }
        }

        public class RouteConfigModel
        {
            public RouteConfig Model { get; set; }
            public DateTime? LastHealthyProbeUtc { get; set; }
        }

    }
}
