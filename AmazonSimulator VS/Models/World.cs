using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Numerics;
using Controllers;

namespace Models
{
    public class World : IObservable<UICommand>, IUpdatable, EntityEnvironmentInfoProvider
    {
        private List<Entity> worldObjects = new List<Entity>();
        private List<IObserver<UICommand>> observers = new List<IObserver<UICommand>>();
        private SimulationTask<World> _simulationTask;

        private Graph robotPathfindingGraph;
        private Graph truckPathfindingGraph;

        public Graph RobotGraph { get => robotPathfindingGraph; }
        public Graph TruckGraph { get => truckPathfindingGraph; }

        public Node RobotQueueStart => Graph.NearestExplicitNodeTo(POI.RobotEnterTruck, RobotGraph); 
        public Node RobotTruckExit => Graph.NearestExplicitNodeTo(POI.RobotExitTruck, RobotGraph);

        public struct PointsOfInterest
        {
            public List<Vector3> RobotSpawns;

            public Vector3 TruckSpawn;
            public Vector3 TruckStop;
            public Vector3 TruckDespawn;

            public Vector3 RobotEnterTruck;
            public Vector3 RobotExitTruck;

            public PointsOfInterest(List<Vector3> RobotSpawns, Vector3 TruckSpawn, Vector3 TruckStop, Vector3 TruckDespawn, Vector3 RobotInteractTruck, Vector3 RobotExitTruck)
            {
                this.RobotSpawns = RobotSpawns;
                this.TruckSpawn = TruckSpawn;
                this.TruckStop = TruckStop;
                this.TruckDespawn = TruckDespawn;
                this.RobotEnterTruck = RobotInteractTruck;
                this.RobotExitTruck = RobotExitTruck;
            }
        }

        public PointsOfInterest POI;

        public World()
        {
            this._simulationTask = new EmptyTask<World>(this);

            POI = new PointsOfInterest(
                new List<Vector3>() { new Vector3(0f, 0f, 0f), new Vector3(1f, 0f, 0f), new Vector3(2f, 0f, 0f), new Vector3(3f, 0f, 0f), new Vector3(4f, 0f, 0f)/* */},

                new Vector3(-50f, 0f, 25f),
                new Vector3(0f, 0f, 25f),
                new Vector3(50f, 0f, 25f),

                new Vector3(-6.5f, 0f, 25f),
                new Vector3(-5.0f, 0f, 25f)
            );

            CreateStoragePlot(-4f, 0.01f, 2.5f, 5, 2);
            CreateStoragePlot(+4f, 0.01f, 2.5f, 5, 2);

            CreateStoragePlot(-4f, 0.01f, 7.5f, 5, 2);
            CreateStoragePlot(+4f, 0.01f, 7.5f, 5, 2);

            CreateStoragePlot(-4f, 0.01f, 12.5f, 5, 2);
            CreateStoragePlot(+4f, 0.01f, 12.5f, 5, 2);

            Vector3 A = new Vector3(-6.5f, 0.0f, 15.0f);
            Vector3 B = new Vector3(+0.0f, 0.0f, 15.0f);
            Vector3 C = new Vector3(+6.5f, 0.0f, 15.0f);

            Vector3 D = new Vector3(-6.5f, 0.0f, 10.0f);
            Vector3 E = new Vector3(+0.0f, 0.0f, 10.0f);
            Vector3 F = new Vector3(+6.5f, 0.0f, 10.0f);

            Vector3 G = new Vector3(-6.5f, 0.0f, 5.0f);
            Vector3 H = new Vector3(+0.0f, 0.0f, 5.0f);
            Vector3 I = new Vector3(+6.5f, 0.0f, 5.0f);

            Vector3 J = new Vector3(-6.5f, 0.0f, 0.0f);
            Vector3 K = new Vector3(+0.0f, 0.0f, 0.0f);
            Vector3 L = new Vector3(+6.5f, 0.0f, 0.0f);

            float laneWidth = 2.0f;

            Edge AB = new Edge(A, B, laneWidth);
            Edge BC = new Edge(B, C, laneWidth);

            Edge BE = new Edge(B, E, laneWidth);
            Edge DE = new Edge(D, E, laneWidth);

            Edge EF = new Edge(E, F, laneWidth);
            Edge EH = new Edge(E, H, laneWidth);

            Edge GH = new Edge(G, H, laneWidth);
            Edge HI = new Edge(H, I, laneWidth);

            Edge HK = new Edge(H, K, laneWidth);
            Edge JK = new Edge(J, K, laneWidth);
            Edge KL = new Edge(K, L, laneWidth);

            List<Vector3> vertices = new List<Vector3>()
            {
                A, B, C, D, E, F, G, H, I, J, K, L
            };

            List<Edge> edges = new List<Edge>()
            {
                AB, BC, BE, DE, EF, EH, GH, HI, HK, JK, KL
            };

            robotPathfindingGraph = new Graph(edges);

            truckPathfindingGraph = new Graph(
                new List<Edge>(){ new Edge(POI.TruckSpawn, POI.TruckStop), new Edge(POI.TruckStop, POI.TruckDespawn) }
            );

            List<Vector3> storagePositions = new List<Vector3>();
            this.GetObjectsOfType<StoragePlot>().ForEach((p) => p.CargoSlots.ForEach(s => storagePositions.Add(s.PositionAbsolute)));

            //robotPathfindingGraph.IntegrateVerticesToNearestEdge(storagePositions);

            robotPathfindingGraph.IntegrateVerticesToNearestEdge(new List<Vector3>() { POI.RobotEnterTruck, POI.RobotExitTruck }, 0);

            worldObjects.Add(new GraphDisplay(this, robotPathfindingGraph));

            foreach (Vector3 s in POI.RobotSpawns) CreateRobot(s);

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
                    e.needsUpdate = false;
                }
            }

