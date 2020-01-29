using Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Controllers
{
    public abstract class UICommand : Command
    {
        public UICommand(Object parameters) : base(parameters) { }
    }

    public abstract class Model3DCommand : UICommand
    {
        public Model3DCommand(Entity parameters) : base(parameters) { }
    }

    public class UpdateModel3DCommand : Model3DCommand
    {
        public UpdateModel3DCommand(Entity parameters) : base(parameters) { }
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
}
