using log4net;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace ToRefactor
{
    public class ConnectionTrackingLogger
    {
        private static ILog _log = LogManager.GetLogger(typeof(ConnectionTrackingLogger));

        public static void LogMessage<T>(string connectionIdentifier, string message)
        {
            _log.DebugFormat("{0} Connection Id: {1} - {2}", typeof(T), connectionIdentifier, message);
        }

        public static void LogMessage(Type callingType, string connectionIdentifier, string message)
        {
            _log.DebugFormat("{0} Connection Id: {1} - {2}", callingType, connectionIdentifier, message);
        }

        public static void LogWebSocket<T>(WebSocket? websocket, string connectionIdentifier, string message)
        {

            if (websocket != null)
            {
                var logMessage = string.Format("{0} Connection Id: {1} - {2}", typeof(T),
                    connectionIdentifier,
                    message);
                if (!string.IsNullOrWhiteSpace(websocket.State.ToString()))
                    logMessage = $"{logMessage} State: {websocket.State}.";
                if (!string.IsNullOrEmpty(websocket.CloseStatus.ToString()))
                    logMessage = $"{logMessage} Close Status: {websocket.CloseStatus}.";
                if (!string.IsNullOrEmpty(websocket.CloseStatusDescription))
                    logMessage = $"{logMessage} Close Status Description: {websocket.CloseStatusDescription}.";

                _log.DebugFormat(logMessage);
                return;
            }

            _log.DebugFormat("{0} Connection Id: {1} - {2}. Websocket is null", typeof(T), connectionIdentifier,
                message);
        }

        public static void LogException<T>(Exception exception, string connectionIdentifier, string message)
        {
            _log.ErrorFormat("{0} Connection Id: {1} - {2}: {3}. {4}", typeof(T), connectionIdentifier, message, exception.Message, exception);
        }
    }
}
