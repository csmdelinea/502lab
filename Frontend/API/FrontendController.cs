using Microsoft.AspNetCore.Mvc;
using System.Net.Sockets;
using System.Net.WebSockets;
using ToRefactor;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Frontend.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class FrontendController : ControllerBase
    {
        [HttpGet("Sockets")]
        public List<SocketModel> GetSockets()
        {
            ConnectionMonitorService.RefreshSockets();
            var result = ConnectionMonitorService.GetSocketModels().OrderBy(n => n.Created).ToList();

            return result;
        }

        [HttpPut("CloseSocket/{id}")]
        public async Task<IActionResult> CloseSocket(string id, CancellationToken token)
        {

            var socket = ConnectionMonitorService.GetSocketModels().SingleOrDefault(n => n.Id == id);
            if (socket == null)
                return NotFound(id);
            //   if(!TunnelExensions.Sockets.ContainsKey(id))
            var webSocket = TunnelExensions.WebSockets.Single(n => n.Key == socket.Id);
            webSocket.Value.Dispose();

            socket.State = webSocket.Value.State.ToString();
            //var socket = _frontendMessageHandler.GetWebSocketById(id);
            //if (socket == null)
            //    return NotFound(id);

            // var socket = TunnelExensions.Sockets.Single(n => n.Key == id);
            ////socket.Dispose();
            ////await socket.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Closed", token);
            //// var model = new SocketModel { Id = id, State = socket.State.ToString() };
            ////TunnelExensions.Sockets.
            //await socket.Value.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Close Requested by API", token);
            //var model = new SocketModel { Id = socket.Key, State = socket.Value.State.ToString() };
            return Ok(socket);
        }

    }
}
