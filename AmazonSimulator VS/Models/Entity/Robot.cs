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
        private List<LineSegment2> _trail;

        public bool IsStandBy 
            => _tasks.Count == 0;
        public Graph GetPathfindingGraph() => _graph;
        public Vector3 IdlePos => _idlePos;

        public List<LineSegment2> Trail => _trail;

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
            : base("robot", parent, x, y, z, rotationX, rotationY, rotationZ, Constants.RobotSpeed, 5f, Constants.RobotAcceleration)
        {
            _cargo = new CargoSlot(this, new Vector3(0.0f, 0.0f, 0.0f));
            _graph = pathfindingGraph;
            _tasks = new Queue<SimulationTask<Robot>>();
            _idlePos = Position;
            _trail = this._footprint.AsLineSegments();
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

            var pos2 = new Vector2(this.Position.X, this.Position.Z);
            var tPos2 = new Vector2(this._target.X, this._target.Z) - pos2;

            var d = this.GetRequiredDistanceToFullStop() + 0.25f;
            var tDelta2 = new Vector2(0, 0);
            if(tPos2.Length() > 0) tDelta2 = Vector2.Normalize(tPos2) * d;

            _trail = this.Intersectable.AsCoveredAreaRect<List<LineSegment2>>(tDelta2);
            base.Tick(tick, _environment.GetCollisions(_trail).Where(h => h.Guid != this.Guid).Any());

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