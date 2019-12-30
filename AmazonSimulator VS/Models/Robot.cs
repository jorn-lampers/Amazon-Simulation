using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using Newtonsoft.Json;

namespace Models {
    public class Robot : Entity
    {
        static float SPEED = 0.05f;
        List<Vector3> _route;

        public List<Vector3> route { get { return _route; } }

        public Robot(float x, float y, float z, float rotationX, float rotationY, float rotationZ) : base("robot", x, y, z, rotationX, rotationY, rotationZ)
        {

        }

        public void setTarget(Vector3 t, Graph g)
        {
            Vector3 current = new Vector3((float)x, (float)y, (float)z);
            Vector3 nearest = g.findNearestVertex((float)x, (float)y, (float)z);

            _route = new List<Vector3>();

            if (!current.Equals(nearest)) _route.Add(nearest); // Move to nearest vertex on graph first if robot isn't there already // TODO: Skip this if robot is on any edge between vertices
            _route.AddRange(g.dijkstraShortestPath(nearest, t));

            needsUpdate = true;
        }

        public Vector3 getCurrentTarget()
        {
            if (route.Count == 0) return getVertexOnPos();
            else return route[0];
        }

        public void nextTarget()
        {
            _route.RemoveAt(0);
            Console.WriteLine("Next target: " + route.Count());
        }        

        public Vector3 getDestinationTarget()
        {
            if (_route == null || route.Count == 0) return getVertexOnPos();
            else return _route[_route.Count - 1];
        }

        public Vector3 getVertexOnPos()
        {
            return new Vector3(x, y, z);
        }

        public bool isWaiting()
        {
            Vector3 dest = getDestinationTarget();
            Vector3 curr = getVertexOnPos();
            float dist = Vector3.Distance(dest, curr);
            bool waiting = dist < 0.01;
            return waiting;
        }

        public override bool Update(int tick)
        {
            if (!base.Update(tick)) return false;

            Vector3 target = getCurrentTarget();
            Vector3 pos = getVertexOnPos();

            if (pos.Equals(getDestinationTarget()))
            {
                Console.WriteLine("Robot has reached its final destination.");
                return true;
            }
            else
            {
                float distance = SPEED;
                if ((target-pos).Length() < SPEED)
                {
                    Move((float)target.X, (float)target.Y, (float)target.Z);
                    distance -= (target-pos).Length();
                    nextTarget();
                }
                target = getCurrentTarget();
                pos = getVertexOnPos();
                if (target.Equals(pos))
                {
                    Console.WriteLine("Robot has reached its final destination.");
                    return true;
                }

                Vector3 dir = Vector3.Normalize(target - pos);
                pos += dir * distance;

                Move(pos);

                //Console.WriteLine("Pos: " + pos.X + ", " + pos.Z);
                //Console.WriteLine("Target: " + target.X + ", " + target.Z);

            }

            return true;
        }

    }
}