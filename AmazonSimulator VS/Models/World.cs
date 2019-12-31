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


            graph = new Graph(vertices, edges);
            //r.Move(4.6, 0, 13);
            r.setPathfindingTarget(E, graph);
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
                        Vector3 target = graph.vertices[random.Next(0, graph.vertices.Count - 1)];

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