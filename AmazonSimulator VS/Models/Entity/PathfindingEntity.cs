using AmazonSimulator_VS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace Models {

    public abstract class PathfindingEntity : DisplacingEntity
    {
        private List<Vector3> _route = new List<Vector3>();
        private int _currentWaypoint;

        private bool _brake;
        protected float _acceleration;

        public List<Vector3> Route => _route;
        public bool Brake => _brake;
        public Vector3 Destination
            => _route.Count > 0
            ? _route.Last()
            : _target;

        public PathfindingEntity(string type, EntityEnvironmentInfoProvider parent, float x, float y, float z, float rotationX, float rotationY, float rotationZ, float movementPerSecond, float rotationPerSecond, float accelerationPerSecond)
            : base(type, parent, x, y, z, rotationX, rotationY, rotationZ, movementPerSecond, rotationPerSecond)
        {
            this._acceleration = accelerationPerSecond / Constants.SIM_TPS;
            this._brake = false;
        }

        public void ClearPathfindingTarget()
        {
            this._route.Clear();
            this._currentWaypoint = -1;
        }

        public void SetPathfindingTarget(Vector3 target, Graph g)
        {
            this._route.Clear();
            this._route.Add(target);

            this._currentWaypoint = 0;

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

                if (i + 1 < dr.Count)
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
            if (_currentWaypoint == -1) return _target;

            _currentWaypoint++;

            if (_currentWaypoint == _route.Count)
            {
                _route.Clear();
                _currentWaypoint = -1;
            }

            SetTarget(GetCurrentWaypoint());

            return _target;
        }

        public bool IsAtDestination()
            => this.Destination.Equals(this.Position);

        public int GetRequiredTicksToFullStop()
        {
            // Assuming linear decelleration and current velocity
            return (int)Math.Ceiling(_velocity / _acceleration);
        }

        public float GetRequiredDistanceToFullStop()
        {
            // Assuming linear decelleration and current velocity
            var ticks = GetRequiredTicksToFullStop();
            return this.Velocity * ticks / 2;
        }

        public bool Tick(int tick, bool brake)
        {
            var d = GetRequiredDistanceToFullStop();

            if (Vector3.Distance(_target, Position) < d)
                _brake = true;
            else _brake = brake;


            if (_brake) // Decellerate velocity by acceleration if result >= 0
                this._velocity = Math.Max(0, this._velocity - this._acceleration);
            else if(!IsAtTarget) // Accelerate velocity by acceleration if top speed has not been reached
                this._velocity = Math.Min(this._velocity + this._acceleration, this._maxMovementSpeed);

            base.Tick(tick);

            if (this.Position.Equals(Destination)) // Entity has reached its final pathfinding waypoint
                ClearPathfindingTarget();

            else if (_target.Equals(this.Position)) // Entity has reached a waypoint, cycle to the next waypoint
                NextWaypoint();

            return _needsUpdate;
        }

        public override bool Tick(int tick)
        {
            return Tick(tick, false);
        }

        public override void SetTarget(Vector3 target)
        {
            this._target = target;
        }
    }
}