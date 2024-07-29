//using Backend.Monitor;
//using Microsoft.AspNetCore.Mvc;

//// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

//namespace Backend.API
//{
//    [Route("api/[controller]")]
//    [ApiController]
//    public class BackendController : ControllerBase
//    {
//        // GET: api/<BackendController>
//        [HttpGet]
//        public IEnumerable<ConnectionViewModel> Get()
//        {
//            var connections = ConnectionMonitor.Instance.GetConnections();
//          var result = connections.Select(FromConnection).ToList();

//            //contexts.Where(n => n.UnderlyingWebSocket != null).Select(n => n.UnderlyingWebSocket.)
//            return result;
//        }

//        // GET api/<BackendController>/5
//        [HttpGet("{id}")]
//        public string Get(int id)
//        {
//            return "value";
//        }

//        // POST api/<BackendController>
//        [HttpPost]
//        public void Post([FromBody] string value)
//        {
//        }

//        // PUT api/<BackendController>/5
//        [HttpPut("{id}")]
//        public void Put(int id, [FromBody] string value)
//        {
//        }

//        // DELETE api/<BackendController>/5
//        [HttpDelete("{id}")]
//        public void Delete(int id)
//        {
//        }

//        public ConnectionViewModel FromConnection(TrackLifetimeConnectionContext context)
//        {
//            var result = new ConnectionViewModel();
//            result.Id = context.ConnectionId;
//            var webSocket = context.GetWebSocketConnectionContext().UnderlyingWebSocket;
//            if (webSocket != null)
//            {
//                result.SocketState = webSocket.State.ToString();
//            }

//            return result;
//        }

//        public class ConnectionViewModel
//        {
//            public string Id { get; set; }
//            public string SocketState { get; set; }
//        }
//    }
//}
