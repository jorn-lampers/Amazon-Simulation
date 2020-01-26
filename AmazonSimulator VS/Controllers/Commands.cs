using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using System.Threading.Tasks;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Controllers {

    public interface ICommandHandle : IObservable<ICommandResponse>
    {
        void OnResponseReceived(ICommandResponse response);
    }

    public interface ICommandResponse
    {
        Guid GetCommandID();
        bool IsFinalResponse();
    }

    public class CommandHandle<T> : ICommandHandle where T: UICommand
    {
        private List<IObserver<ICommandResponse>> _observers;

        private T _command;
        private bool _sent;
        private Task _timeOutTask;

        public T Command { get => _command; }
        public bool IsCommandSent { get => _sent; }

        public CommandHandle(T command)
        {
            this._command = command;
            this._sent = false;
        }

        public void OnResponseReceived(ICommandResponse response)
        {
            _observers.ForEach(o => o.OnNext(response));

            if(response.IsFinalResponse())
                _observers.ForEach(o => o.OnCompleted());
        }

        public async Task TimeOutAfter(TimeSpan timeout)
        {
            _timeOutTask = new Task(() =>
            {
                _observers.ForEach(o => o.OnError(new TimeoutException("CommandResponse timed out after " + timeout.TotalSeconds + " ms!")));
            });
            await _timeOutTask;
        }

        public IDisposable Subscribe(IObserver<ICommandResponse> observer)
        {
            _observers.Add(observer);
            return new Unsubscriber<ICommandResponse>(this._observers, observer);
        }
    }

    public abstract class Command
    {
        public Guid id;
        public string type { get { return this.GetType().Name; } }
        protected Object parameters;

        public Command()
        {
            this.id = Guid.NewGuid();
        }

        public Command(Object parameters) : this() {
            this.parameters = parameters;
        }

        public string ToJson() {
            return JsonConvert.SerializeObject(new {
                id = id,
                command = type,
                parameters = parameters
            });
        }
    }

    public class CommandResponse<T> : ServerCommand, ICommandResponse where T : UICommand
    {
        private T _command;

        public Command Command { get; }

        public override void Execute(World model)
        {
            throw new NotImplementedException();
        }

        public Guid GetCommandID()
        {
            return _command.id;
        }

        public bool IsFinalResponse()
        {
            throw new NotImplementedException();
        }
    }

    public abstract class UICommand : Command
    {
        public UICommand(Object parameters) : base(parameters) {}
    }

    public abstract class Model3DCommand : UICommand
    {
        public Model3DCommand(Entity parameters) : base(parameters) {}
    }

    public class UpdateModel3DCommand : Model3DCommand
    {
        public UpdateModel3DCommand(Entity parameters) : base(parameters) {}
    }

    internal class DiscardModel3DCommand : UICommand
    {
        public DiscardModel3DCommand(Guid parameters) : base(parameters)
        {
        }
    }

    public class SimulationMetricsCommand : UICommand
    {
        public SimulationMetricsCommand(SimulationMetrics parameters) : base(parameters)
        {

        }
    }

    public abstract class ServerCommand : Command
    { 
        public abstract void Execute(World model);
    }

    public class TestCommand : ServerCommand
    {
        public Vector3 target;

        public override void Execute(World model)
        {
            Console.WriteLine("Moving to target: " + target);
            foreach (Robot r in model.GetObjectsOfType<Robot>())
            {
                if (r.IsAtDestination())
                {
                    r.AssignTask(new PathfinderTask(r, target, r.PathfindingGraph));
                }
            }
        }
    }

    public class ReceiveShipmentCommand : ServerCommand
    {
        public override void Execute(World model)
        {
            Console.WriteLine("Receiving shipment...");
            model.RunTask(new ReceiveShipmentTask(model));
        }
    }

    public class SendShipmentCommand : ServerCommand
    {
        public int amount = 1;

        public override void Execute(World model)
        {
            Console.WriteLine("Executing command: {0}", this);
            foreach(Entity u in model.GetObjects())
            {
                if (u is PathfindingEntity)
                {
                    PathfindingEntity r = (PathfindingEntity)u;
                    if (r.IsAtDestination())
                    {
                        Random random = new Random();
                        //Vector3 target = model.RobotGraph.Vertices[random.Next(0, model.RobotGraph.Vertices.Count - 1)];
                        //r.SetPathfindingTarget(target, model.RobotGraph);
                    }
                }
            }
        }
    }
}