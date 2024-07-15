using System.Net;
using System.Runtime.CompilerServices;
using Frontend.Monitor;
using Microsoft.AspNetCore.Mvc;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Frontend.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class HealthController : ControllerBase
    {

        private readonly HealthMonitor _healthMonitor;

        public HealthController(HealthMonitor healthMonitor)
        {
            _healthMonitor = healthMonitor;
        }
        // GET api/<HealthController>/5
        [HttpGet("{id}")]
        public HttpResponseMessage Get(string id)
        {
            _healthMonitor.UpdateHealth(id);
            return new HttpResponseMessage(HttpStatusCode.OK);
        }

    }
}
