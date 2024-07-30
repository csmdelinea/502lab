using Backend.Monitor;
using Microsoft.AspNetCore.Mvc;
using ToRefactor;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Backend.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class BackendController : ControllerBase
    {
        // GET: api/<BackendController>
        [HttpGet]
        public IEnumerable<ConnectionViewModel> Get()
        {
            var connections = ConnectionMonitor.Instance.GetConnections();
            var result = connections.Select(FromConnection).ToList();

            //contexts.Where(n => n.UnderlyingWebSocket != null).Select(n => n.UnderlyingWebSocket.)
            return result;
        }

        [HttpPut("CloseSocket/{id}")]
        public async Task<IActionResult> CloseSocket(string id, CancellationToken token)
        {
            var connections = ConnectionMonitor.Instance.GetConnections();
            var selected = connections.Single(n => n.ConnectionId == id);
            await ConnectionMonitor.Instance.CloseConnection(selected);
            return Ok(FromConnection(selected));
        }

        [HttpPut("AbortSocket/{id}")]
        public IActionResult AbortSocket(string id, CancellationToken token)
        {
            var connections = ConnectionMonitor.Instance.GetConnections();
            var selected = connections.Single(n => n.ConnectionId == id);
            ConnectionMonitor.Instance.AbortConnection(selected);
            return Ok(FromConnection(selected));
        }

        public ConnectionViewModel FromConnection(TrackLifetimeConnectionContext context)
        {
            var result = new ConnectionViewModel();
            result.Id = context.ConnectionId;

            var webSocket = context.GetWebSocketConnectionContext().UnderlyingWebSocket;
            if (webSocket != null)
            {
                result.SocketState = webSocket.State.ToString();
                
            }

            return result;
        }

        public class ConnectionViewModel
        {
            public string Id { get; set; }
            public string SocketState { get; set; }
        }
    }
}
