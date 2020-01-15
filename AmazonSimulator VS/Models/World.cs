using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Controllers;

namespace Models {
    public class World : IObservable<UICommand>, IUpdatable
    {
        private List<Entity> worldObjects = new List<Entity>();
        private List<IObserver<UICommand>> observers = new List<IObserver<UICommand>>();

        private Graph robotPathfindingGraph;
        public Target tr = new Target(0, 0, 0);

        public Graph RobotGraph { get => robotPathfindingGraph; }

        public World() {
            worldObjects.Add(tr);

            Robot r = CreateRobot(0,0,0);

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

            Edge AB = new Edge(A, B);
            Edge BC = new Edge(B, C);

            Edge BE = new Edge(B, E);
            Edge DE = new Edge(D, E);

            Edge EF = new Edge(E, F);
            Edge EH = new Edge(E, H);

            Edge GH = new Edge(G, H);
            Edge HI = new Edge(H, I);

            Edge HK = new Edge(H, K);
            Edge JK = new Edge(J, K);
            Edge KL = new Edge(K, L);

            List<Vector3> vertices = new List<Vector3>()
            {
                A, B, C, D, E, F, G, H, I, J, K, L
            };

            List<Edge> edges = new List<Edge>()
            {
                AB, BC, BE, DE, EF, EH, GH, HI, HK, JK, KL
            };

            robotPathfindingGraph = new Graph(vertices, edges);

            foreach (StoragePlot p in GetObjectsOfType<StoragePlot>())
                robotPathfindingGraph.IntegrateVerticesToNearestEdge(p.StoragePositionsInWorld);

            worldObjects.Add(new GraphDisplay(robotPathfindingGraph));


            Shelf s = CreateShelf(0.0f, 0.0f, 0.0f);
            r.attachShelf(s);
        }

        internal void SendUpdate()
        {
            int numUpdates = 0;
            int numEntities = worldObjects.Count;

            foreach (Entity e in worldObjects)
            {
                if (e.NeedsUpdate())
                {
                    SendCommandToObservers(new UpdateModel3DCommand(e));
                    numUpdates++;
                    e.needsUpdate = false;
                }
            }
        }

        public Robot CreateRobot(float x, float y, float z)
        {
            Robot r = new Robot(x,y,z,0,0,0);
            worldObjects.Add(r);
            return r;
        }

        public StoragePlot CreateStoragePlot(float x, float y, float z, int length, int width)
        {
            StoragePlot p = new StoragePlot(length, width, x, y, z, 0, 0, 0);
            worldObjects.Add(p);
            return p;
        }

        public Shelf CreateShelf(float x, float y, float z)
        {
            Shelf s = new Shelf(x, y, z);
            worldObjects.Add(s);
            return s;
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

        private void SendCommandToObservers(UICommand c) {
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
            for(int i = 0; i < worldObjects.Count; i++) worldObjects[i].Tick(tick);
            
            return true;
        }

        /*
        public bool Tick(int tick)
        {
            for (int i = 0; i < worldObjects.Count; i++)
            {
                Entity u = worldObjects[i];
                u.Tick(tick);

                if (u is PathfindingEntity)
                {
                    PathfindingEntity r = (PathfindingEntity)u;
                    if (r.isAtDestination())
                    {
                        Random random = new Random();
                        Vector3 target = graph.vertices[random.Next(0, graph.vertices.Count - 1)];
                        tr.Move(target);

                        r.setPathfindingTarget(target, graph);
                    }
                }
            }

            return true;
        }
        */
        public bool NeedsUpdate()
        {   // If any child object needs an update, so does the world
            foreach (IUpdatable u in worldObjects.Where(e => e is IUpdatable).Select(e => e as IUpdatable))
                if (u.NeedsUpdate()) return true;

            return false;
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