            // Marked entities can now safely be discarded.
            this.worldObjects.RemoveAll((e) => e.DiscardRequested());
        }

        private Robot CreateRobot(Vector3 s)
        {
            return CreateRobot(s.X, s.Y, s.Z);
        }
        public Robot CreateRobot(float x, float y, float z)
        {
            Robot r = new Robot(this,x,y,z,0,0,0,robotPathfindingGraph);
            worldObjects.Add(r);
            return r;
        }
        public Truck CreateTruck(Vector3 pos) => CreateTruck(pos.X, pos.Y, pos.Z);
        public Truck CreateTruck(float x, float y, float z)
        {
            Truck t = new Truck(this,x,y,z,0,0,0);
            foreach(CargoSlot s in t.CargoSlots) s.SetCargo(CreateShelf());
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

        internal void DestroyObject(Entity e)
        {
            e.Destroy();
            // TODO: Implement delete command for view
        }

        public List<CargoSlot> GetFreeStoragePlotCargoSlots()
        {
            List<CargoSlot> available = new List<CargoSlot>();
            foreach(StoragePlot plot in GetObjectsOfType<StoragePlot>())
            {
                available.AddRange(plot.FreeCargoSlots);
            }
            return available;
        }

        public IDisposable Subscribe(IObserver<UICommand> observer)
        {
            if (!observers.Contains(observer)) {
                observers.Add(observer);

                SendCreationCommandsToObserver(observer);
            }
            return new Unsubscriber<UICommand>(observers, observer);
        }

        public List<Entity> GetObjects()
        {
            return this.worldObjects;
        }

        public List<T> GetObjectsOfType<T>() where T : Entity
        {
            List<T> obs = new List<T>();

            foreach (Entity e in GetObjects())
                if (e is T) obs.Add(e as T);

            return obs;
        }

        private async void SendCommandToObservers(UICommand c) {
            for(int i = 0; i < this.observers.Count; i++) {
                this.observers[i].OnNext(c);
            }
        }

        private void SendCreationCommandsToObserver(IObserver<UICommand> obs) {
            foreach(Entity m3d in worldObjects) {
                obs.OnNext(new UpdateModel3DCommand(m3d));
            }
        }

        public bool Tick(int tick)
        {
            int threads = Process.GetCurrentProcess().Threads.Count;
            // Run current simulation task if any is available
            if (this._simulationTask != null)
            {
                if(this._simulationTask.Tick())
                {
                    Console.WriteLine("Task finished running after tick #{0}", this._simulationTask.TickRuntime);
                    this._simulationTask = null;
                }
            }
            foreach (Entity o in worldObjects)
            {
                o.Tick(tick);
            }
            return true;
        }

        public bool NeedsUpdate()
        {   // If any child object needs an update, so does the world
            foreach (IUpdatable u in worldObjects.Where(e => e is IUpdatable).Select(e => e as IUpdatable))
                if (u.NeedsUpdate()) return true;

            return false;
        }

        public List<CollidablePathfindingEntity> GetCollisions(CollidablePathfindingEntity entity)
        {
            List<CollidablePathfindingEntity> collisions = new List<CollidablePathfindingEntity>();

            foreach (var ce in GetObjectsOfType<CollidablePathfindingEntity>())
                if (entity.Guid != ce.Guid && entity.CheckCollision(ce.Intersectable))
                    collisions.Add(ce);
                
            return collisions;
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