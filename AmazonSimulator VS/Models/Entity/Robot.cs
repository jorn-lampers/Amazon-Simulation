using AmazonSimulator_VS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Models
{
    public class Robot 
        : CollidablePathfindingEntity, 
        ICargoCarrier
    {
        private CargoSlot _cargo;
        private Queue<SimulationTask<Robot>> _tasks;
        private Graph _graph;
        private Vector3 _idlePos;

        public bool IsStandBy 
            => _tasks.Count == 0;
        public Graph PathfindingGraph 
            => _graph;
        public Vector3 IdlePos => _idlePos;


        public bool HasFreeCargoSlots 
            => _cargo.IsAvailable;
        public List<CargoSlot> CargoSlots 
            => new List<CargoSlot>() { _cargo };
        public List<CargoSlot> FreeCargoSlots 
            => _cargo.IsAvailable ? CargoSlots : new List<CargoSlot>();
        public List<CargoSlot> OccupiedCargoSlots 
            => _cargo.IsAvailable ? new List<CargoSlot>() : CargoSlots;

        public void AssignTask(SimulationTask<Robot> task)
            => _tasks.Enqueue(task);

        public Robot(EntityEnvironmentInfoProvider parent, float x, float y, float z, float rotationX, float rotationY, float rotationZ, Graph pathfindingGraph) 
            : base("robot", parent, x, y, z, rotationX, rotationY, rotationZ, Constants.RobotSpeed)
        {
            _cargo = new CargoSlot(this, new Vector3(0.0f, 0.0f, 0.0f));
            _graph = pathfindingGraph;
            _tasks = new Queue<SimulationTask<Robot>>();
            _idlePos = Position;
        }

        public Shelf ReleaseCargo()
            => this._cargo.ReleaseCargo();

        public bool TryAddCargo(Shelf item)
            => this._cargo.SetCargo(item);

        public override Vector3 Move(Vector3 pos)
        {
            return base.Move(pos);
        }

        public override Vector3 Move(float x, float y, float z)
        {
            return base.Move(x,y,z);
        }

        public override bool Tick(int tick)
        {
            if (this._tasks.Count > 0)
                if (_tasks.First().Tick())
                    _tasks.Dequeue();

            base.Tick(tick);
            _cargo.Tick(tick);

            return _needsUpdate;
        }

        public override void Destroy()
        {
            base.Destroy();
            CargoSlots.ForEach(s => s.Destroy());
        }
    }
}