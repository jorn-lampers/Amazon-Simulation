using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Controllers;

namespace Models
{
    public static class Constants
    {
        public static readonly Vector3[] RobotSpawns = {
            new Vector3(5f, 0f, -5f),
            new Vector3(2.5f, 0f, -5f),
            new Vector3(0f, 0f, -5f),
            new Vector3(-2.5f, 0f, -5f),
            new Vector3(-5f, 0f, -5f)
        };

        public static readonly Vector3 TruckSpawn = new Vector3(-50f, 0f, 25f);
        public static readonly Vector3 TruckStop = new Vector3(15f, 0f, 25f);
        public static readonly Vector3 TruckDespawn = new Vector3(50f, 0f, 25f);

        public static readonly Vector3 RobotEnterTruck = new Vector3(-6.5f, 0f, 26f);
        public static readonly Vector3 RobotExitTruck = new Vector3(-5.0f, 0f, 24f);

        public static readonly float LaneWidth = 2.0f;

        public static readonly Vector3[] GraphNodePositions =
        {
            new Vector3(-6.5f, 0.0f, 15.0f),
            new Vector3(+0.0f, 0.0f, 15.0f),
            new Vector3(+6.5f, 0.0f, 15.0f),

            new Vector3(-6.5f, 0.0f, 10.0f),
            new Vector3(+0.0f, 0.0f, 10.0f),
            new Vector3(+6.5f, 0.0f, 10.0f),

            new Vector3(-6.5f, 0.0f, 5.0f),
            new Vector3(+0.0f, 0.0f, 5.0f),
            new Vector3(+6.5f, 0.0f, 5.0f),

            new Vector3(-6.5f, 0.0f, 0.0f),
            new Vector3(+0.0f, 0.0f, 0.0f),
            new Vector3(+6.5f, 0.0f, 0.0f)
        };

        public static readonly Edge[] GraphEdges =
        {
            new Edge(GraphNodePositions[0], GraphNodePositions[1], LaneWidth),
            new Edge(GraphNodePositions[1], GraphNodePositions[2], LaneWidth),

            new Edge(GraphNodePositions[1], GraphNodePositions[4], LaneWidth),
            new Edge(GraphNodePositions[3], GraphNodePositions[4], LaneWidth),

            new Edge(GraphNodePositions[4], GraphNodePositions[5], LaneWidth),
            new Edge(GraphNodePositions[4], GraphNodePositions[7], LaneWidth),

            new Edge(GraphNodePositions[6], GraphNodePositions[7], LaneWidth),
            new Edge(GraphNodePositions[7], GraphNodePositions[8], LaneWidth),

            new Edge(GraphNodePositions[7], GraphNodePositions[10], LaneWidth),
            new Edge(GraphNodePositions[9], GraphNodePositions[10], LaneWidth),
            new Edge(GraphNodePositions[10], GraphNodePositions[11], LaneWidth)
        };

        public static readonly int StoragePlotLength = 5;
        public static readonly int StoragePlotWidth = 2;

        public static readonly Vector3[] StoragePositions =
        {
            new Vector3(-4f, 0.01f, 2.5f),
            new Vector3(+4f, 0.01f, 2.5f),

            new Vector3(-4f, 0.01f, 7.5f),
            new Vector3(+4f, 0.01f, 7.5f),

            new Vector3(-4f, 0.01f, 12.5f),
            new Vector3(+4f, 0.01f, 12.5f)
        };
    }

