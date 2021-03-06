using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using AmazonSimulator_VS;
using Controllers;

namespace Models
{
    public class World 
        : IObservable<UICommand>
        , IUpdatable
        , EntityEnvironmentInfoProvider
    {
        /************************************START OF FIELDS************************************/
        private SimulationTask<World> _simulationTask;
        private List<Entity> _worldObjects = new List<Entity>();
        private List<IObserver<UICommand>> _observers = new List<IObserver<UICommand>>();

        private Graph _robotPathfindingGraph;
        private Graph _truckPathfindingGraph;
        /************************************ END OF FIELDS ************************************/


        /**********************************START OF PROPERTIES**********************************/
        public Graph RobotGraph => _robotPathfindingGraph; 
        public Graph TruckGraph => _truckPathfindingGraph;
        public List<Entity> Objects => this._worldObjects;
        /********************************** END OF PROPERTIES **********************************/

        public World()
        {
            this._simulationTask = null;

            foreach (Vector3 pos in Constants.StoragePositions)
                CreateStoragePlot(pos.X, pos.Y, pos.Z, 0, 0, 0, Constants.StoragePlotLength, Constants.StoragePlotWidth);

            _robotPathfindingGraph = new Graph(Constants.GraphEdges);
            _truckPathfindingGraph = new Graph(
                new Edge[]
                {
                    new Edge(Constants.TruckSpawn, Constants.TruckStop),
                    new Edge(Constants.TruckStop, Constants.TruckDespawn)
                }
            );

            _robotPathfindingGraph.IntegrateVerticesToNearestEdge(
                new List<Vector3>() {
                    Constants.RobotEnterTruck,
                    Constants.RobotExitTruck
                },
            Constants.LaneWidth);

            _worldObjects.Add(new GraphDisplay(this, _robotPathfindingGraph));

            foreach (Vector3 s in Constants.RobotSpawns)
                CreateRobot(s.X, s.Y, s.Z);
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
            int numEntities = _worldObjects.Count;

            foreach (Entity e in _worldObjects)
            {
                if (e.DiscardRequested()) SendCommandToObservers(new DiscardModel3DCommand(e.Guid));
                else if (e.NeedsUpdate())
                {
                    SendCommandToObservers(new UpdateModel3DCommand(e));
                    numUpdates++;
                }
            }

            // Marked entities can now safely be discarded.
            this._worldObjects.RemoveAll((e) => e.DiscardRequested());
        }

        public List<T> ObjectsOfType<T>() where T : Entity
        {
            return Objects
                .Where(e => e is T)
                .Select(et => et as T)
                .ToList();
        }
        
        public Robot CreateRobot(float x = 0f, float y = 0f, float z = 0f, float rx = 0f, float ry = 0f, float rz = 0f)
        {
            Robot r = new Robot(this,x,y,z,rx,ry,rz,_robotPathfindingGraph);
            _worldObjects.Add(r);
            return r;
        }

        public Truck CreateTruck(float x = 0f, float y = 0f, float z = 0f, float rx = 0f, float ry = 0f, float rz = 0f, int cargo = 0)
        {
            Truck t = new Truck(this,x,y,z, rx, ry, rz);
            _worldObjects.Add(t);

            if (cargo > t.CargoSlots.Count) cargo = t.CargoSlots.Count;

            for(int i = 0; i < cargo; i++)
                t.CargoSlots[i].SetCargo(CreateShelf());


            return t;
        }

        public StoragePlot CreateStoragePlot(float x, float y, float z, float rx = 0f, float ry = 0f, float rz = 0f, int length = 5, int width = 2)
        {
            StoragePlot p = new StoragePlot(this, length, width, x, y, z, rx, ry, rz);
            _worldObjects.Add(p);
            return p;
        }

        public Shelf CreateShelf(float x = 0f, float y = 0f, float z = 0f, float rx = 0f, float ry = 0f, float rz = 0f)
        {
            Shelf s = new Shelf(this, x, y, z, rx, ry, rz);
            _worldObjects.Add(s);
            return s;
        }

        public List<CollidablePathfindingEntity> GetCollisions(CollidablePathfindingEntity entity) 
            => ObjectsOfType<CollidablePathfindingEntity>().Where(ce => 
                entity.Guid != ce.Guid 
                && entity.CheckCollision(ce.Intersectable)
            ).ToList();

        public List<CollidablePathfindingEntity> GetCollisions(LineSegment2 path)
            => ObjectsOfType<CollidablePathfindingEntity>().Where(ce =>
               ce.Intersectable.Intersects(path)
            ).ToList();

        public List<CollidablePathfindingEntity> GetCollisions(ICollection<LineSegment2> path)
            => ObjectsOfType<CollidablePathfindingEntity>().Where(ce =>
               ce.Intersectable.Intersects(path)
            ).ToList();

        public IDisposable Subscribe(IObserver<UICommand> observer)
        {
            if (!_observers.Contains(observer))
            {
                _observers.Add(observer);
                SendCreationCommandsToObserver(observer);
            }
            return new Unsubscriber<UICommand>(_observers, observer);
        }

        private void SendCommandToObservers(UICommand c)
        {
            _observers.ForEach(o => o.OnNext(c));
        }

        private void SendCreationCommandsToObserver(IObserver<UICommand> obs)
        {
            _worldObjects.ForEach(o => obs.OnNext(new UpdateModel3DCommand(o)));
        }

        public bool NeedsUpdate(bool evaluateOnly = false)
        {
            return _worldObjects
                .Where(e => e is IUpdatable)
                .Any(e => (e as IUpdatable)
                .NeedsUpdate());
        }

        public bool Tick(int tick)
        {
            // Run current simulation task if any is available
            if(this._simulationTask != null && this._simulationTask.Tick())
                this._simulationTask = null;

            foreach (Entity o in _worldObjects)
                o.Tick(tick);

            return true;
        }

        public void Destroy()
        {
            Objects.ForEach(o => o.Destroy());
        }
    }
}