using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Net.WebSockets;
using System.Threading.Tasks;
using System.Text;
using Controllers;
using Newtonsoft.Json.Linq;

namespace Views
{
    public struct ClientSettings
    {
        public int UpdateIntervalInMilliseconds;

        public static ClientSettings CreateDefault()
        {
            return new ClientSettings
            {
                UpdateIntervalInMilliseconds = 10
            };
        }
    }
    public class ClientView : IObserver<UICommand>
    {
        private ClientSettings settings;

        private Boolean abortRequested;
        private WebSocket socket;

        private Queue<ServerCommand> commandsIn;
        private Queue<UICommand> commandsOut;

        private Dictionary<Guid, ICommandHandle> awaitingResponse;

        public ClientView(WebSocket socket)
        {
            settings = ClientSettings.CreateDefault();

            abortRequested = false;
            this.socket = socket;

            this.commandsIn = new Queue<ServerCommand>();
            this.commandsOut = new Queue<UICommand>();

            this.awaitingResponse = new Dictionary<Guid, ICommandHandle>();
        }

        public ServerCommand nextCommandIn()
        {
            if (this.commandsIn.Count > 0) return commandsIn.Dequeue();
            else return null;
        }

        private Command nextCommandOut()
        {
            if (this.commandsOut.Count > 0) return commandsOut.Dequeue();
            else return null;
        }

        public void Run()
        {
            var buffer = new byte[1024 * 4];

            Console.WriteLine("ClientView started. ({0})", socket.SubProtocol);

            ServerCommand commandIn;
            Task<WebSocketReceiveResult> receiveTask = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
            
            while (socket.State == WebSocketState.Open)
            { 
                UICommand commandOut;
                lock(commandsOut) while (commandsOut.TryDequeue(out commandOut)) SendMessage(commandOut.ToJson());

                if (receiveTask.IsCompleted)
                {
                    commandIn = CommandParser.Parse(Encoding.UTF8.GetString(buffer).Trim('\0'));
                    commandsIn.Enqueue(commandIn);
                    buffer = new byte[1024 * 4];

                    receiveTask.Dispose();
                    receiveTask = socket.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                }

                Thread.Sleep(settings.UpdateIntervalInMilliseconds);
            }
            try
            {
                socket.CloseAsync(WebSocketCloseStatus.NormalClosure, "", CancellationToken.None);
                Console.WriteLine("Closed websocket.");
            } catch (WebSocketException ex)
            {
                Console.WriteLine("Could not close websocket! Was it already closed? {0}", ex.Message);
            }
        }

        private bool SendMessage(string message)
        {
            byte[] buffer = Encoding.UTF8.GetBytes(message);
            try
            {
                lock(socket)
                {
                    socket.SendAsync(new ArraySegment<byte>(buffer, 0, message.Length), WebSocketMessageType.Text, true, CancellationToken.None);
                }
                return true;
            }
            catch (Exception e)
            {
                Console.WriteLine("Error while sending information to client, probably a Socket disconnect {0}", e);
                return false;
            }
        }

        public void OnCompleted()
        {
            throw new NotImplementedException();
        }

        public void OnError(Exception error)
        {
            throw new NotImplementedException();
        }

        public void OnNext(UICommand value)
        {
            lock(commandsOut) commandsOut.Enqueue(value);
        }

        public CommandHandle<T> OnNext<T>(T command) where T : UICommand
        {
            CommandHandle<T> handle = new CommandHandle<T>(command);
            awaitingResponse.Add(command.id, handle);
            OnNext(command as UICommand);
            
            return handle;
        }

        internal void Abort()
        {
            socket.Abort();
        }
    }
}