    public class World 
        : IObservable<UICommand>
        , IUpdatable
        , EntityEnvironmentInfoProvider
    {
        private List<Entity> worldObjects = new List<Entity>();
        private List<IObserver<UICommand>> observers = new List<IObserver<UICommand>>();
        private SimulationTask<World> _simulationTask;

        private Graph robotPathfindingGraph;
        private Graph truckPathfindingGraph;

        public List<Entity> Objects
            => this.worldObjects;

        public List<T> ObjectsOfType<T>() where T : Entity
            => Objects.Where(e => e is T)
            .Select(et => et as T).ToList();

        public Graph RobotGraph => robotPathfindingGraph; 
        public Graph TruckGraph => truckPathfindingGraph; 

        public World()
        {
            this._simulationTask = null;

            foreach (Vector3 pos in Constants.StoragePositions)
                CreateStoragePlot(pos.X, pos.Y, pos.Z, Constants.StoragePlotLength, Constants.StoragePlotWidth);

            robotPathfindingGraph = new Graph(Constants.GraphEdges);
            truckPathfindingGraph = new Graph(
                new Edge[]
                {
                    new Edge(Constants.TruckSpawn, Constants.TruckStop),
                    new Edge(Constants.TruckStop, Constants.TruckDespawn)
                }
            );

            robotPathfindingGraph.IntegrateVerticesToNearestEdge(
                new List<Vector3>() {
                    Constants.RobotEnterTruck,
                    Constants.RobotExitTruck
                }, 
            0);

            worldObjects.Add(new GraphDisplay(this, robotPathfindingGraph));

            foreach (Vector3 s in Constants.RobotSpawns)
                CreateRobot(s);
        }

        public bool RunTask(SimulationTask<World> task)
        {
            if (this._simulationTask != null && !this._simulationTask.IsFinished) return false;
            this._simulationTask = task;
            return true;
        }

        internal void SendUpdate()
        {
            int numUpdates = 0;
            int numEntities = worldObjects.Count;

            foreach (Entity e in worldObjects)
            {
                if (e.DiscardRequested()) SendCommandToObservers(new DiscardModel3DCommand(e.Guid));
                else if (e.NeedsUpdate())
                {
                    SendCommandToObservers(new UpdateModel3DCommand(e));
                    numUpdates++;
                }
            }

            // Marked entities can now safely be discarded.
            this.worldObjects.RemoveAll((e) => e.DiscardRequested());
        }

        internal IEnumerable<CargoSlot> GetOccupiedStoragePlotCargoSlots()
            => ObjectsOfType<StoragePlot>().SelectMany(plot => plot.OccupiedCargoSlots).ToList();

        private Robot CreateRobot(Vector3 s)
            => CreateRobot(s.X, s.Y, s.Z);
        
        public Robot CreateRobot(float x, float y, float z)
        {
            Robot r = new Robot(this,x,y,z,0,0,0,robotPathfindingGraph);
            worldObjects.Add(r);
            return r;
        }

        public Truck CreateTruck(Vector3 pos, bool cargo) 
            => CreateTruck(pos.X, pos.Y, pos.Z, cargo);

        public Truck CreateTruck(float x, float y, float z, bool cargo)
        {
            Truck t = new Truck(this,x,y,z,0,0,0,cargo);
            worldObjects.Add(t);

            return t;
        }

        public StoragePlot CreateStoragePlot(float x, float y, float z, int length, int width)
        {
            StoragePlot p = new StoragePlot(this, length, width, x, y, z, 0, 0, 0);
            worldObjects.Add(p);
            return p;
        }

        public Shelf CreateShelf(float x = 0f, float y = 0f, float z = 0f)
        {
            Shelf s = new Shelf(this, x, y, z);
            worldObjects.Add(s);
            return s;
        }

        public List<CollidablePathfindingEntity> GetCollisions(CollidablePathfindingEntity entity)
            => ObjectsOfType<CollidablePathfindingEntity>().Where(ce => 
                entity.Guid != ce.Guid 
                && entity.CheckCollision(ce.Intersectable)
            ).ToList();

        public void DestroyObject(Entity e)
            => e.Destroy();
        
        public List<CargoSlot> GetFreeStoragePlotCargoSlots()
            => ObjectsOfType<StoragePlot>().SelectMany(plot => plot.FreeCargoSlots).ToList();

        public IDisposable Subscribe(IObserver<UICommand> observer)
        {
            if (!observers.Contains(observer))
            {
                observers.Add(observer);
                SendCreationCommandsToObserver(observer);
            }
            return new Unsubscriber<UICommand>(observers, observer);
        }

        private void SendCommandToObservers(UICommand c)
            => observers.ForEach(o => o.OnNext(c));

        private void SendCreationCommandsToObserver(IObserver<UICommand> obs)
            => worldObjects.ForEach(o => obs.OnNext(new UpdateModel3DCommand(o)));

        public bool NeedsUpdate()
            => worldObjects.Where(e => e is IUpdatable).Any(e => (e as IUpdatable).NeedsUpdate());

        public bool Tick(int tick)
        {
            // Run current simulation task if any is available
            if(this._simulationTask != null && this._simulationTask.Tick()) this._simulationTask = null;
            foreach (Entity o in worldObjects) o.Tick(tick);
            return true;
        }

        public void Destroy()
        {
            Objects.ForEach(o => o.Destroy());
        }
    }

    internal class Unsubscriber<Command> : IDisposable
    {
        private List<IObserver<Command>> _observers;
        private IObserver<Command> _observer;

        internal Unsubscriber(List<IObserver<Command>> observers, IObserver<Command> observer)
        {
            this._observers = observers;
            this._observer = observer;
        }

        public void Dispose() 
        {
            if (_observers.Contains(_observer))
                _observers.Remove(_observer);
        }
    }
}