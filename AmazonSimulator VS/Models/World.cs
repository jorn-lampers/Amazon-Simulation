using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Controllers;

namespace Models {
    public class World : IObservable<Command>, IUpdatable
    {
        private List<Entity> worldObjects = new List<Entity>();
        private List<IObserver<Command>> observers = new List<IObserver<Command>>();

        private Graph graph;
        
        public World() {
            PathfindingEntity r = CreateRobot(0,0,0);

            Vector3 sw = new Vector3(-10.0f, 0.0f, -10.0f);
            Vector3 nw = new Vector3(-10.0f, 0.0f, 10.0f);
            Vector3 se = new Vector3(10.0f, 0.0f, -10.0f);
            Vector3 ne = new Vector3(10.0f, 0.0f, 10.0f);
            Vector3 c = new Vector3(0.0f, 0.0f, 0.0f);

            Edge n = new Edge(nw, ne);
            Edge e = new Edge(ne, se);
            Edge s = new Edge(se, sw);
            Edge w = new Edge(nw, sw);

            Edge csw = new Edge(sw, c);
            Edge cnw = new Edge(nw, c);
            Edge cse = new Edge(se, c);
            Edge cne = new Edge(ne, c);


            List<Vector3> vertices = new List<Vector3>();
            vertices.Add(sw);
            vertices.Add(se);
            vertices.Add(nw);
            vertices.Add(ne);
            vertices.Add(c);

            List<Edge> edges = new List<Edge>();
            edges.Add(n);
            //edges.Add(e);
            edges.Add(s);
            edges.Add(w);

            //edges.Add(csw);
            //edges.Add(cnw);
            //edges.Add(cse);
            edges.Add(cne);

            graph = new Graph(vertices, edges);

            //r.Move(4.6, 0, 13);
            r.setPathfindingTarget(nw, graph);
        }

        private PathfindingEntity CreateRobot(float x, float y, float z)
        {
            Robot r = new Robot(x,y,z,0,0,0);
            worldObjects.Add(r);
            return r;
        }

        public IDisposable Subscribe(IObserver<Command> observer)
        {
            if (!observers.Contains(observer)) {
                observers.Add(observer);

                SendCreationCommandsToObserver(observer);
            }
            return new Unsubscriber<Command>(observers, observer);
        }

        private void SendCommandToObservers(Command c) {
            for(int i = 0; i < this.observers.Count; i++) {
                this.observers[i].OnNext(c);
            }
        }

        private void SendCreationCommandsToObserver(IObserver<Command> obs) {
            foreach(Entity m3d in worldObjects) {
                obs.OnNext(new UpdateModel3DCommand(m3d));
            }
        }

        public bool Update(int tick)
        {
            for(int i = 0; i < worldObjects.Count; i++) {
                Entity u = worldObjects[i];

                if(u is PathfindingEntity)
                {
                    PathfindingEntity r = (PathfindingEntity)u;
                    if (r.isAtDestination())
                    {
                        Random random = new Random();
                        Vector3 target = graph.findNearestVertex(new Vector3((float)(random.NextDouble() * 15 - 7.5), 0.0f, (float)(random.NextDouble() * 15 - 7.5)));
                        r.setPathfindingTarget(target, graph);
                        Console.WriteLine("No. of waypoints: " + r.route.Count());
                    }
                }

                if(u is IUpdatable) {
                    bool needsCommand = ((IUpdatable)u).Update(tick);

                    if(needsCommand) {
                        SendCommandToObservers(new UpdateModel3DCommand(u));
                    }
                }
            }

            return true;
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