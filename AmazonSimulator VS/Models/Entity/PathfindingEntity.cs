using AmazonSimulator_VS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Models {
    public abstract class PathfindingEntity : Entity
    {
        private List<Vector3> _route = new List<Vector3>();

        private float _acceleration;
        private float _maxMovementSpeed;
        protected float _velocity = 0;

        private float _rotationSpeed;

        private int _currentWaypoint;

        public float MovementSpeed => _maxMovementSpeed; // In units per second.
        public float RotationSpeed => _rotationSpeed;
        public float Velocity => _velocity;
        public List<Vector3> Route => _route; 
        public Vector3? Destination
            => _route.Count > 0 
            ? _route.Last() 
            : Position;

        public PathfindingEntity(string type, EntityEnvironmentInfoProvider parent, float x, float y, float z, float rotationX, float rotationY, float rotationZ, float movementPerSecond, float rotationPerSecond, float accelerationPerSecond)
            : base(type, parent, x, y, z, rotationX, rotationY, rotationZ)
        {
            this._maxMovementSpeed = movementPerSecond / Constants.SIM_TPS;
            this._rotationSpeed = rotationPerSecond / Constants.SIM_TPS;
            this._acceleration = accelerationPerSecond / Constants.SIM_TPS;
        }

        public void SetPathfindingTarget(Vector3 target)
        {
            this._route.Clear();
            this._route.Add(target);

            this._currentWaypoint = 0;
        }

        public void SetPathfindingTarget(Vector3 target, Graph g)
        {
            this._route.Clear();

            Node firstOnGraph = g.NearestNodeTo(Position);
            Node lastOnGraph = g.NearestNodeTo(target);

            List<Node> dr = g.DijkstraShortestPath(firstOnGraph, lastOnGraph);
            dr = dr.Distinct().ToList(); // TODO: Graph.DijkstraShortestPath() shouldn't return any duplicates...

            List<Vector3> routeRight = new List<Vector3>();

            if (!g.ImpliesNodeAt(Position)) // If current position is not present on graph ...
                dr = dr.Prepend(new Node(Position, 0)).ToList(); // ... manually add it to the route

            if (!g.ImpliesNodeAt(target)) // If target position is not present on graph ...
                dr = dr.Append(new Node(target, 0)).ToList(); // ... manually add it to the route

            for (int i = 0; i < dr.Count; i++)
            {
                Vector3 pOff = new Vector3(0, 0, 0);
                Vector3 nOff = new Vector3(0, 0, 0);

                Node a, b, c;
                b = dr[i];

                if (i > 0)
                {
                    a = dr[i - 1];
                    Edge pEdge = new Edge(a, b, Math.Min(a.Width, b.Width));
                    Vector3 pDir = pEdge.Direction;
                    Vector3 pRight = Vector3.Transform(pDir, Matrix4x4.CreateRotationY((float)(-0.5 * Math.PI)));
                    pOff = (pRight * pEdge.Width * 0.5f);
                }

                if(i+1 < dr.Count)
                {
                    c = dr[i + 1];
                    Edge nEdge = new Edge(b, c, Math.Min(b.Width, c.Width));
                    Vector3 nDir = nEdge.Direction;
                    Vector3 nRight = Vector3.Transform(nDir, Matrix4x4.CreateRotationY((float)(-0.5 * Math.PI)));
                    nOff = (nRight * nEdge.Width * 0.5f);
                }

                Vector3 cOff = pOff + nOff;
                Vector3 off = new Vector3(
                    Math.Clamp(cOff.X, Math.Min(pOff.X, nOff.X), Math.Max(pOff.X, nOff.X)),
                    Math.Clamp(cOff.Y, Math.Min(pOff.Y, nOff.Y), Math.Max(pOff.Y, nOff.Y)),
                    Math.Clamp(cOff.Z, Math.Min(pOff.Z, nOff.Z), Math.Max(pOff.Z, nOff.Z))
                );
                Node wpAdjusted = new Node(b.Position + off);

                routeRight.Add(wpAdjusted);
            }
            routeRight.Add(dr.Last());

            // Find the rest of the waypoints used for traversing Graph 'g'
            this._route.AddRange(routeRight);
            this._currentWaypoint = 0;
        }

        public Vector3 GetCurrentWaypoint(bool MaintainRight = false)
            => (this._route.Count == 0 || _currentWaypoint == -1) 
            ? this.Position
            : this._route[_currentWaypoint];

        public Vector3 NextWaypoint()
        {
            _currentWaypoint++;

            if(_currentWaypoint == _route.Count)
            {
                _route.Clear();
                _currentWaypoint = -1;
            }

            return GetCurrentWaypoint();
        }        

        public bool IsAtDestination()
            => this.Destination.Equals(this.Position);

        public override bool Tick(int tick)
        {
            Vector3 target = this.GetCurrentWaypoint();

            if (this.Position.Equals(Destination)) return _needsUpdate; // Entity has reached its final pathfinding waypoint
            else // Move entity proportional to its movementspeed
            {
                if (!target.Equals(this.Position))
                {
                    var tDir = Vector3.Normalize(target - this.Position);
                    var cDir = Forward;

                    var cross2 = Vector3.Cross(tDir, cDir);

                    float pi = (float)Math.PI * 0.5f;
                    float deltaRY = Math.Min(Math.Max(cross2.Y * pi, -RotationSpeed), RotationSpeed);

                    this.Rotate(this.RotationX, this.RotationY + deltaRY, this.RotationZ);

                    if (cross2.Length() > 0.001)
                    {
                        _velocity = 0f;
                        return _needsUpdate;
                    }
                }

                // Accelerate velocity by acceleration if top speed has not been reached
                this._velocity += Math.Min(this._acceleration, this._maxMovementSpeed - this._velocity);

                float distance = this._velocity;
                if ((target-this.Position).Length() < this._velocity)
                {   // If Entity's distance moved this tick exceeds the distance to the next waypoint ...
                    Move((float)target.X, (float)target.Y, (float)target.Z); // ... move to next waypoint ...
                    distance -= (target - this.Position).Length(); // ... calculate remaining distance to move ...
                    target = NextWaypoint(); // ... and cycle to the next waypoint.
                }

                if (target.Equals(this.Position))// Entity has reached its destination waypoint, no need to move any further
                    return _needsUpdate;
                
                // Move entity over the vector spanned between target position and current position with length == distance
                Move(this.Position + Vector3.Normalize(target - this.Position) * distance);
            }

            // Base class (Entity) determines whether or not a gfx update will be required
            return _needsUpdate;
        }
    }
}