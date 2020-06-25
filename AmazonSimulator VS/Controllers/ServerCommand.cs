using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Threading.Tasks;

namespace Controllers
{
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
            foreach (Robot r in model.ObjectsOfType<Robot>())
            {
                if (r.IsAtDestination())
                {
                    r.AssignTask(new PathfinderTask(r, target, r.GetPathfindingGraph()));
                }
            }
        }
    }

    public class ReceiveShipmentCommand : ServerCommand
    {
        public int amount;

        public override void Execute(World model)
        {
            Console.WriteLine("Receiving shipment...");
            model.RunTask(new ReceiveShipmentTask(model, amount));
        }
    }

    public class SendShipmentCommand : ServerCommand
    {
        public int amount;

        public override void Execute(World model)
        {
            Console.WriteLine("Sending shipment...");
            model.RunTask(new SendShipmentTask(model, amount));
        }
    }
}
