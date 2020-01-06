using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using Controllers;

namespace Views
{
    public class ClientView : IObserver<Command>
    {
        private WebSocket socket;
        private Queue<ViewCommand> viewCommands;

        public ClientView(WebSocket socket)
        {
            this.socket = socket;
            this.viewCommands = new Queue<ViewCommand>();
        }

        public ViewCommand nextCommand()
        {
            if (this.viewCommands.Count > 0) return viewCommands.Dequeue();
            else return null;
        }

        public async Task StartReceiving()
        {
            var buffer = new byte[1024 * 4];

            Console.WriteLine("ClientView connection started");

            WebSocketReceiveResult result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            while (!result.CloseStatus.HasValue) // As long as client hasn't announced closing of stream ...
            {   // ... handle User-input sent by view
                string input = Encoding.UTF8.GetString(buffer);
                ViewCommand viewCommand = null;
                try
                {   // Input from view could be malformed due to various reasons
                    viewCommand = ViewCommand.Parse(input);
                } catch (Exception ex)
                {   // Log failed attempts to parse malformed ViewCommands
                    Console.WriteLine("Received a malformed command from ClientView: '{0}'", input);
                } finally
                {   // Only enqueue the command if it was parsed successfully
                    if (viewCommand != null) viewCommands.Enqueue(viewCommand);
                }

                result = await socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            }

            // ClientView requested abort
            Console.WriteLine("ClientView has disconnected");

            await socket.CloseAsync(result.CloseStatus.Value, result.CloseStatusDescription, CancellationToken.None);
        }

        private async void SendMessage(string message) {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            try {
                await socket.SendAsync(new ArraySegment<byte>(buffer, 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);
            } catch(Exception e) {
                Console.WriteLine("Error while sending information to client, probably a Socket disconnect");
            }
        }

        public void SendCommand(Command c) {
            SendMessage(c.ToJson());
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(Command value)
        {
            SendCommand(value);
        }
    }
}