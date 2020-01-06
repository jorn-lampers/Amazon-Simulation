using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Models;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Controllers {

    public abstract class Command {

        protected string type;
        protected Object parameters;

        public Command(string type, Object parameters) {
            this.type = type;
            this.parameters = parameters;
        }

        public string ToJson() {
            return JsonConvert.SerializeObject(new {
                command = type,
                parameters = parameters
            });
        }
    }

    public abstract class Model3DCommand : Command
    {
        public Model3DCommand(string type, Entity parameters) : base(type, parameters) {
        }
    }

    /// <summary>
    /// Base class of Commands sent by ClientView to Server (SimulationController)
    /// </summary>
    public class ViewCommand : Command
    {
        public ViewCommand(string type, Object parameters) : base(type, parameters)
        {
        }

        public static ViewCommand Parse(string json)
        {
            ViewCommand c = JsonConvert.DeserializeObject<ViewCommand>(json);

            switch(c.type)
            {
                case "TestCommand": return JsonConvert.DeserializeObject<TestCommand>(json);
                default: throw new Exception("Unknown command type: '" + c.type + "'");
            }
        }

        public virtual void Execute(World model)
        {
            Console.WriteLine("Execution of command requested: " + JsonConvert.SerializeObject(this));
        }
    }

    public class UpdateModel3DCommand : Model3DCommand
    {
        public UpdateModel3DCommand(Entity parameters) : base("update", parameters) {
        }
    }

    public class TestCommand : ViewCommand
    {
        public TestCommand(Guid parameters) : base("test", parameters)
        {
            Console.WriteLine("Constructor of TestCommand was called!");
        }

        public override void Execute(World model)
        {
            Console.WriteLine("Executing test command :D {0}", JsonConvert.SerializeObject(parameters));
        }
    }
}